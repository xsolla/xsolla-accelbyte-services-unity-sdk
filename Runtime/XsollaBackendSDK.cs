namespace Xsolla.Backend
{
    public static class XsollaBackendSDK
    {
        public static XsollaBackendSDKInstance Instance;

        static XsollaBackendSDK()
        {
            Instance = null;
            Instance = new XsollaBackendSDKInstance();
        }
    }
}