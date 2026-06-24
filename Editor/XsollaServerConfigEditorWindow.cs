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
    public class XsollaServerConfigEditorWindow : EditorWindow
    {
        private const string WindowTitle = "Xsolla Backend SDK Server Settings";

        private static XsollaServerConfigEditorWindow instance;

        private int temporaryEnvironmentSetting;
        private int temporaryPlatformSetting;
        private Rect logoRect;

        private MultiOAuthConfigs originalServerOAuthConfigs;
        private MultiServerConfigs originalServerConfigs;
        private OAuthConfig editedServerOAuthConfig;
        private ServerConfig editedServerConfig;

        private Vector2 scrollPos;
        private Dictionary<string, bool> foldoutStatus;
        private bool initialized;
        private bool generateServiceUrl = true;

        [MenuItem("Xsolla/Backend SDK/Edit Server Settings")]
        public static void OpenWindow()
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }

            instance = GetWindow<XsollaServerConfigEditorWindow>(
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
                logoRect = new Rect((position.width - 300) / 2, 10, 300, 86);
                initialized = true;
            }

            if (foldoutStatus == null)
            {
                foldoutStatus = new Dictionary<string, bool>();
            }

            if (originalServerConfigs == null)
            {
                originalServerConfigs = AccelByteSettingsV2.LoadSDKServerConfigFile();
                if (originalServerConfigs == null)
                {
                    originalServerConfigs = new MultiServerConfigs();
                    originalServerConfigs.InitializeNullEnv();
                }
            }

            if (originalServerOAuthConfigs == null)
            {
                originalServerOAuthConfigs = AccelByteSettingsV2.LoadOAuthFile(string.Empty, isServerConfig: true);
                if (originalServerOAuthConfigs == null)
                {
                    originalServerOAuthConfigs = new MultiOAuthConfigs();
                    originalServerOAuthConfigs.InitializeNullEnv();
                }
            }

            var targetEnvironment = XsollaEditorCommon.GetEnvironment(XsollaEditorCommon.EnvironmentList, temporaryEnvironmentSetting);

            if (editedServerConfig == null)
            {
                var original = AccelByteSettingsV2.GetSDKConfigByEnvironment(originalServerConfigs, targetEnvironment);
                editedServerConfig = original != null ? original.ShallowCopy() : new ServerConfig();
            }

            if (editedServerOAuthConfig == null)
            {
                var original = AccelByteSettingsV2.GetOAuthByEnvironment(originalServerOAuthConfigs, targetEnvironment);
                editedServerOAuthConfig = original != null ? original.ShallowCopy() : new OAuthConfig();
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
                var originalSrv = AccelByteSettingsV2.GetSDKConfigByEnvironment(originalServerConfigs, targetEnvironment);
                var originalOAuth = AccelByteSettingsV2.GetOAuthByEnvironment(originalServerOAuthConfigs, targetEnvironment);

                bool srvSame = XsollaEditorCommon.CompareConfig(editedServerConfig, originalSrv);
                bool oAuthSame = XsollaEditorCommon.CompareConfig(editedServerOAuthConfig, originalOAuth);

                EditorGUILayout.HelpBox(
                    (!srvSame || !oAuthSame) ? "Unsaved changes" : "No changes detected",
                    (!srvSame || !oAuthSame) ? UnityEditor.MessageType.Warning : UnityEditor.MessageType.Info,
                    wide: true);
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, alwaysShowHorizontal: false, alwaysShowVertical: true);

            // Environment dropdown
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Environment");
            EditorGUI.BeginChangeCheck();
            temporaryEnvironmentSetting = EditorGUILayout.Popup(temporaryEnvironmentSetting, XsollaEditorCommon.EnvironmentList);
            if (EditorGUI.EndChangeCheck())
            {
                var env = XsollaEditorCommon.GetEnvironment(XsollaEditorCommon.EnvironmentList, temporaryEnvironmentSetting);
                var origSrv = AccelByteSettingsV2.GetSDKConfigByEnvironment(originalServerConfigs, env);
                var origOAuth = AccelByteSettingsV2.GetOAuthByEnvironment(originalServerOAuthConfigs, env);
                editedServerConfig = origSrv != null ? origSrv.ShallowCopy() : new ServerConfig();
                editedServerOAuthConfig = origOAuth != null ? origOAuth.ShallowCopy() : new OAuthConfig();
            }
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndHorizontal();

            // Core fields
            XsollaEditorCommon.CreateTextInput(v => editedServerConfig.BaseUrl = v, editedServerConfig.BaseUrl, "Base Url", required: true);
            XsollaEditorCommon.CreateTextInput(v => editedServerConfig.RedirectUri = v, editedServerConfig.RedirectUri, "Redirect Uri", required: true);
            XsollaEditorCommon.CreateTextInput(v => editedServerConfig.Namespace = v, editedServerConfig.Namespace, "Namespace Id", required: true);
            XsollaEditorCommon.CreateTextInput(v => editedServerConfig.PublisherNamespace = v, editedServerConfig.PublisherNamespace, "Publisher Namespace Id", required: true);
            XsollaEditorCommon.CreateTextInput(v => editedServerOAuthConfig.ClientId = v, editedServerOAuthConfig.ClientId, "Client Id", required: true);
            XsollaEditorCommon.CreateTextInput(v => editedServerOAuthConfig.ClientSecret = v, editedServerOAuthConfig.ClientSecret, "Client Secret");

            // Log Configs
            if (XsollaEditorCommon.CreateFoldout("Log Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => editedServerConfig.EnableDebugLog = v, editedServerConfig.EnableDebugLog, "Enable Debug Log", indentLevel: 1);

                EditorGUILayout.BeginHorizontal();
                XsollaEditorCommon.AddIndent(depth: 1);
                EditorGUILayout.LabelField("Log Type Filter");
                if (!Enum.TryParse(editedServerConfig.DebugLogFilter, out AccelByteLogType currentLog))
                {
                    currentLog = AccelByteLogType.Verbose;
                }
                editedServerConfig.DebugLogFilter = ((AccelByteLogType)EditorGUILayout.EnumPopup(currentLog)).ToString();
                EditorGUILayout.LabelField("");
                EditorGUILayout.EndHorizontal();

                XsollaEditorCommon.CreateToggleInput(v => editedServerConfig.EnhancedServiceLogging = v, editedServerConfig.EnhancedServiceLogging, "Enhanced Service Logging", indentLevel: 1);
            }

            // Service Url Configs
            if (XsollaEditorCommon.CreateFoldout("Service Url Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => generateServiceUrl = v, generateServiceUrl, "Auto Generate Service Url", indentLevel: 1);

                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.IamServerUrl = v, editedServerConfig.IamServerUrl, "IAM Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.BasicServerUrl = v, editedServerConfig.BasicServerUrl, "Basic Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.DSHubServerUrl = v, editedServerConfig.DSHubServerUrl, "DS Hub Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => { }, editedServerConfig.DSHubServerWsUrl, "DS Hub Server Websocket Url", @readonly: true, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.DSMControllerServerUrl = v, editedServerConfig.DSMControllerServerUrl, "DSM Controller Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.StatisticServerUrl = v, editedServerConfig.StatisticServerUrl, "Statistic Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.UGCServerUrl = v, editedServerConfig.UGCServerUrl, "UGC Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.PlatformServerUrl = v, editedServerConfig.PlatformServerUrl, "Platform Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.QosManagerServerUrl = v, editedServerConfig.QosManagerServerUrl, "QoS Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.GameTelemetryServerUrl = v, editedServerConfig.GameTelemetryServerUrl, "Game Telemetry Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.AchievementServerUrl = v, editedServerConfig.AchievementServerUrl, "Achievement Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.ChallengeServerUrl = v, editedServerConfig.ChallengeServerUrl, "Challenge Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.InventoryServerUrl = v, editedServerConfig.InventoryServerUrl, "Inventory Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.ProfanityFilterServerUrl = v, editedServerConfig.ProfanityFilterServerUrl, "Profanity Filter Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.LobbyServerUrl = v, editedServerConfig.LobbyServerUrl, "Lobby Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.SessionServerUrl = v, editedServerConfig.SessionServerUrl, "Session Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.CloudSaveServerUrl = v, editedServerConfig.CloudSaveServerUrl, "CloudSave Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.MatchmakingV2ServerUrl = v, editedServerConfig.MatchmakingV2ServerUrl, "MatchmakingV2 Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.AMSServerUrl = v, editedServerConfig.AMSServerUrl, "AMS Server Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.WatchdogUrl = v, editedServerConfig.WatchdogUrl, "Watchdog Url", @readonly: generateServiceUrl, indentLevel: 1);
                XsollaEditorCommon.CreateTextInput(v => editedServerConfig.StatsDServerUrl = v, editedServerConfig.StatsDServerUrl, "Stat DS URL", @readonly: generateServiceUrl, indentLevel: 1);
            }

            // Server Port
            if (XsollaEditorCommon.CreateFoldout("Server Port", foldoutStatus))
            {
                XsollaEditorCommon.CreateNumberInput(v => editedServerConfig.StatsDServerPort = (int)v, editedServerConfig.StatsDServerPort, "Stat DS Port", indentLevel: 1);
            }

            // Server DMetric
            if (XsollaEditorCommon.CreateFoldout("Server DMetric", foldoutStatus))
            {
                XsollaEditorCommon.CreateNumberInput(v => editedServerConfig.StatsDMetricInterval = (int)v, editedServerConfig.StatsDMetricInterval, "DMetric Collection Interval (Seconds)", indentLevel: 1);
            }

            // Pre-Defined Event Configs
            if (XsollaEditorCommon.CreateFoldout("Pre-Defined Event Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => editedServerConfig.EnablePreDefinedEvent = v, editedServerConfig.EnablePreDefinedEvent, "Enable Pre-Defined Game Event", indentLevel: 1);
            }

            // Cache Configs
            if (XsollaEditorCommon.CreateFoldout("Cache Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateNumberInput(
                    v => { if (v > 0) editedServerConfig.MaximumCacheSize = Mathf.FloorToInt((float)v); },
                    editedServerConfig.MaximumCacheSize, "Cache Size", indentLevel: 1);
                XsollaEditorCommon.CreateNumberInput(
                    v => { if (v > 0) editedServerConfig.MaximumCacheLifeTime = Mathf.FloorToInt((float)v); },
                    editedServerConfig.MaximumCacheLifeTime, "Cache Life Time (Seconds)", indentLevel: 1);
            }

            // Websocket Configs
            if (XsollaEditorCommon.CreateFoldout("Websocket Configs", foldoutStatus))
            {
                XsollaEditorCommon.CreateToggleInput(v => editedServerConfig.ServerUseAMS = v, editedServerConfig.ServerUseAMS, "DS Connection Using AMS", indentLevel: 1);
                XsollaEditorCommon.CreateNumberInput(v => editedServerConfig.DSHubReconnectTotalTimeout = (int)v, editedServerConfig.DSHubReconnectTotalTimeout, "DS Hub Reconnect Total Timeout (ms)", indentLevel: 1);
                XsollaEditorCommon.CreateNumberInput(v => editedServerConfig.AMSReconnectTotalTimeout = (int)v, editedServerConfig.AMSReconnectTotalTimeout, "AMS Reconnect Total Timeout (ms)", indentLevel: 1);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();

            if (GUILayout.Button("Save"))
            {
                editedServerOAuthConfig.Expand();

                var targetEnvironment = XsollaEditorCommon.GetEnvironment(XsollaEditorCommon.EnvironmentList, temporaryEnvironmentSetting);

                originalServerOAuthConfigs = AccelByteSettingsV2.SetOAuthByEnvironment(originalServerOAuthConfigs, editedServerOAuthConfig.ShallowCopy(), targetEnvironment);
                originalServerConfigs = AccelByteSettingsV2.SetSDKConfigByEnvironment(originalServerConfigs, editedServerConfig.ShallowCopy(), targetEnvironment);

                AccelByteSettingsV2.SaveConfig(originalServerOAuthConfigs, AccelByteSettingsV2.OAuthFullPath(string.Empty, isServer: true));
                AccelByteSettingsV2.SaveConfig(originalServerConfigs, AccelByteSettingsV2.SDKConfigFullPath(isServer: true));

                Debug.Log("Xsolla Backend SDK server config saved.");
            }

            EditorGUILayout.EndVertical();
        }
    }
}
