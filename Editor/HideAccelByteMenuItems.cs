using UnityEditor;

public static class HideAccelByteMenuItems
{
    [MenuItem("AccelByte/Edit Client Settings", true)]
    private static bool DisableClientSettings() => false;

    [MenuItem("AccelByte/Edit Server Settings", true)]
    private static bool DisableServerSettings() => false;
}
