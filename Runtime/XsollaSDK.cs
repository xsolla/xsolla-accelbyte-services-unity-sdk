namespace Xsolla.GamingService
{
    public static class XsollaSDK
    {
        public static XsollaSDKInstance Instance;

        static XsollaSDK()
        {
            Instance = null;
            Instance = new XsollaSDKInstance();
        }
    }
}