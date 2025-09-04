// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

namespace Xsolla.GamingService
{
    [UnityEngine.Scripting.Preserve]
    public class XsollaServerRegistry
    {
        private AccelByte.Server.AccelByteServerRegistry registryImplementation;

        public AccelByte.Core.IDebugger Logger
        {
            get
            {
                return registryImplementation.Logger;
            }
        }
        
        public XsollaServerRegistry(AccelByte.Server.AccelByteServerRegistry serverRegistry)
        {
            registryImplementation = serverRegistry;
        }
        
        public Xsolla.GamingService.ApiServer GetApi(string id = "DEFAULT")
        {
            Xsolla.GamingService.ApiServer serverApi = new ApiServer(registryImplementation.GetApi(id));
            return serverApi;
        }

        public AccelByte.Server.ServerAMS GetAMS(bool autoCreate = true, bool autoConnect = true)
        {
            return registryImplementation.GetAMS(autoCreate, autoConnect);
        }
    }
}