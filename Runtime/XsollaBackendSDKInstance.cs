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
    }
}