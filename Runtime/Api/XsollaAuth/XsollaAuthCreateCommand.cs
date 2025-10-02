// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

namespace Xsolla.Backend
{
    [UnityEngine.Scripting.Preserve]
    public class XsollaAuthCreateCommand
    {
        internal string XsollaPlatformId;
        internal AccelByte.Api.User AccelByteUser; 
        
        public XsollaAuthCreateCommand()
        {
            XsollaPlatformId = "xsolla";    
        }
        
        public XsollaAuthCreateCommand(string xsollaPlatformId)
        {
            XsollaPlatformId = xsollaPlatformId;
        }
    }
}