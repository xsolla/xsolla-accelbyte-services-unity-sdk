// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using System;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEditor;
using UnityEngine;
using Xsolla.Backend.Editor;

namespace Xsolla.Backend
{
    public class XsollaConfigEditorWindow : EditorWindow
    {
        private const string WindowTitle = "Xsolla Backend SDK Client Settings";

        private static XsollaConfigEditorWindow instance;

        private int temporaryEnvironmentSetting;
        private int temporaryPlatformSetting;
        private int temporaryPresenceBroadcastEventGameStateSetting;
        private string[] presenceBroadcastEventGameStateList;
        private Rect logoRect;

        private MultiOAuthConfigs originalClientOAuthConfigs;
        private MultiConfigs originalSdkConfigs;
        private OAuthConfig editedClientOAuthConfig;
        private Config editedSdkConfig;

        private Vector2 scrollPos;
        private Dictionary<string, bool> foldoutStatus;
        private bool initialized;
        private bool generateServiceUrl = true;

        [MenuItem("Xsolla/Backend SDK/Edit Client Settings")]
        public static void OpenWindow()
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }

            instance = GetWindow<XsollaConfigEditorWindow>(
                WindowTitle, true,
                Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll"));
            instance.Show();
        }

        private void OnEnable()
        {
            instance = this;
        }

        private void OnDisable()
        {
            if (instance == this) instance = null;
        }

        // ──────────────────────────────────────────────
        //  Initialization
        // ──────────────────────────────────────────────

        private void Initialize()
        {
            if (!initialized)
            {
                temporaryPlatformSetting = 0;
                temporaryEnvironmentSetting = 0;

                presenceBroadcastEventGameStateList = new string[]
                {
                    AccelByte.Utils.JsonUtils.SerializeWithStringEnum(PresenceBroadcastEventGameState.OutOfGameplay),
                    AccelByte.Utils.JsonUtils.SerializeWithStringEnum(PresenceBroadcastEventGameState.InGameplay),
                    AccelByte.Utils.JsonUtils.SerializeWithStringEnum(PresenceBroadcastEventGameState.Store),
                };
                temporaryPresenceBroadcastEventGameStateSetting = 0;

                logoRect = new Rect((position.width - 300) / 2, 10, 300, 86);
                initialized = true;
            }

            if (foldoutStatus == null)
            {
                foldoutStatus = new Dictionary<string, bool>();
            }

            if (originalSdkConfigs == null)
            {
                originalSdkConfigs = AccelByteSettingsV2.LoadSDKConfigFile();
                if (originalSdkConfigs == null)
                {
                    originalSdkConfigs = new MultiConfigs();
                    originalSdkConfigs.InitializeNullEnv();
                }
            }

            if (originalClientOAuthConfigs == null)
            {
                string platformName = XsollaEditorCommon.GetPlatformName(XsollaEditorCommon.PlatformList, temporaryPlatformSetting);
                originalClientOAuthConfigs = AccelByteSettingsV2.LoadOAuthFile(platformName);
                if (originalClientOAuthConfigs == null)
                {
                    originalClientOAuthConfigs = new MultiOAuthConfigs();
                    originalClientOAuthConfigs.InitializeNullEnv();
                }
            }

            var targetEnvironment = XsollaEditorCommon.GetEnvironment(XsollaEditorCommon.EnvironmentList, temporaryEnvironmentSetting);

            if (editedSdkConfig == null)
            {
                var original = AccelByteSettingsV2.GetSDKConfigByEnvironment(originalSdkConfigs, targetEnvironment);
                editedSdkConfig = original != null ? original.ShallowCopy() : new Config();
            }

            if (editedClientOAuthConfig == null)
            {
                var original = AccelByteSettingsV2.GetOAuthByEnvironment(originalClientOAuthConfigs, targetEnvironment);
                editedClientOAuthConfig = original != null ? original.ShallowCopy() : new OAuthConfig();
            }
        }

        // ──────────────────────────────────────────────
        //  GUI
        // ──────────────────────────────────────────────

