// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using UnityEngine;

namespace Xsolla.Backend
{
    /// <summary>
    /// Xsolla Backend SDK environment selector.
    /// Values mirror the service environments available in the Xsolla backend.
    /// </summary>
    public enum XsollaEnvironment
    {
        Development,
        Certification,
        Production,
        Default,
        Sandbox,
        QA,
        Integration
    }

    /// <summary>
    /// Central configuration asset for the Xsolla Backend SDK.
    /// Create via <b>Assets &gt; Create &gt; Xsolla &gt; Backend SDK Config</b> and
    /// fill in the values obtained from the Xsolla Publisher Account.
    /// </summary>
    [CreateAssetMenu(menuName = "Xsolla/Backend SDK Config")]
    public class XsollaConfig : ScriptableObject
    {
        // ──────────────────────────────────────────────
        //  Core settings
        // ──────────────────────────────────────────────

        /// <summary>
        /// The Xsolla backend environment this configuration targets
        /// (e.g. Development, Production).
        /// </summary>
        [Header("Core")]
        [Tooltip("Xsolla backend environment this configuration targets.")]
        [SerializeField]
        private XsollaEnvironment environment = XsollaEnvironment.Default;

        /// <summary>
        /// The game namespace registered in the Xsolla Publisher Account.
        /// </summary>
        [Tooltip("Game namespace registered in the Xsolla Publisher Account.")]
        [SerializeField]
        private string gameNamespace = "";

        /// <summary>
        /// OAuth client ID issued by the Xsolla Publisher Account.
        /// </summary>
        [Tooltip("OAuth client ID issued by the Xsolla Publisher Account.")]
        [SerializeField]
        private string clientId = "";

        /// <summary>
        /// OAuth client secret issued by the Xsolla Publisher Account.
        /// Required only for server / confidential clients; leave empty for
        /// public (game-client) configurations.
        /// </summary>
        [Tooltip("OAuth client secret (server / confidential clients only).")]
        [SerializeField]
        private string clientSecret = "";

        /// <summary>
        /// Publisher-level namespace in the Xsolla Publisher Account.
        /// Used when APIs must operate at the publisher scope.
        /// </summary>
        [Tooltip("Publisher-level namespace in the Xsolla Publisher Account.")]
        [SerializeField]
        private string publisherNamespace = "";

        /// <summary>
        /// Base URL of the Xsolla backend services
        /// (e.g. <c>https://your-env.gamingservices.xsolla.com</c>).
        /// All service-specific URLs are derived from this value when left empty.
        /// </summary>
        [Tooltip("Base URL of the Xsolla backend services.")]
        [SerializeField]
        private string baseUrl = "";

        /// <summary>
        /// OAuth redirect URI configured in the Xsolla Publisher Account.
        /// </summary>
        [Tooltip("OAuth redirect URI configured in the Xsolla Publisher Account.")]
        [SerializeField]
        private string redirectUri = "";

        // ──────────────────────────────────────────────
        //  TURN / relay settings (optional)
        // ──────────────────────────────────────────────

        /// <summary>
        /// When <c>true</c>, the SDK will use the Xsolla TURN relay manager
        /// for peer-to-peer connectivity.
        /// </summary>
        [Header("TURN / Relay (optional)")]
        [Tooltip("Enable the Xsolla TURN relay manager for P2P connectivity.")]
        [SerializeField]
        private bool useTurnManager = true;

        /// <summary>
        /// URL of the Xsolla TURN manager service.
        /// If left empty, it is derived from <see cref="BaseUrl"/>.
        /// </summary>
        [Tooltip("URL of the Xsolla TURN manager service.")]
        [SerializeField]
        private string turnManagerServerUrl = "";

        /// <summary>
        /// Hostname or IP address of the Xsolla TURN relay server.
        /// </summary>
        [Tooltip("Hostname or IP of the Xsolla TURN relay server.")]
        [SerializeField]
        private string turnServerHost = "";

        /// <summary>
        /// Port of the Xsolla TURN relay server.
        /// </summary>
        [Tooltip("Port of the Xsolla TURN relay server.")]
        [SerializeField]
        private string turnServerPort = "";

        /// <summary>
        /// Username for authenticating with the Xsolla TURN relay server.
        /// </summary>
        [Tooltip("Username for TURN relay server authentication.")]
        [SerializeField]
        private string turnServerUsername = "";

        /// <summary>
        /// Password for authenticating with the Xsolla TURN relay server.
        /// </summary>
        [Tooltip("Password for TURN relay server authentication.")]
        [SerializeField]
        private string turnServerPassword = "";

        /// <summary>
        /// Shared secret used to generate short-lived TURN credentials.
        /// </summary>
        [Tooltip("Shared secret for short-lived TURN credential generation.")]
        [SerializeField]
        private string turnServerSecret = "";

        // ──────────────────────────────────────────────
        //  Public accessors
        // ──────────────────────────────────────────────

        /// <summary>Target Xsolla backend environment.</summary>
        public XsollaEnvironment Environment => environment;

        /// <summary>Game namespace registered in the Xsolla Publisher Account.</summary>
        public string Namespace => gameNamespace;

        /// <summary>OAuth client ID.</summary>
        public string ClientId => clientId;

        /// <summary>OAuth client secret (server / confidential clients only).</summary>
        public string ClientSecret => clientSecret;

        /// <summary>Publisher-level namespace.</summary>
        public string PublisherNamespace => publisherNamespace;

        /// <summary>Base URL of the Xsolla backend services.</summary>
        public string BaseUrl => baseUrl;

        /// <summary>OAuth redirect URI.</summary>
        public string RedirectUri => redirectUri;

        /// <summary>Whether the Xsolla TURN relay manager is enabled.</summary>
        public bool UseTurnManager => useTurnManager;

        /// <summary>URL of the Xsolla TURN manager service.</summary>
        public string TurnManagerServerUrl => turnManagerServerUrl;

        /// <summary>Hostname or IP of the Xsolla TURN relay server.</summary>
        public string TurnServerHost => turnServerHost;

        /// <summary>Port of the Xsolla TURN relay server.</summary>
        public string TurnServerPort => turnServerPort;

        /// <summary>TURN relay server authentication username.</summary>
        public string TurnServerUsername => turnServerUsername;

        /// <summary>TURN relay server authentication password.</summary>
        public string TurnServerPassword => turnServerPassword;

        /// <summary>Shared secret for short-lived TURN credentials.</summary>
        public string TurnServerSecret => turnServerSecret;
    }
}
