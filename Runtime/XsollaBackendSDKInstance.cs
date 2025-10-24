namespace Xsolla.Backend
{
    [UnityEngine.Scripting.Preserve]
    public class XsollaBackendSDKInstance
    {
        private XsollaClientRegistry clientRegistry;
        private XsollaServerRegistry serverRegistry;
        
        public XsollaClientRegistry GetClientRegistry()
        {
            if (clientRegistry == null)
            {
                clientRegistry = new XsollaClientRegistry(AccelByte.Core.AccelByteSDK.GetClientRegistry());
            }
            return clientRegistry;
        }
        
        public XsollaServerRegistry GetServerRegistry()
        {
            if (serverRegistry == null)
            {
                serverRegistry = new XsollaServerRegistry(AccelByte.Core.AccelByteSDK.GetServerRegistry());
            }
            return serverRegistry;
        }
        public AccelByte.Models.Config GetClientConfig()
        {
            return AccelByte.Core.AccelByteSDK.GetClientConfig();
        }

        public AccelByte.Models.ServerConfig GetServerConfig()
        {
            return AccelByte.Core.AccelByteSDK.GetServerConfig();
        }
    }
}