        private void OnGUI()
        {
            Initialize();

            logoRect.x = (position.width - 300) / 2;
            if (XsollaEditorCommon.XsollaLogo != null)
            {
                GUI.DrawTexture(logoRect, XsollaEditorCommon.XsollaLogo);
            }

            EditorGUILayout.BeginVertical();
            GUILayout.Space(100);

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Editor Deactivated On Runtime", UnityEditor.MessageType.Info, wide: true);
                EditorGUILayout.EndVertical();
                return;
            }

            // Unsaved-changes banner
            {
                var targetEnvironment = XsollaEditorCommon.GetEnvironment(XsollaEditorCommon.EnvironmentList, temporaryEnvironmentSetting);
                var originalSdk = AccelByteSettingsV2.GetSDKConfigByEnvironment(originalSdkConfigs, targetEnvironment);
                var originalOAuth = AccelByteSettingsV2.GetOAuthByEnvironment(originalClientOAuthConfigs, targetEnvironment);

                bool sdkSame = XsollaEditorCommon.CompareConfig(editedSdkConfig, originalSdk);
                bool oAuthSame = XsollaEditorCommon.CompareConfig(editedClientOAuthConfig, originalOAuth);

                EditorGUILayout.HelpBox(
                    (!sdkSame || !oAuthSame) ? "Unsaved changes" : "No changes detected",
                    (!sdkSame || !oAuthSame) ? UnityEditor.MessageType.Warning : UnityEditor.MessageType.Info,
                    wide: true);
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, alwaysShowHorizontal: false, alwaysShowVertical: true);

            // SDK Version
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("SDK Version");
            EditorGUILayout.LabelField(AccelByteSettingsV2.AccelByteSDKVersion);
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndHorizontal();

            // Environment dropdown
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Environment");
            EditorGUI.BeginChangeCheck();
            temporaryEnvironmentSetting = EditorGUILayout.Popup(temporaryEnvironmentSetting, XsollaEditorCommon.EnvironmentList);
            if (EditorGUI.EndChangeCheck())
            {
                var env = XsollaEditorCommon.GetEnvironment(XsollaEditorCommon.EnvironmentList, temporaryEnvironmentSetting);
                var origSdk = AccelByteSettingsV2.GetSDKConfigByEnvironment(originalSdkConfigs, env);
                var origOAuth = AccelByteSettingsV2.GetOAuthByEnvironment(originalClientOAuthConfigs, env);
                editedSdkConfig = origSdk != null ? origSdk.ShallowCopy() : new Config();
                editedClientOAuthConfig = origOAuth != null ? origOAuth.ShallowCopy() : new OAuthConfig();
            }
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndHorizontal();

            // Platform dropdown
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Platform");
            EditorGUI.BeginChangeCheck();
            temporaryPlatformSetting = EditorGUILayout.Popup(temporaryPlatformSetting, XsollaEditorCommon.PlatformList);
            if (EditorGUI.EndChangeCheck())
            {
                string platform = XsollaEditorCommon.GetPlatformName(XsollaEditorCommon.PlatformList, temporaryPlatformSetting);
                var env = XsollaEditorCommon.GetEnvironment(XsollaEditorCommon.EnvironmentList, temporaryEnvironmentSetting);
                originalClientOAuthConfigs = AccelByteSettingsV2.LoadOAuthFile(platform);
                if (originalClientOAuthConfigs == null)
                {
                    originalClientOAuthConfigs = new MultiOAuthConfigs();
                    originalClientOAuthConfigs.InitializeNullEnv();
                }
                var origOAuth = AccelByteSettingsV2.GetOAuthByEnvironment(originalClientOAuthConfigs, env);
                editedClientOAuthConfig = origOAuth != null ? origOAuth.ShallowCopy() : new OAuthConfig();
            }
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndHorizontal();

