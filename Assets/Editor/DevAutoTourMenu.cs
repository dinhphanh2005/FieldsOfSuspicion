#if UNITY_EDITOR
using UnityEditor;

// Menu chạy bản chơi tự động để rà UI (chỉ trong Editor).
public static class DevAutoTourMenu
{
    [MenuItem("FoS/Dev/Tự chơi một lượt (chụp màn hình)")]
    static void Run()
    {
        EditorPrefs.SetString(DevAutoTour.PrefKey, "rook-correct");
        if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
    }

    [MenuItem("FoS/Dev/Tự chơi theo Shrike (kiểm tra Bại lộ)")]
    static void RunShrike()
    {
        EditorPrefs.SetString(DevAutoTour.PrefKey, "shrike");
        if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
    }
}
#endif
