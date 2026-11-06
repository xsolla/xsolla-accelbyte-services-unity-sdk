// Copyright (c) 2025-2026 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using AccelByte.Core;
using AccelByte.Models;

namespace Xsolla.AccelByte
{
    public interface IXsollaAccelByteAuth
    {
        void LoginWithXsollaAccount(ResultCallback<TokenData, OAuthError> callback);

        void LoginWithXsollaSilentAuth(
            string providerName,
            string appId,
            string sessionTicket,
            ResultCallback<TokenData, OAuthError> callback);

        void LoginWithXsollaAccessToken(
            string xsollaAccessToken,
            ResultCallback<TokenData, OAuthError> callback);

        /// <summary>
        /// Logs in to AGS with an Xsolla ID id_token that the game already has, for example from the Xsolla ID web SDK.
        /// </summary>
        void LoginWithXsollaIdToken(
            string idToken,
            ResultCallback<TokenData, OAuthError> callback);

        /// <summary>
        /// Logs in with Xsolla ID in the system browser (authorization code flow with PKCE), then logs in to AGS.
        /// Windows, Mac and Linux standalone builds and the Editor only. Call it on the main thread.
        /// </summary>
        void LoginWithXsollaId(
            XsollaIdOptions options,
            ResultCallback<TokenData, OAuthError> callback);

        /// <summary>
        /// Stops a LoginWithXsollaId that waits for the browser. Its callback receives the cancelled error.
        /// </summary>
        void CancelXsollaIdLogin();

        void Logout(ResultCallback callback);
    }
}
