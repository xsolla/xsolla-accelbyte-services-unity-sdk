namespace Xsolla.AccelByte
{
    public static class XsollaAccelByteSDK
    {
        public static IXsollaAccelByteAuth Auth => global::AccelByte.Core.AccelByteSDK
            .GetClientRegistry()
            .GetApi()
            .GetXsollaAuth();
    }
}
