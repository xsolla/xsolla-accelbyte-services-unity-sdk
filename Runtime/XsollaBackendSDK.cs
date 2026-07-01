namespace Xsolla.Backend
{
    public static class XsollaBackendSDK
    {
        public static XsollaBackendSDKInstance Instance;

        static XsollaBackendSDK()
        {
            Instance = new XsollaBackendSDKInstance();
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticMembers()
        {
            Instance = new XsollaBackendSDKInstance();
        }
    }
}
