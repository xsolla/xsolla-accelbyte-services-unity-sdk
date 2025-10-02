// Copyright (c) 2025 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using UnityEditor;
using AccelByte.Editor;

namespace Xsolla.Backend
{
    public class XsollaSDKServerSettingsEditor : EditorWindow
    {
        [MenuItem("Window/Xsolla/Edit Backend Server Settings")]
        public static void OpenWindow()
        {
            AccelByteServerSettingsEditor.OpenWindow();
        }
    }
}
