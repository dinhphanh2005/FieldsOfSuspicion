#if UNITY_EDITOR
using UnityEditor;

// Menu chạy bản chơi tự động để rà UI (chỉ trong Editor).
[InitializeOnLoad]
public static class DevAutoTourMenu
{
    // Thoát Play mode thì xoá cờ, để lần bấm Play sau là chơi bình thường
    static DevAutoTourMenu()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredEditMode) Disarm();
        };
    }

    static void Arm(string plan)
    {
        EditorPrefs.SetString(DevAutoTour.PrefKey, plan);
        EditorPrefs.SetString(DevAutoTour.PrefKey + ".at", System.DateTime.UtcNow.Ticks.ToString());
    }

    static void Disarm()
    {
        EditorPrefs.DeleteKey(DevAutoTour.PrefKey);
        EditorPrefs.DeleteKey(DevAutoTour.PrefKey + ".at");
    }

    [MenuItem("FoS/Dev/Tự chơi một lượt (chụp màn hình)")]
    static void Run()
    {
        if (EditorApplication.isPlaying) { UnityEngine.Debug.Log("[Tour] Hãy dừng Play trước khi chạy tự chơi."); return; }
        Arm("rook-correct");
        EditorApplication.isPlaying = true;
    }

    [MenuItem("FoS/Dev/Tự chơi theo Shrike (kiểm tra Bại lộ)")]
    static void RunShrike()
    {
        if (EditorApplication.isPlaying) { UnityEngine.Debug.Log("[Tour] Hãy dừng Play trước khi chạy tự chơi."); return; }
        Arm("shrike");
        EditorApplication.isPlaying = true;
    }
}
#endif
