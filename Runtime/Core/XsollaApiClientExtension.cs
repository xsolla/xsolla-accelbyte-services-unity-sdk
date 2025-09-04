// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

namespace Xsolla.GamingService
{
    public partial class ApiClient
    {
        public XsollaAuth GetXsollaAuth(XsollaAuthCreateCommand createCommand = null)
        {
            XsollaAuth xsollaAuthWrapper = accelByteApi.GetApi<XsollaAuth, XsollaAuthApi>(autoCreate: false);
            if (xsollaAuthWrapper == null)
            {
                xsollaAuthWrapper = accelByteApi.GetApi<XsollaAuth, XsollaAuthApi>(autoCreate: true);
                if (createCommand == null)
                {
                    createCommand = new XsollaAuthCreateCommand();
                }
                createCommand.AccelByteUser = GetUser(autoCreate: true);
                xsollaAuthWrapper.CreateCommand(createCommand);
            }
            return xsollaAuthWrapper;
        }
    }
}