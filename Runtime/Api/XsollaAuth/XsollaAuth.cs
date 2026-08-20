// Copyright (c) 2025-2026 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using System;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;

namespace Xsolla.AccelByte
{
    [UnityEngine.Scripting.Preserve]
    public class XsollaAuth : IXsollaAccelByteAuth
    {
        private const string XsollaPlatformId = "xsolla";

        private readonly User user;

        [UnityEngine.Scripting.Preserve]
        public XsollaAuth()
            : this(AccelByteSDK.GetClientRegistry().GetApi().GetUser())
        {
        }

        [UnityEngine.Scripting.Preserve]
        public XsollaAuth(User user)
        {
            this.user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public void LoginWithXsollaAccount(ResultCallback<TokenData, OAuthError> callback)
        {
            System.Action<Xsolla.Core.Error> onError = error =>
            {
                callback?.TryError(ToOAuthError(error));
            };

            System.Action onCancel = () =>
            {
                callback?.TryError(new OAuthError
                {
                    error = ((int)ErrorCode.ExpectationFailed).ToString(),
                    error_description = "Cancelled"
                });
            };

            System.Action onSuccess = () =>
            {
                LoginWithXsollaAccessToken(Xsolla.Core.XsollaToken.AccessToken, callback);
            };

            Xsolla.Auth.XsollaAuth.AuthWithXsollaWidget(onSuccess, onError, onCancel);
        }

        public void LoginWithXsollaSilentAuth(
            string providerName,
            string appId,
            string sessionTicket,
            ResultCallback<TokenData, OAuthError> callback)
        {
            System.Action<Xsolla.Core.Error> onError = error =>
            {
                callback?.TryError(ToOAuthError(error));
            };

            System.Action onSuccess = () =>
            {
                LoginWithXsollaAccessToken(Xsolla.Core.XsollaToken.AccessToken, callback);
            };

            Xsolla.Auth.XsollaAuth.SilentAuth(providerName, appId, sessionTicket, onSuccess, onError);
        }

        public void LoginWithXsollaAccessToken(
            string xsollaAccessToken,
            ResultCallback<TokenData, OAuthError> callback)
        {
            user.LoginWithOtherPlatformV4(
                new LoginPlatformType(XsollaPlatformId),
                xsollaAccessToken,
                callback);
        }

        public void Logout(ResultCallback callback)
        {
            Xsolla.Core.Error xsollaLogoutError = null;

            void LogoutFromAccelByte()
            {
                user.Logout(result =>
                {
                    if (result.IsError)
                    {
                        callback?.Invoke(result);
                        return;
                    }

                    if (xsollaLogoutError != null)
                    {
                        UnityEngine.Debug.LogWarning($"Xsolla logout failed after AGS logout succeeded: {xsollaLogoutError.ToJsonString()}");
                    }

                    callback?.Invoke(result);
                });
            }

            System.Action<Xsolla.Core.Error> onError = error =>
            {
                xsollaLogoutError = error;
                LogoutFromAccelByte();
            };

            System.Action onSuccess = () =>
            {
                LogoutFromAccelByte();
            };

            Xsolla.Auth.XsollaAuth.Logout(onSuccess, onError);
        }

        private static OAuthError ToOAuthError(Xsolla.Core.Error error)
        {
            return new OAuthError
            {
                error = error.errorCode,
                error_description = error.ToJsonString()
            };
        }
    }
}
