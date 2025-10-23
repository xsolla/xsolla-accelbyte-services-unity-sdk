// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

namespace Xsolla.Backend
{
    [UnityEngine.Scripting.Preserve]
    public class XsollaClientRegistry
    {
        private AccelByte.Api.AccelByteClientRegistry registryImplementation;

        public AccelByte.Core.IDebugger Logger
        {
            get
            {
                return registryImplementation.Logger;
            }
        }
        
        public XsollaClientRegistry(AccelByte.Api.AccelByteClientRegistry clientRegistry)
        {
            registryImplementation = clientRegistry;
        }
        
        public AccelByte.Api.GameStandardAnalyticsClientService GetGameStandardEvents()
        {
            return registryImplementation.GetGameStandardEvents();
        }
        
        public AccelByte.Core.PresenceBroadcastEventScheduler GetPresenceBroadcastEvent()
        {
            return registryImplementation.GetPresenceBroadcastEvent();
        }
        
        public ApiClient GetApi(string id = "default")
        {
            ApiClient clientApi = new ApiClient(registryImplementation.GetApi(id));
            return clientApi;
        }
    }
}