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

        void Logout(ResultCallback callback);
    }
}
