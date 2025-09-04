namespace Xsolla.GamingService
{
    [UnityEngine.Scripting.Preserve]
    public class XsollaSDKInstance
    {
        private XsollaClientRegistry clientRegistry;
        private XsollaServerRegistry serverRegistry;
        
        public Xsolla.GamingService.XsollaClientRegistry GetClientRegistry()
        {
            if (clientRegistry == null)
            {
                clientRegistry = new XsollaClientRegistry(AccelByte.Core.AccelByteSDK.GetClientRegistry());
            }
            return clientRegistry;
        }
        
        public Xsolla.GamingService.XsollaServerRegistry GetServerRegistry()
        {
            if (serverRegistry == null)
            {
                serverRegistry = new XsollaServerRegistry(AccelByte.Core.AccelByteSDK.GetServerRegistry());
            }
            return serverRegistry;
        }
    }
}