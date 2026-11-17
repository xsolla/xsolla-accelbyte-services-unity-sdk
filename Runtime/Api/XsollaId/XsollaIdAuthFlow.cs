// Copyright (c) 2026 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AccelByte.Core;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Xsolla.AccelByte
{
    internal sealed class XsollaIdTokens
    {
        public string IdToken;
        public string AccessToken;
        public string RefreshToken;
        public long ExpiresAt;
    }

    /// <summary>
    /// Runs the Xsolla ID OpenID Connect authorization code flow with PKCE for a desktop game (RFC 7636, RFC 8252):
    /// opens the system browser, receives the code on a loopback listener, exchanges it for tokens and checks the id_token claims.
    /// One instance runs one login. Start it on the main thread; the callbacks run on the main thread.
    /// </summary>
    internal sealed class XsollaIdAuthFlow
    {
        public const string PlatformUnsupportedCode = "xsolla-id-platform-unsupported";
        public const string ConfigInvalidCode = "xsolla-id-config-invalid";
        public const string ListenerFailedCode = "xsolla-id-listener-failed";
        public const string TimeoutCode = "xsolla-id-login-timeout";
        public const string StateMismatchCode = "xsolla-id-state-mismatch";
        public const string AuthorizationFailedCode = "xsolla-id-authorization-failed";
        public const string TokenExchangeFailedCode = "xsolla-id-token-exchange-failed";
        public const string TokenInvalidCode = "xsolla-id-token-invalid";
        public const string InternalErrorCode = "xsolla-id-internal-error";

        // The token request is not retried: the authorization code works only once (RFC 6749 section 4.1.2).
        private const int TokenRequestTimeoutSeconds = 30;

        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        private XsollaIdOptions options;
        private Action<XsollaIdTokens> onSuccess;
        private Action<string, string> onFailed;
        private Action onCancelled;
        private XsollaIdLoopbackListener listener;
        private UnityWebRequest tokenRequest;
        private bool finished;

        public void Start(XsollaIdOptions inOptions, Action<XsollaIdTokens> inOnSuccess, Action<string, string> inOnFailed, Action inOnCancelled)
        {
            options = inOptions;
            onSuccess = inOnSuccess;
            onFailed = inOnFailed;
            onCancelled = inOnCancelled;

#if !(UNITY_STANDALONE || UNITY_EDITOR)
            Fail(PlatformUnsupportedCode, "Xsolla ID browser login is only supported on Windows, Mac and Linux. Use LoginWithXsollaIdToken instead.");
#else
            _ = RunSafelyAsync();
#endif
        }

        /// <summary>Stops the flow and calls onCancelled. The flow cannot detect a closed browser tab, so the game calls this instead.</summary>
        public void Cancel()
        {
            if (finished)
            {
                return;
            }

            Action callback = onCancelled;
            Finish();
            callback?.Invoke();
        }

        // Start does not await the flow, so an unexpected exception would otherwise be lost and the callback would never run.
        private async Task RunSafelyAsync()
        {
            try
            {
                await RunAsync();
            }
            catch (Exception exception)
            {
                Fail(InternalErrorCode, exception.Message);
            }
        }

        private async Task RunAsync()
        {
            if (options == null || string.IsNullOrEmpty(options.ClientId) || string.IsNullOrEmpty(options.Issuer))
            {
                Fail(ConfigInvalidCode, "Set ClientId and Issuer in XsollaIdOptions.");
                return;
            }
            // The token request carries the PKCE code_verifier and returns the tokens, so it must not go over plain HTTP.
            if (!Uri.TryCreate(options.Issuer, UriKind.Absolute, out Uri issuerUri) || issuerUri.Scheme != Uri.UriSchemeHttps)
            {
                Fail(ConfigInvalidCode, "XsollaIdOptions.Issuer must be an https:// URL.");
                return;
            }
            if (options.RedirectPort < 0 || options.RedirectPort > 65535)
            {
                Fail(ConfigInvalidCode, "XsollaIdOptions.RedirectPort must be between 0 and 65535. Use 0 for any free port.");
                return;
            }
            if (options.Scope == null || !options.Scope.Contains("openid"))
            {
                Fail(ConfigInvalidCode, "XsollaIdOptions.Scope must contain openid, otherwise Xsolla ID does not return an id_token.");
                return;
            }

            string redirectHost = string.IsNullOrEmpty(options.RedirectHost) ? "127.0.0.1" : options.RedirectHost;
            // The listener binds the IPv4 loopback address, so the redirect must resolve to it.
            if (redirectHost != "127.0.0.1" && redirectHost != "localhost")
            {
                Fail(ConfigInvalidCode, "XsollaIdOptions.RedirectHost must be 127.0.0.1 or localhost.");
                return;
            }

            string redirectPath = string.IsNullOrEmpty(options.RedirectPath) ? "/callback" : options.RedirectPath;
            if (!redirectPath.StartsWith("/"))
            {
                redirectPath = "/" + redirectPath;
            }

            string codeVerifier = XsollaIdProtocol.CreateRandomValue();
            string state = XsollaIdProtocol.CreateRandomValue();
            string nonce = XsollaIdProtocol.CreateRandomValue();

            listener = new XsollaIdLoopbackListener();
            try
            {
                listener.Start(options.RedirectPort, redirectPath, state);
            }
            catch (SocketException exception)
            {
                Fail(ListenerFailedCode, $"Failed to listen on 127.0.0.1:{options.RedirectPort}. The port may be in use. {exception.Message}");
                return;
            }

            string redirectUri = XsollaIdProtocol.BuildRedirectUri(redirectHost, listener.Port, redirectPath);
            Application.OpenURL(XsollaIdProtocol.BuildAuthorizeUrl(options.Issuer, options.ClientId, redirectUri, options.Scope,
                state, nonce, XsollaIdProtocol.CreateCodeChallenge(codeVerifier)));

            Dictionary<string, string> parameters;
            try
            {
                Task<Dictionary<string, string>> callbackTask = listener.WaitForCallbackAsync();
                Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(Math.Max(1, options.LoginTimeoutSeconds)), cancellation.Token);
                if (await Task.WhenAny(callbackTask, timeoutTask) != callbackTask)
                {
                    Fail(TimeoutCode, "The Xsolla ID login was not completed in time.");
                    return;
                }
                parameters = await callbackTask;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exception)
            {
                Fail(ListenerFailedCode, exception.Message);
                return;
            }

            if (finished)
            {
                return;
            }
            listener.Dispose();

            // The listener already ignores callbacks with another state; check again before reading anything else (RFC 6749 section 10.12).
            if (!parameters.TryGetValue("state", out string returnedState) || returnedState != state)
            {
                Fail(StateMismatchCode, "The login callback does not belong to this login request.");
                return;
            }
            if (parameters.TryGetValue("error", out string error))
            {
                if (error == "access_denied")
                {
                    Cancel();
                    return;
                }
                parameters.TryGetValue("error_description", out string errorDescription);
                Fail(AuthorizationFailedCode, $"{error}: {errorDescription}");
                return;
            }
            if (!parameters.TryGetValue("code", out string code) || string.IsNullOrEmpty(code))
            {
                Fail(AuthorizationFailedCode, "The login callback has no authorization code.");
                return;
            }

            await ExchangeCodeAsync(code, redirectUri, codeVerifier, nonce);
        }

        private async Task ExchangeCodeAsync(string code, string redirectUri, string codeVerifier, string nonce)
        {
            tokenRequest = UnityWebRequest.Post(XsollaIdProtocol.BuildTokenEndpoint(options.Issuer),
                XsollaIdProtocol.BuildTokenRequestForm(options.ClientId, code, redirectUri, codeVerifier));
            tokenRequest.SetRequestHeader("Accept", "application/json");
            tokenRequest.timeout = TokenRequestTimeoutSeconds;

            var completion = new TaskCompletionSource<bool>();
            tokenRequest.SendWebRequest().completed += _ => completion.TrySetResult(true);
            await completion.Task;

            if (finished)
            {
                return;
            }

            UnityWebRequest request = tokenRequest;
            tokenRequest = null;
            using (request)
            {
                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Fail(TokenExchangeFailedCode, $"Could not reach the Xsolla ID token endpoint. {request.error}");
                    return;
                }

                JObject body = null;
                try
                {
                    body = JObject.Parse(request.downloadHandler.text);
                }
                catch (Exception)
                {
                }

                if (request.responseCode != 200)
                {
                    Fail(TokenExchangeFailedCode, $"HTTP {request.responseCode} {(string)body?["error"]}: {(string)body?["error_description"]}");
                    return;
                }

                var tokens = new XsollaIdTokens
                {
                    IdToken = (string)body?["id_token"],
                    AccessToken = (string)body?["access_token"],
                    RefreshToken = (string)body?["refresh_token"]
                };
                if (string.IsNullOrEmpty(tokens.IdToken))
                {
                    Fail(TokenExchangeFailedCode, "The token response has no id_token. Check that XsollaIdOptions.Scope contains openid.");
                    return;
                }
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                JToken expiresIn = body?["expires_in"];
                if (expiresIn != null && expiresIn.Type == JTokenType.Integer)
                {
                    tokens.ExpiresAt = now + (long)expiresIn;
                }

                if (!XsollaIdProtocol.ValidateIdTokenClaims(tokens.IdToken, options.Issuer, options.ClientId, nonce, now, out string validationError))
                {
                    Fail(TokenInvalidCode, validationError);
                    return;
                }

                Action<XsollaIdTokens> callback = onSuccess;
                Finish();
                callback?.Invoke(tokens);
            }
        }

        private void Fail(string code, string description)
        {
            if (finished)
            {
                return;
            }

            AccelByteDebug.LogWarning($"Xsolla ID login failed: {code}. {description}");
            Action<string, string> callback = onFailed;
            Finish();
            callback?.Invoke(code, description);
        }

        private void Finish()
        {
            finished = true;
            onSuccess = null;
            onFailed = null;
            onCancelled = null;
            cancellation.Cancel();
            listener?.Dispose();
            if (tokenRequest != null)
            {
                tokenRequest.Abort();
                tokenRequest.Dispose();
                tokenRequest = null;
            }
        }
    }
}
