// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AccelByte.Models;
using UnityEditor;
using UnityEngine;

namespace Xsolla.Backend.Editor
{
    internal static class XsollaEditorCommon
    {
        private static GUIStyle requiredTextFieldGUIStyle;
        private static Texture2D xsollaLogo;
        private static string[] environmentList;

        // ──────────────────────────────────────────────
        //  Environment / Platform lists
        // ──────────────────────────────────────────────

        internal static string[] EnvironmentList
        {
            get
            {
                if (environmentList == null)
                {
                    var values = Enum.GetValues(typeof(SettingsEnvironment))
                        .Cast<SettingsEnvironment>()
                        .Where(e => e != SettingsEnvironment.Default)
                        .Select(e => e.ToString())
                        .ToList();
                    values.Insert(0, SettingsEnvironment.Default.ToString());
                    environmentList = values.ToArray();
                }
                return environmentList;
            }
        }

        internal static readonly string[] PlatformList =
        {
            "Default",
            PlatformType.Steam.ToString(),
            PlatformType.Apple.ToString(),
            PlatformType.iOS.ToString(),
            PlatformType.Android.ToString(),
            PlatformType.PS4.ToString(),
            PlatformType.PS5.ToString(),
            PlatformType.Live.ToString(),
            PlatformType.Nintendo.ToString()
        };

        internal static SettingsEnvironment GetEnvironment(string[] envList, int index)
        {
            var result = SettingsEnvironment.Default;
            try
            {
                if (Enum.TryParse(envList[index], out SettingsEnvironment parsed))
                {
                    result = parsed;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
            return result;
        }

        internal static string GetPlatformName(string[] platformList, int index)
        {
            return platformList[index] == "Default" ? "" : platformList[index];
        }

        // ──────────────────────────────────────────────
        //  Config comparison
        // ──────────────────────────────────────────────

        internal static bool CompareConfig<T>(T first, T second) where T : class
        {
            if (first == null && second == null) return true;
            if (first == null || second == null) return false;

            foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object a = field.GetValue(first);
                object b = field.GetValue(second);
                if (a != null && b == null) return false;
                if (a == null && b != null) return false;
                if (a != null && !a.Equals(b)) return false;
            }
            return true;
        }

        // ──────────────────────────────────────────────
        //  Logo
        // ──────────────────────────────────────────────

        internal static GUIStyle RequiredTextFieldGUIStyle
        {
            get
            {
                if (requiredTextFieldGUIStyle == null)
                {
                    requiredTextFieldGUIStyle = new GUIStyle();
                    requiredTextFieldGUIStyle.normal.textColor = Color.yellow;
                }
                return requiredTextFieldGUIStyle;
            }
        }

        internal static Texture2D XsollaLogo
        {
            get
            {
                if (xsollaLogo == null)
                {
                    xsollaLogo = Resources.Load<Texture2D>("xsolla-logo");
                }
                return xsollaLogo;
            }
        }

        // ──────────────────────────────────────────────
        //  UI helpers
        // ──────────────────────────────────────────────

        internal static void CreateNumberInput(
            Action<double> setter,
            double defaultValue,
            string fieldLabel,
            bool required = false,
            int indentLevel = 0)
        {
            EditorGUILayout.BeginHorizontal();
            AddIndent(indentLevel);
            EditorGUILayout.LabelField(fieldLabel);
            var newValue = EditorGUILayout.DoubleField(defaultValue);
            setter?.Invoke(newValue);
            EditorGUILayout.LabelField(required ? "Required" : "", RequiredTextFieldGUIStyle);
            EditorGUILayout.EndHorizontal();
        }

        internal static void CreateNumberInput(
            Action<float> setter,
            float defaultValue,
            string fieldLabel,
            bool required = false,
            int indentLevel = 0)
        {
            EditorGUILayout.BeginHorizontal();
            AddIndent(indentLevel);
            EditorGUILayout.LabelField(fieldLabel);
            var newValue = EditorGUILayout.FloatField(defaultValue);
            setter?.Invoke(newValue);
            EditorGUILayout.LabelField(required ? "Required" : "", RequiredTextFieldGUIStyle);
            EditorGUILayout.EndHorizontal();
        }

        internal static void CreateNumberInput(
            Action<int> setter,
            int defaultValue,
            string fieldLabel,
            bool required = false,
            int indentLevel = 0)
        {
            EditorGUILayout.BeginHorizontal();
            AddIndent(indentLevel);
            EditorGUILayout.LabelField(fieldLabel);
            var newValue = EditorGUILayout.IntField(defaultValue);
            setter?.Invoke(newValue);
            EditorGUILayout.LabelField(required ? "Required" : "", RequiredTextFieldGUIStyle);
            EditorGUILayout.EndHorizontal();
        }

        internal static void CreateTextInput(
            Action<string> setter,
            string defaultValue,
            string fieldLabel,
            bool required = false,
            bool @readonly = false,
            int indentLevel = 0)
        {
            EditorGUILayout.BeginHorizontal();
            AddIndent(indentLevel);
            EditorGUILayout.LabelField(fieldLabel);

            if (!@readonly)
            {
                var newValue = EditorGUILayout.TextField(defaultValue);
                setter?.Invoke(newValue);
                EditorGUILayout.LabelField(
                    required && string.IsNullOrEmpty(newValue) ? "Required" : "",
                    RequiredTextFieldGUIStyle);
            }
            else
            {
                EditorGUILayout.LabelField(defaultValue);
                EditorGUILayout.LabelField(string.Empty, RequiredTextFieldGUIStyle);
            }

            EditorGUILayout.EndHorizontal();
        }

        internal static void CreateToggleInput(
            Action<bool> setter,
            bool defaultValue,
            string fieldLabel,
            int indentLevel = 0)
        {
            EditorGUILayout.BeginHorizontal();
            AddIndent(indentLevel);
            EditorGUILayout.LabelField(fieldLabel);
            var newValue = EditorGUILayout.Toggle(defaultValue);
            setter?.Invoke(newValue);
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndHorizontal();
        }

        internal static bool CreateFoldout(
            string label,
            Dictionary<string, bool> foldoutStatus,
            int indentLevel = 0)
        {
            bool previous = foldoutStatus.ContainsKey(label) && foldoutStatus[label];
            EditorGUILayout.BeginHorizontal();
            AddIndent(indentLevel);
            foldoutStatus[label] = EditorGUILayout.Foldout(previous, label);
            EditorGUILayout.EndHorizontal();
            return foldoutStatus[label];
        }

        internal static void AddIndent(int depth)
        {
            GUILayout.Space(Mathf.Max(0, depth) * 15 + 4);
        }
    }
}
