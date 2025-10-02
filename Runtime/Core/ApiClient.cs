// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using AccelByte.Api;

namespace Xsolla.Backend
{
    [UnityEngine.Scripting.Preserve]
    public partial class ApiClient
    {
        private AccelByte.Core.ApiClient accelByteApi;

        public ApiClient(AccelByte.Core.ApiClient accelByteApi)
        {
            this.accelByteApi = accelByteApi;
        }

        public User GetUser(bool autoCreate = true)
        {
            return accelByteApi.GetUser(autoCreate);
        }

        public UserProfiles GetUserProfiles()
        {
            return accelByteApi.GetUserProfiles();
        }

        public Categories GetCategories()
        {
            return accelByteApi.GetCategories();
        }

        public AccelByte.Api.Interface.IClientChallenge GetChallenge()
        {
            return accelByteApi.GetChallenge();
        }

        public Items GetItems()
        {
            return accelByteApi.GetItems();
        }

        public AccelByte.Api.Interface.IClientInventory GetInventory()
        {
            return accelByteApi.GetInventory();
        }

        public Currencies GetCurrencies()
        {
            return accelByteApi.GetCurrencies();
        }

        public AccelByte.Api.Orders GetOrders()
        {
            return accelByteApi.GetOrders();
        }

        public Reward GetReward()
        {
            return accelByteApi.GetReward();
        }

        public Wallet GetWallet()
        {
            return accelByteApi.GetWallet();
        }

        public Lobby GetLobby()
        {
            return accelByteApi.GetLobby();
        }

        public Entitlement GetEntitlement()
        {
            return accelByteApi.GetEntitlement();
        }

        public Fulfillment GetFulfillment()
        {
            return accelByteApi.GetFulfillment();
        }

        public Statistic GetStatistic()
        {
            return accelByteApi.GetStatistic();
        }

        public QosManager GetQos()
        {
            return accelByteApi.GetQos();
        }

        public Agreement GetAgreement()
        {
            return accelByteApi.GetAgreement();
        }

        public Leaderboard GetLeaderboard()
        {
            return accelByteApi.GetLeaderboard();
        }

        public CloudSave GetCloudSave()
        {
            return accelByteApi.GetCloudSave();
        }

        public GameTelemetry GetGameTelemetry()
        {
            return accelByteApi.GetGameTelemetry();
        }

        public Achievement GetAchievement()
        {
            return accelByteApi.GetAchievement();
        }

        public Group GetGroup()
        {
            return accelByteApi.GetGroup();
        }

        public UGC GetUgc()
        {
            return accelByteApi.GetUgc();
        }

        public Reporting GetReporting()
        {
            return accelByteApi.GetReporting();
        }

        public SeasonPass GetSeasonPass()
        {
            return accelByteApi.GetSeasonPass();
        }

        public TurnManager GetTurnManager()
        {
            return accelByteApi.GetTurnManager();
        }

        public Miscellaneous GetMiscellaneous()
        {
            return accelByteApi.GetMiscellaneous();
        }

        public Session GetSession(bool autoCreate = true)
        {
            return accelByteApi.GetSession(autoCreate);
        }

        public MatchmakingV2 GetMatchmakingV2()
        {
            return accelByteApi.GetMatchmakingV2();
        }

        public Chat GetChat()
        {
            return accelByteApi.GetChat();
        }

        public Gdpr GetGdpr()
        {
            return accelByteApi.GetGdpr();
        }

        public AnalyticsService GetAnalyticsService()
        {
            return accelByteApi.GetAnalyticsService();
        }

        public BinaryCloudSave GetBinaryCloudSave()
        {
            return accelByteApi.GetBinaryCloudSave();
        }

        public AccelByte.Api.ServiceVersion GetVersionService()
        {
            return accelByteApi.GetVersionService();
        }

        public HeartBeat GetHeartBeatService(bool autoCreate = true)
        {
            HeartBeat retval = accelByteApi.GetHeartBeatService(autoCreate);
            return retval;
        }

        public StoreDisplay GetStoreDisplayService()
        {
            return accelByteApi.GetStoreDisplayService();
        }
    }
}