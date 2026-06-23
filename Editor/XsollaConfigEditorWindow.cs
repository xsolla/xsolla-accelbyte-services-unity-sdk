// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using UnityEditor;
using UnityEngine;

namespace Xsolla.Backend
{
    /// <summary>
    /// Xsolla-branded settings editor window that creates and edits a
    /// <see cref="XsollaConfig"/> ScriptableObject asset stored under a
    /// <c>Resources</c> folder so the runtime loader can find it via
    /// <c>Resources.Load&lt;XsollaConfig&gt;("XsollaConfig")</c>.
    /// </summary>
    public class XsollaConfigEditorWindow : EditorWindow
    {
        private const string WindowTitle = "Xsolla Backend SDK Settings";
        private const string ConfigAssetPath = "Assets/Resources/XsollaConfig.asset";
        private const string ResourcesFolderPath = "Assets/Resources";

        private static XsollaConfigEditorWindow instance;

        private XsollaConfig config;
        private SerializedObject serializedConfig;
        private Vector2 scrollPos;
        private bool hasUnsavedChanges;

        [MenuItem("Xsolla/Backend SDK/Settings")]
        public static void OpenWindow()
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }

            instance = GetWindow<XsollaConfigEditorWindow>(
                WindowTitle,
                true,
                System.Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll"));
            instance.Show();
        }

        private void OnEnable()
        {
            instance = this;
            LoadConfig();
        }

        private void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        // ──────────────────────────────────────────────
        //  Asset loading / creation
        // ──────────────────────────────────────────────

        private void LoadConfig()
        {
            // Try loading through AssetDatabase first (editor-time path).
            config = AssetDatabase.LoadAssetAtPath<XsollaConfig>(ConfigAssetPath);

            if (config == null)
            {
                // Fall back to any Resources folder in the project.
                config = Resources.Load<XsollaConfig>("XsollaConfig");
            }

            if (config != null)
            {
                serializedConfig = new SerializedObject(config);
            }
            else
            {
                serializedConfig = null;
            }

            hasUnsavedChanges = false;
        }

        private void CreateConfigAsset()
        {
            if (!AssetDatabase.IsValidFolder(ResourcesFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            config = CreateInstance<XsollaConfig>();
            AssetDatabase.CreateAsset(config, ConfigAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            serializedConfig = new SerializedObject(config);
        }

        // ──────────────────────────────────────────────
        //  GUI
        // ──────────────────────────────────────────────

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("Xsolla Backend SDK", titleStyle);
            GUILayout.Space(4);
            EditorGUILayout.LabelField("Settings", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(10);

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Settings editor is disabled during Play Mode.",
                    MessageType.Info,
                    wide: true);
                EditorGUILayout.EndVertical();
                return;
            }

            if (config == null)
            {
                EditorGUILayout.HelpBox(
                    "No Xsolla config asset found.\n" +
                    "Click the button below to create one in Assets/Resources/.",
                    MessageType.Warning,
                    wide: true);

                GUILayout.Space(5);

                if (GUILayout.Button("Create Xsolla Config Asset"))
                {
                    CreateConfigAsset();
                }

                EditorGUILayout.EndVertical();
                return;
            }

            serializedConfig.Update();

            scrollPos = EditorGUILayout.BeginScrollView(
                scrollPos,
                alwaysShowHorizontal: false,
                alwaysShowVertical: true);

            DrawCoreSettings();

            GUILayout.Space(10);

            DrawTurnSettings();

            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);

            DrawFooter();

            EditorGUILayout.EndVertical();

            if (serializedConfig.ApplyModifiedProperties())
            {
                hasUnsavedChanges = true;
            }
        }

        // ──────────────────────────────────────────────
        //  Section drawers
        // ──────────────────────────────────────────────

        private void DrawCoreSettings()
        {
            EditorGUILayout.LabelField("Core", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("environment"),
                new GUIContent("Environment", "Xsolla backend environment."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("gameNamespace"),
                new GUIContent("Game Namespace", "Game namespace from the Xsolla Publisher Account."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("clientId"),
                new GUIContent("Client ID", "OAuth client ID from the Xsolla Publisher Account."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("clientSecret"),
                new GUIContent("Client Secret", "OAuth client secret (server / confidential clients only)."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("publisherNamespace"),
                new GUIContent("Publisher Namespace", "Publisher-level namespace."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("baseUrl"),
                new GUIContent("Base URL", "Base URL of the Xsolla backend services."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("redirectUri"),
                new GUIContent("Redirect URI", "OAuth redirect URI from the Xsolla Publisher Account."));

            EditorGUI.indentLevel--;
        }

        private void DrawTurnSettings()
        {
            EditorGUILayout.LabelField("TURN / Relay", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("useTurnManager"),
                new GUIContent("Use TURN Manager", "Enable the Xsolla TURN relay manager for P2P connectivity."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("turnManagerServerUrl"),
                new GUIContent("TURN Manager URL", "URL of the Xsolla TURN manager service."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("turnServerHost"),
                new GUIContent("TURN Server Host", "Hostname or IP of the Xsolla TURN relay server."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("turnServerPort"),
                new GUIContent("TURN Server Port", "Port of the Xsolla TURN relay server."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("turnServerUsername"),
                new GUIContent("TURN Server Username", "Username for TURN relay server authentication."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("turnServerPassword"),
                new GUIContent("TURN Server Password", "Password for TURN relay server authentication."));

            EditorGUILayout.PropertyField(
                serializedConfig.FindProperty("turnServerSecret"),
                new GUIContent("TURN Server Secret", "Shared secret for short-lived TURN credential generation."));

            EditorGUI.indentLevel--;
        }

        private void DrawFooter()
        {
            if (hasUnsavedChanges)
            {
                EditorGUILayout.HelpBox("Unsaved changes", MessageType.Warning, wide: true);
            }
            else
            {
                EditorGUILayout.HelpBox("No changes detected", MessageType.Info, wide: true);
            }

            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                hasUnsavedChanges = false;
                Debug.Log("Xsolla Backend SDK config saved.");
            }

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select Config Asset", GUILayout.Width(160)))
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
        }
    }
}
