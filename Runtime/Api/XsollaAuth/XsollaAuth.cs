// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;

namespace Xsolla.Backend
{
    [UnityEngine.Scripting.Preserve]
    public class XsollaAuth : AccelByte.Core.WrapperBase
    {
        private readonly XsollaAuthApi api;
        private readonly CoroutineRunner coroutineRunner;
        private readonly UserSession session;
        private User userWrapper;
        private string xsollaPlatformId;

        [UnityEngine.Scripting.Preserve]
        public XsollaAuth(XsollaAuthApi inApi, UserSession inLoginSession, CoroutineRunner inCoroutineRunner)
        {
            api = inApi;
            coroutineRunner = inCoroutineRunner;
            session = inLoginSession;
        }

        internal void CreateCommand(XsollaAuthCreateCommand createCommand)
        {
            xsollaPlatformId = createCommand.XsollaPlatformId;
            userWrapper = createCommand.AccelByteUser;
        }

        public void AuthBySavedToken(ResultCallback<XsollaSDKToken, OAuthError> callback)
        {
            AuthBySavedToken(null, callback);
        }

        public void AuthBySavedToken(OptionalParametersBase optionalParameter,
            ResultCallback<XsollaSDKToken, OAuthError> callback)
        {
            System.Action<Xsolla.Core.Error> onError = (error) =>
            {
                callback?.TryError(new OAuthError()
                {
                    error = error.errorCode, error_description = error.ToJsonString()
                });
            };

            System.Action onSuccess = () =>
            {
                string accessToken = Xsolla.Core.XsollaToken.AccessToken;
                LoginToGamingService(accessToken, callback);
            };
            Xsolla.Auth.XsollaAuth.AuthBySavedToken(onSuccess, onError);
        }

        public void AuthWithXsollaWidget(ResultCallback<XsollaSDKToken, OAuthError> callback)
        {
            AuthWithXsollaWidget(null, callback);
        }

        public void AuthWithXsollaWidget(OptionalParametersBase optionalParameter,
            ResultCallback<XsollaSDKToken, OAuthError> callback)
        {
            System.Action<Xsolla.Core.Error> onError = (error) =>
            {
                callback?.TryError(new OAuthError()
                {
                    error = error.errorCode, error_description = error.ToJsonString()
                });
            };

            System.Action oncancel = () =>
            {
                callback?.TryError(new OAuthError()
                {
                    error = ((int)AccelByte.Core.ErrorCode.ExpectationFailed).ToString(),
                    error_description = "Cancelled"
                });
            };

            System.Action onSuccess = () =>
            {
                string accessToken = Xsolla.Core.XsollaToken.AccessToken;
                LoginToGamingService(accessToken, callback);
            };
            Xsolla.Auth.XsollaAuth.AuthWithXsollaWidget(onSuccess, onError, oncancel);
        }

        public void AuthViaXsollaLauncher(ResultCallback<XsollaSDKToken, OAuthError> callback)
        {
            AuthViaXsollaLauncher(null, callback);
        }

        public void AuthViaXsollaLauncher(OptionalParametersBase optionalParameter,
            ResultCallback<XsollaSDKToken, OAuthError> callback)
        {
            System.Action<Xsolla.Core.Error> onError = (error) =>
            {
                callback?.TryError(new OAuthError()
                {
                    error = error.errorCode, error_description = error.ToJsonString()
                });
            };

            System.Action onSuccess = () =>
            {
                string accessToken = Xsolla.Core.XsollaToken.AccessToken;
                LoginToGamingService(accessToken, callback);
            };
            Xsolla.Auth.XsollaAuth.AuthViaXsollaLauncher(onSuccess, onError);
        }

        public void AuthViaSocialNetwork(Xsolla.Core.SocialProvider socialProvider,
            ResultCallback<XsollaSDKToken, OAuthError> callback)
        {
            AuthViaSocialNetwork(socialProvider, null, callback);
        }

        public void AuthViaSocialNetwork(Xsolla.Core.SocialProvider socialProvider,
            OptionalParametersBase optionalParameter, ResultCallback<XsollaSDKToken, OAuthError> callback)
        {
            System.Action<Xsolla.Core.Error> onError = (error) =>
            {
                callback?.TryError(new OAuthError()
                {
                    error = error.errorCode, error_description = error.ToJsonString()
                });
            };

            System.Action oncancel = () =>
            {
                callback?.TryError(new OAuthError()
                {
                    error = ((int)AccelByte.Core.ErrorCode.ExpectationFailed).ToString(),
                    error_description = "Cancelled"
                });
            };

            System.Action onSuccess = () =>
            {
                string accessToken = Xsolla.Core.XsollaToken.AccessToken;
                LoginToGamingService(accessToken, callback);
            };
            Xsolla.Auth.XsollaAuth.AuthViaSocialNetwork(socialProvider, onSuccess, onError, oncancel);
        }

        public void LogOut(ResultCallback callback)
        {
            System.Action<Xsolla.Core.Error> onError = (error) =>
            {
                callback?.TryError(ErrorCode.None, error.ToJsonString());
            };

            System.Action onSuccess = () => { userWrapper.Logout(callback); };
            Xsolla.Auth.XsollaAuth.Logout(onSuccess, onError);
        }

        public void SilentAuth(string providerName, string appId, string sessionTicket, ResultCallback<XsollaSDKToken, OAuthError> callback)
        {
            System.Action<Xsolla.Core.Error> onError = (error) =>
            {
                callback?.TryError(new OAuthError()
                {
                    error = error.errorCode, error_description = error.ToJsonString()
                });
            };

            System.Action oncancel = () =>
            {
                callback?.TryError(new OAuthError()
                {
                    error = ((int)AccelByte.Core.ErrorCode.ExpectationFailed).ToString(),
                    error_description = "Cancelled"
                });
            };

            System.Action onSuccess = () =>
            {
                string accessToken = Xsolla.Core.XsollaToken.AccessToken;
                LoginToGamingService(accessToken, callback);
            };
            Xsolla.Auth.XsollaAuth.SilentAuth(providerName, appId, sessionTicket, onSuccess, onError);
        }

        public void AuthWithXsollaAccessToken(string xsollaAccessToken, ResultCallback<XsollaSDKToken, OAuthError> callback)
        {
            LoginToGamingService(xsollaAccessToken, callback);
        }

        private void LoginToGamingService(string accessToken,
            ResultCallback<XsollaSDKToken, OAuthError> callback)
        {
            AccelByte.Core.ResultCallback<AccelByte.Models.TokenData, AccelByte.Models.OAuthError>
                accelByteLoginCallback = (loginResult) =>
                {
                    if (loginResult.IsError)
                    {
                        callback?.TryError(loginResult.Error);
                        return;
                    }

                    var callbackToken = new XsollaSDKToken()
                    {
                        XsollaAccessToken = accessToken, GamingServiceToken = loginResult.Value
                    };

                    callback?.TryOk(callbackToken);
                };
            userWrapper.LoginWithOtherPlatformV4(new LoginPlatformType(xsollaPlatformId), accessToken,
                accelByteLoginCallback);
        }
    }
}