            // Core fields
            XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.BaseUrl = v, editedSdkConfig.BaseUrl, "Base Url", required: true);
            XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.RedirectUri = v, editedSdkConfig.RedirectUri, "Redirect Uri", required: true);
            XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.Namespace = v, editedSdkConfig.Namespace, "Namespace Id", required: true);
            XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.PublisherNamespace = v, editedSdkConfig.PublisherNamespace, "Publisher Namespace Id", required: true);
            XsollaEditorCommon.CreateTextInput(v => editedClientOAuthConfig.ClientId = v, editedClientOAuthConfig.ClientId, "Client Id", required: true);
            XsollaEditorCommon.CreateTextInput(v => editedClientOAuthConfig.ClientSecret = v, editedClientOAuthConfig.ClientSecret, "Client Secret");
            XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.AppId = v, editedSdkConfig.AppId, "App Id");

            // Analytics
            if (XsollaEditorCommon.CreateFoldout("Analytics Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateTextInput(v => editedClientOAuthConfig.ClientId = v, editedClientOAuthConfig.ClientId, "Analytics Client Id", indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedClientOAuthConfig.ClientSecret = v, editedClientOAuthConfig.ClientSecret, "Analytics Client Secret", indentLevel: 1);
            }

            // Cache
            if (XsollaEditorCommon.CreateFoldout("Cache Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateNumberInput(
                    v => { if (v > 0) editedSdkConfig.MaximumCacheSize = Mathf.FloorToInt((float)v); },
                    editedSdkConfig.MaximumCacheSize, "Cache Size", indentLevel: 1);
                XsollaEditorCommon.CreateNumberInput(
                    v => { if (v > 0) editedSdkConfig.MaximumCacheLifeTime = Mathf.FloorToInt((float)v); },
                    editedSdkConfig.MaximumCacheLifeTime, "Cache Life Time (Seconds)", indentLevel: 1);
            }

            // Matchmaking
            if (XsollaEditorCommon.CreateFoldout("Matchmaking Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.EnableMatchmakingTicketCheck = v, editedSdkConfig.EnableMatchmakingTicketCheck, "Enable Fallback Ticket Poll Check", indentLevel: 1);
                XsollaEditorCommon.CreateNumberInput(v => editedSdkConfig.MatchmakingTicketCheckPollRate = (int)v, editedSdkConfig.MatchmakingTicketCheckPollRate, "Ticket Poll Rate", indentLevel: 1);
            }

            // Other
            if (XsollaEditorCommon.CreateFoldout("Other Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.CustomerName = v, editedSdkConfig.CustomerName, "Customer Name", indentLevel: 1);
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.UsePlayerPrefs = v, editedSdkConfig.UsePlayerPrefs, "Use PlayerPrefs", indentLevel: 1);
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.RandomizeDeviceId = v, editedSdkConfig.RandomizeDeviceId, "Randomize Device Id", indentLevel: 1);
            }

            // Log
            if (XsollaEditorCommon.CreateFoldout("Log Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.EnableDebugLog = v, editedSdkConfig.EnableDebugLog, "Enable Debug Log", indentLevel: 1);

                EditorGUILayout.BeginHorizontal();
                XsollaEditorCommon.AddIndent(depth: 1);
                EditorGUILayout.LabelField("Log Type Filter");
                if (!Enum.TryParse(editedSdkConfig.DebugLogFilter, out AccelByteLogType currentLog))
                {
                    currentLog = AccelByteLogType.Verbose;
                }
                editedSdkConfig.DebugLogFilter = ((AccelByteLogType)EditorGUILayout.EnumPopup(currentLog)).ToString();
                EditorGUILayout.LabelField("");
                EditorGUILayout.EndHorizontal();

                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.EnhancedServiceLogging = v, editedSdkConfig.EnhancedServiceLogging, "Enhanced Service Logging", indentLevel: 1);
            }

            // Service URLs
            if (XsollaEditorCommon.CreateFoldout("Service Url Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => generateServiceUrl = v, generateServiceUrl, "Auto Generate Service Url", indentLevel: 1);
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.EnableAmsServerQos = v, editedSdkConfig.EnableAmsServerQos, "Use AMS QoS Server Url", indentLevel: 1);

                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.IamServerUrl = v, editedSdkConfig.IamServerUrl, "IAM Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.PlatformServerUrl = v, editedSdkConfig.PlatformServerUrl, "Platform Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.BasicServerUrl = v, editedSdkConfig.BasicServerUrl, "Basic Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.LobbyServerUrl = v, editedSdkConfig.LobbyServerUrl, "Lobby Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.ChatServerUrl = v, editedSdkConfig.ChatServerUrl, "Chat Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.ChatServerWsUrl = v, editedSdkConfig.ChatServerWsUrl, "Chat Server Websocket Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.CloudStorageServerUrl = v, editedSdkConfig.CloudStorageServerUrl, "Cloud Storage Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.GameProfileServerUrl = v, editedSdkConfig.GameProfileServerUrl, "Game Profile Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.StatisticServerUrl = v, editedSdkConfig.StatisticServerUrl, "Statistic Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.AchievementServerUrl = v, editedSdkConfig.AchievementServerUrl, "Achievement Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.CloudSaveServerUrl = v, editedSdkConfig.CloudSaveServerUrl, "CloudSave Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.QosManagerServerUrl = v, editedSdkConfig.QosManagerServerUrl, "QoS Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.AgreementServerUrl = v, editedSdkConfig.AgreementServerUrl, "Agreement Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.LeaderboardServerUrl = v, editedSdkConfig.LeaderboardServerUrl, "Leaderboard Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.GameTelemetryServerUrl = v, editedSdkConfig.GameTelemetryServerUrl, "Game Telemetry Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.GroupServerUrl = v, editedSdkConfig.GroupServerUrl, "Group Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.SeasonPassServerUrl = v, editedSdkConfig.SeasonPassServerUrl, "Season Pass Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.SessionServerUrl = v, editedSdkConfig.SessionServerUrl, "Session Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.MatchmakingV2ServerUrl = v, editedSdkConfig.MatchmakingV2ServerUrl, "MatchmakingV2 Server Url", @readonly: generateServiceUrl, indentLevel: 1);
            }

            // TURN
            if (XsollaEditorCommon.CreateFoldout("TURN Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.UseTurnManager = v, editedSdkConfig.UseTurnManager, "Use TURN Manager", indentLevel: 1);
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.EnableAuthHandshake = v, editedSdkConfig.EnableAuthHandshake, "Use Secure Handshaking", indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.TurnServerHost = v, editedSdkConfig.TurnServerHost, "TURN Server Host", indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.TurnServerPort = v, editedSdkConfig.TurnServerPort, "TURN Server Port", indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.TurnManagerServerUrl = v, editedSdkConfig.TurnManagerServerUrl, "TURN Manager Server Url", indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.TurnServerUsername = v, editedSdkConfig.TurnServerUsername, "TURN Server Username", indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.TurnServerSecret = v, editedSdkConfig.TurnServerSecret, "TURN Server Secret", indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.TurnServerPassword = v, editedSdkConfig.TurnServerPassword, "TURN Server Password", indentLevel: 1);
                XsollaEditorCommon.CreateNumberInput(v => editedSdkConfig.PeerMonitorIntervalMs = (int)v, editedSdkConfig.PeerMonitorIntervalMs, "Peer Monitor Interval in Milliseconds", indentLevel: 1);
                XsollaEditorCommon.CreateNumberInput(v => editedSdkConfig.PeerMonitorTimeoutMs = (int)v, editedSdkConfig.PeerMonitorTimeoutMs, "Peer Monitor Timeout in Milliseconds", indentLevel: 1);
                XsollaEditorCommon.CreateNumberInput(v => editedSdkConfig.HostCheckTimeoutInSeconds = (int)v, editedSdkConfig.HostCheckTimeoutInSeconds, "Host Check Timeout in Seconds", indentLevel: 1);
            }

            // Presence Broadcast Event
            if (XsollaEditorCommon.CreateFoldout("Presence Broadcast Event Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.EnablePresenceBroadcastEvent = v, editedSdkConfig.EnablePresenceBroadcastEvent, "Enable Presence Broadcast Event", indentLevel: 1);
                XsollaEditorCommon.CreateNumberInput(v => editedSdkConfig.PresenceBroadcastEventInterval = (int)v, editedSdkConfig.PresenceBroadcastEventInterval, "Set Interval In Seconds", indentLevel: 1);

                int minInterval = PresenceBroadcastEventScheduler.MiniumAllowedIntervalInlMs / 1000;
                if (editedSdkConfig.PresenceBroadcastEventInterval < minInterval)
                {
                    editedSdkConfig.PresenceBroadcastEventInterval = minInterval;
                }

                EditorGUILayout.BeginHorizontal();
                XsollaEditorCommon.AddIndent(depth: 1);
                EditorGUILayout.LabelField("Game State");
                EditorGUI.BeginChangeCheck();
                temporaryPresenceBroadcastEventGameStateSetting = EditorGUILayout.Popup(temporaryPresenceBroadcastEventGameStateSetting, presenceBroadcastEventGameStateList);
                if (EditorGUI.EndChangeCheck())
                {
                    editedSdkConfig.PresenceBroadcastEventGameState =
                        presenceBroadcastEventGameStateList[temporaryPresenceBroadcastEventGameStateSetting] !=
                        AccelByte.Utils.JsonUtils.SerializeWithStringEnum(PresenceBroadcastEventGameState.OutOfGameplay)
                            ? temporaryPresenceBroadcastEventGameStateSetting
                            : 0;
                }
                EditorGUILayout.LabelField("");
                EditorGUILayout.EndHorizontal();

                XsollaEditorCommon.CreateTextInput(v => editedSdkConfig.PresenceBroadcastEventGameStateDescription = v, editedSdkConfig.PresenceBroadcastEventGameStateDescription, "Set Game State description", indentLevel: 1);
            }

            // Pre-Defined Event
            if (XsollaEditorCommon.CreateFoldout("Pre-Defined Event Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.EnablePreDefinedEvent = v, editedSdkConfig.EnablePreDefinedEvent, "Enable Pre-Defined Game Event", indentLevel: 1);
            }

            // Client Analytics
            if (XsollaEditorCommon.CreateFoldout("Client Analytics Event Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.EnableClientAnalyticsEvent = v, editedSdkConfig.EnableClientAnalyticsEvent, "Enable Client Analytics Event", indentLevel: 1);
                XsollaEditorCommon.CreateNumberInput(v => editedSdkConfig.ClientAnalyticsEventInterval = (float)v, editedSdkConfig.ClientAnalyticsEventInterval, "Set Interval In Seconds", indentLevel: 1);

                const float minAnalyticsInterval = ClientAnalyticsEventScheduler.ClientAnalyticsMiniumAllowedIntervalInlMs / 1000f;
                if (editedSdkConfig.ClientAnalyticsEventInterval < minAnalyticsInterval)
                {
                    editedSdkConfig.ClientAnalyticsEventInterval = minAnalyticsInterval;
                }
            }

            // Game Telemetry
            if (XsollaEditorCommon.CreateFoldout("Game Telemetry Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.GameTelemetryCacheEnabled = v, editedSdkConfig.GameTelemetryCacheEnabled, "Enable Game Telemetry event cache", indentLevel: 1);
                XsollaEditorCommon.CreateToggleInput(v => editedSdkConfig.EnableGameTelemetryStartupAutoSend = v, editedSdkConfig.EnableGameTelemetryStartupAutoSend, "Enable Game Telemetry start up auto-send", indentLevel: 1);
            }

            // Google
            if (XsollaEditorCommon.CreateFoldout("Google Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateTextInput(v => editedClientOAuthConfig.GoogleWebClientId = v, editedClientOAuthConfig.GoogleWebClientId, "Web Client Id");
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();

            if (GUILayout.Button("Save"))
            {
                editedClientOAuthConfig.Expand();
                editedSdkConfig.SanitizeBaseUrl();
                editedSdkConfig.Expand(generateServiceUrl);

                var targetEnvironment = XsollaEditorCommon.GetEnvironment(XsollaEditorCommon.EnvironmentList, temporaryEnvironmentSetting);
                string platformName = XsollaEditorCommon.GetPlatformName(XsollaEditorCommon.PlatformList, temporaryPlatformSetting);

                originalClientOAuthConfigs = AccelByteSettingsV2.SetOAuthByEnvironment(originalClientOAuthConfigs, editedClientOAuthConfig.ShallowCopy(), targetEnvironment);
                originalSdkConfigs = AccelByteSettingsV2.SetSDKConfigByEnvironment(originalSdkConfigs, editedSdkConfig.ShallowCopy(), targetEnvironment);

                AccelByteSettingsV2.SaveConfig(originalClientOAuthConfigs, AccelByteSettingsV2.OAuthFullPath(platformName));
                AccelByteSettingsV2.SaveConfig(originalSdkConfigs, AccelByteSettingsV2.SDKConfigFullPath(false));

                Debug.Log("Xsolla Backend SDK client config saved.");
            }

            EditorGUILayout.EndVertical();
        }
    }
}
