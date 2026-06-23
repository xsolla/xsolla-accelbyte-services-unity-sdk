// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using System;
using System.Reflection;
using AccelByte.Models;

namespace Xsolla.Backend
{
    /// <summary>
    /// Maps a <see cref="XsollaConfig"/> ScriptableObject into the AccelByte SDK
    /// override structures and writes them into the AccelByte SDK implementation
    /// so that subsequent registry creation picks them up.
    /// <para>
    /// The AccelByte override types (<c>MultiSDKConfigsArgs</c>,
    /// <c>SDKConfigArgs</c>, <c>OverrideConfigs</c>) and the
    /// <c>AccelByteSDK.Implementation</c> property are assembly-internal, so
    /// this mapper accesses them via reflection.
    /// </para>
    /// </summary>
    internal static class XsollaConfigMapper
    {
        // Cached reflection metadata for the internal AccelByte types.
        private static readonly Assembly AbSdkAssembly =
            typeof(AccelByte.Core.AccelByteSDK).Assembly;

        private static readonly Type MultiSDKConfigsArgsType =
            AbSdkAssembly.GetType("AccelByte.Models.MultiSDKConfigsArgs");

        private static readonly Type SDKConfigArgsType =
            AbSdkAssembly.GetType("AccelByte.Models.SDKConfigArgs");

        /// <summary>
        /// Maps the supplied <see cref="XsollaConfig"/> into AccelByte
        /// <c>OverrideConfigs</c> and writes them to
        /// <c>AccelByteSDK.Implementation.OverrideConfigs</c>.
        /// Only fields with non-empty values are set; all other fields remain
        /// <c>null</c> so that on-disk / default values are preserved.
        /// </summary>
        internal static void Apply(XsollaConfig config)
        {
            if (config == null)
            {
                return;
            }

            object sdkOverride = BuildSdkConfigOverride(config);
            MultiOAuthConfigs oauthOverride = BuildOAuthConfigOverride(config);

            WriteOverridesToImplementation(sdkOverride, oauthOverride);
        }

        // ──────────────────────────────────────────────
        //  SDK config (internal types -> reflection)
        // ──────────────────────────────────────────────

        private static object BuildSdkConfigOverride(XsollaConfig config)
        {
            // Create a single SDKConfigArgs with the Xsolla-provided values.
            object args = Activator.CreateInstance(SDKConfigArgsType);

            // Core fields – only set when the developer provided a value.
            SetStringField(args, "Namespace", config.Namespace);
            SetStringField(args, "BaseUrl", config.BaseUrl);
            SetStringField(args, "PublisherNamespace", config.PublisherNamespace);
            SetStringField(args, "RedirectUri", config.RedirectUri);

            // When a BaseUrl is provided, force all service URLs (IamServerUrl,
            // PlatformServerUrl, LobbyServerUrl, etc.) to be re-derived from it
            // via Config.Expand(true). Without this, IamServerUrl stays empty and
            // the AccelByte HTTP client resolves IAM paths against the base URI,
            // stripping the required /iam prefix from every IAM request.
            if (!string.IsNullOrEmpty(config.BaseUrl))
            {
                SetField(args, "OverrideServiceUrl", (bool?)true);
            }

            // TURN / relay fields.
            SetField(args, "UseTurnManager", (bool?)config.UseTurnManager);
            SetStringField(args, "TurnManagerServerUrl", config.TurnManagerServerUrl);
            SetStringField(args, "TurnServerHost", config.TurnServerHost);
            SetStringField(args, "TurnServerPort", config.TurnServerPort);
            SetStringField(args, "TurnServerUsername", config.TurnServerUsername);
            SetStringField(args, "TurnServerPassword", config.TurnServerPassword);
            SetStringField(args, "TurnServerSecret", config.TurnServerSecret);

            // Wrap into a MultiSDKConfigsArgs with the same values for every
            // environment so the override applies regardless of the active env.
            object multi = Activator.CreateInstance(MultiSDKConfigsArgsType);
            MultiSDKConfigsArgsType
                .GetMethod("SetConfigValueToAllEnv")
                .Invoke(multi, new[] { args });

            return multi;
        }

        // ──────────────────────────────────────────────
        //  OAuth config (public types -> direct access)
        // ──────────────────────────────────────────────

        private static MultiOAuthConfigs BuildOAuthConfigOverride(XsollaConfig config)
        {
            if (string.IsNullOrEmpty(config.ClientId))
            {
                return null;
            }

            var oauth = new OAuthConfig
            {
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret ?? string.Empty
            };

            var multi = new MultiOAuthConfigs();
            multi.SetConfigValueToAllEnv(oauth);
            return multi;
        }

        // ──────────────────────────────────────────────
        //  Write overrides into AccelByteSDK internals
        // ──────────────────────────────────────────────

        private static void WriteOverridesToImplementation(
            object sdkConfigOverride,
            MultiOAuthConfigs oauthConfigOverride)
        {
            // 1. Obtain AccelByteSDK.Implementation (internal static property).
            //    Accessing the getter also lazily creates the implementator.
            PropertyInfo implProp = typeof(AccelByte.Core.AccelByteSDK)
                .GetProperty("Implementation",
                    BindingFlags.Static | BindingFlags.NonPublic);

            object impl = implProp.GetValue(null);

            // 2. Get the OverrideConfigs field (internal value-type field on
            //    the AccelByteSDKImplementator instance).
            FieldInfo overrideField = impl.GetType()
                .GetField("OverrideConfigs",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            // 3. Box the struct, mutate the boxed copy, then write it back.
            object boxed = overrideField.GetValue(impl);
            Type overrideType = boxed.GetType();

            if (sdkConfigOverride != null)
            {
                overrideType
                    .GetField("SDKConfigOverride", BindingFlags.Public | BindingFlags.Instance)
                    .SetValue(boxed, sdkConfigOverride);
            }

            if (oauthConfigOverride != null)
            {
                overrideType
                    .GetField("OAuthConfigOverride", BindingFlags.Public | BindingFlags.Instance)
                    .SetValue(boxed, oauthConfigOverride);
            }

            overrideField.SetValue(impl, boxed);
        }

        // ──────────────────────────────────────────────
        //  Reflection helpers
        // ──────────────────────────────────────────────

        /// <summary>
        /// Sets a <see cref="string"/> field only when the value is non-null
        /// and non-empty, leaving the field <c>null</c> so that the AccelByte
        /// SDK falls back to its on-disk / default value.
        /// </summary>
        private static void SetStringField(object target, string fieldName, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            SetField(target, fieldName, value);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType()
                .GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);

            field?.SetValue(target, value);
        }
    }
}
