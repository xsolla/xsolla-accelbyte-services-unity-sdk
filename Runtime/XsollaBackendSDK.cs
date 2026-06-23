namespace Xsolla.Backend
{
    public static class XsollaBackendSDK
    {
        public static XsollaBackendSDKInstance Instance;

        /// <summary>
        /// A <see cref="XsollaConfig"/> injected via <see cref="Configure"/>.
        /// When non-null it takes priority over the <c>Resources/XsollaConfig</c>
        /// asset loaded automatically at first SDK access.
        /// </summary>
        internal static XsollaConfig ProgrammaticConfig { get; private set; }

        static XsollaBackendSDK()
        {
            Instance = null;
            Instance = new XsollaBackendSDKInstance();
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticMembers()
        {
            ProgrammaticConfig = null;
            Instance = new XsollaBackendSDKInstance();
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            // Write Xsolla overrides into AccelByteSDK.Implementation.OverrideConfigs
            // before any scene MonoBehaviour Awake() can call
            // AccelByteSDK.GetClientRegistry(), which would otherwise create the
            // registry with empty credentials.
            XsollaBackendSDKInstance.EnsureConfigApplied();
        }

        /// <summary>
        /// Injects a <see cref="XsollaConfig"/> programmatically instead of
        /// relying on the automatic <c>Resources.Load&lt;XsollaConfig&gt;
        /// ("XsollaConfig")</c> that runs at first SDK access.
        /// <para>
        /// <b>Ordering:</b> this method must be called <b>before</b> the first
        /// access to <c>XsollaBackendSDK.Instance.GetClientRegistry()</c>,
        /// <c>GetServerRegistry()</c>, <c>GetClientConfig()</c>, or
        /// <c>GetServerConfig()</c>. Any call made after the AccelByte SDK has
        /// already initialised its registries will have no effect.
        /// </para>
        /// </summary>
        /// <param name="config">
        /// A <see cref="XsollaConfig"/> asset to use for SDK initialisation.
        /// Pass <c>null</c> to clear a previously injected config and fall
        /// back to the Resources-based default.
        /// </param>
        public static void Configure(XsollaConfig config)
        {
            ProgrammaticConfig = config;
        }
    }
}