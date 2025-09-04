// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using AccelByte.Server;

namespace Xsolla.GamingService
{
    [UnityEngine.Scripting.Preserve]
    public partial class ApiServer
    {
        private AccelByte.Core.ServerApiClient accelByteApi;

        public ApiServer(AccelByte.Core.ServerApiClient accelByteApi)
        {
            this.accelByteApi = accelByteApi;
        }

        public DedicatedServer GetDedicatedServer()
        {
            return accelByteApi.GetDedicatedServer();
        }

        public DedicatedServerManager GetDedicatedServerManager()
        {
            return accelByteApi.GetDedicatedServerManager();
        }

        public ServerEcommerce GetEcommerce()
        {
            return accelByteApi.GetEcommerce();
        }

        public ServerStatistic GetStatistic()
        {
            return accelByteApi.GetStatistic();
        }

        public ServerUGC GetUGC()
        {
            return accelByteApi.GetUGC();
        }

        public ServerQosManager GetQos()
        {
            return accelByteApi.GetQos();
        }

        public ServerGameTelemetry GetGameTelemetry()
        {
            return accelByteApi.GetGameTelemetry();
        }

        public ServerAchievement GetAchievement()
        {
            return accelByteApi.GetAchievement();
        }

        public ServerLobby GetLobby()
        {
            return accelByteApi.GetLobby();
        }

        public AccelByte.Server.Interface.IServerBinaryCloudSave GetBinaryCloudSave()
        {
            return accelByteApi.GetBinaryCloudSave();
        }

        public ServerCloudSave GetCloudSave()
        {
            return accelByteApi.GetCloudSave();
        }

        public AccelByte.Server.Interface.IServerChallenge GetChallenge()
        {
            return accelByteApi.GetChallenge();
        }

        public AccelByte.Server.Interface.IServerInventory GetInventory()
        {
            return accelByteApi.GetInventory();
        }

        public ServerMatchmaking GetMatchmaking()
        {
            return accelByteApi.GetMatchmaking();
        }

        public ServerUserAccount GetUserAccount()
        {
            return accelByteApi.GetUserAccount();
        }

        public ServerSeasonPass GetSeasonPass()
        {
            return accelByteApi.GetSeasonPass();
        }

        public ServerDSHub GetDsHub()
        {
            return accelByteApi.GetDsHub();
        }

        public ServerSession GetSession()
        {
            return accelByteApi.GetSession();
        }

        public ServerMatchmakingV2 GetMatchmakingV2()
        {
            return accelByteApi.GetMatchmakingV2();
        }

        public ServerAnalyticsService GetAnalyticsService()
        {
            return accelByteApi.GetAnalyticsService();
        }

        public IServerMiscellaneousWrapper GetMiscellaneous()
        {
            return accelByteApi.GetMiscellaneous();
        }

        public AccelByte.Server.Interface.IServerProfanityFilter GetProfanityFilter()
        {
            return accelByteApi.GetProfanityFilter();
        }

        public ServiceVersion GetVersionService()
        {
            return accelByteApi.GetVersionService();
        }

        public AccelByteStatsDMetricExporterApi GetStatsMetricExporterService()
        {
            return accelByteApi.GetStatsMetricExporterService();
        }
    }
}