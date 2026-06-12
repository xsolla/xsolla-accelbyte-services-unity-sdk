namespace Xsolla.Backend
{
    [UnityEngine.Scripting.Preserve]
    public class XsollaBackendSDKInstance
    {
        private XsollaClientRegistry clientRegistry;
        private XsollaServerRegistry serverRegistry;
        private static bool configApplied;

        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticMembers()
        {
            configApplied = false;
        }

        /// <summary>
        /// Ensures the <see cref="XsollaConfig"/> overrides are forwarded to
        /// the AccelByte SDK exactly once, before the first registry or config
        /// access. A programmatic config supplied via
        /// <see cref="XsollaBackendSDK.Configure"/> takes priority; otherwise
        /// the asset is loaded from <c>Resources/XsollaConfig</c>.
        /// </summary>
        private static void EnsureConfigApplied()
        {
            if (configApplied)
            {
                return;
            }

            configApplied = true;

            XsollaConfig config = XsollaBackendSDK.ProgrammaticConfig;
            if (config == null)
            {
                config = UnityEngine.Resources.Load<XsollaConfig>("XsollaConfig");
            }

            if (config != null)
            {
                XsollaConfigMapper.Apply(config);
            }
        }

        public XsollaClientRegistry GetClientRegistry()
        {
            if (clientRegistry == null)
            {
                EnsureConfigApplied();
                clientRegistry = new XsollaClientRegistry(AccelByte.Core.AccelByteSDK.GetClientRegistry());
            }
            return clientRegistry;
        }

        public XsollaServerRegistry GetServerRegistry()
        {
            if (serverRegistry == null)
            {
                EnsureConfigApplied();
                serverRegistry = new XsollaServerRegistry(AccelByte.Core.AccelByteSDK.GetServerRegistry());
            }
            return serverRegistry;
        }

        public AccelByte.Models.Config GetClientConfig()
        {
            EnsureConfigApplied();
            return AccelByte.Core.AccelByteSDK.GetClientConfig();
        }

        public AccelByte.Models.ServerConfig GetServerConfig()
        {
            EnsureConfigApplied();
            return AccelByte.Core.AccelByteSDK.GetServerConfig();
        }
    }
}