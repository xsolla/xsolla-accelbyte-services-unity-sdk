// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using UnityEditor;
using AccelByte.Editor;

namespace Xsolla.GamingService
{
    public class XsollaSDKClientSettingsEditor : EditorWindow
    {
        [MenuItem("Window/Xsolla/Gaming Services Client Settings")]
        public static void OpenWindow()
        {
            AccelByteClientSettingsEditor.OpenWindow();
        }
    }
}