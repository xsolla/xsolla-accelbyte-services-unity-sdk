// Copyright (c) 2026 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

namespace Xsolla.AccelByte
{
    /// <summary>
    /// Settings for <see cref="IXsollaAccelByteAuth.LoginWithXsollaId"/>.
    /// </summary>
    public class XsollaIdOptions
    {
        /// <summary>Xsolla ID public client ID. Must match the Client ID configured for the game namespace in the AGS Admin Portal.</summary>
        public string ClientId;

        /// <summary>Xsolla ID issuer. The authorization and token endpoints are built from it.</summary>
        public string Issuer = "https://id.xsolla.com";

        /// <summary>Space-separated scopes. openid is required; add profile and email to fill the AGS display name and email.</summary>
        public string Scope = "openid";

        /// <summary>
        /// Host of the redirect URI. Keep 127.0.0.1 (RFC 8252 section 8.3). Use localhost only to match a redirect URI
        /// that is registered with localhost, for example during testing. The listener always binds 127.0.0.1.
        /// </summary>
        public string RedirectHost = "127.0.0.1";

        /// <summary>Loopback port for the redirect URI http://&lt;host&gt;:&lt;port&gt;&lt;path&gt;. 0 lets the OS pick a free port.</summary>
        public int RedirectPort = 0;

        /// <summary>Path of the loopback redirect URI. Must match the redirect URI registered for the Xsolla ID client.</summary>
        public string RedirectPath = "/callback";

        /// <summary>Time the player has to finish the Xsolla ID login in the browser.</summary>
        public int LoginTimeoutSeconds = 300;
    }
}
