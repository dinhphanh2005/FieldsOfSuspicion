#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class DevAutoTourMenu
{
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

    [MenuItem("FoS/Dev/Tự chơi không bán hàng (kiểm tra thư nhắc + Bị đuổi nhà)")]
    static void RunBroke()
    {
        if (EditorApplication.isPlaying) { UnityEngine.Debug.Log("[Tour] Hãy dừng Play trước khi chạy tự chơi."); return; }
        Arm("broke");
        EditorApplication.isPlaying = true;
    }

    [MenuItem("FoS/Dev/Tự chơi: tố nhầm Rook (Đào tẩu)")]
    static void Run_defection()
    {
        if (EditorApplication.isPlaying) { UnityEngine.Debug.Log("[Tour] Hãy dừng Play trước khi chạy tự chơi."); return; }
        Arm("defection");
        EditorApplication.isPlaying = true;
    }

    [MenuItem("FoS/Dev/Tự chơi: tố Shrike, bằng chứng yếu")]
    static void Run_weak()
    {
        if (EditorApplication.isPlaying) { UnityEngine.Debug.Log("[Tour] Hãy dừng Play trước khi chạy tự chơi."); return; }
        Arm("weak");
        EditorApplication.isPlaying = true;
    }

    [MenuItem("FoS/Dev/Tự chơi: không gửi báo cáo cuối (Bị bỏ rơi)")]
    static void Run_cold()
    {
        if (EditorApplication.isPlaying) { UnityEngine.Debug.Log("[Tour] Hãy dừng Play trước khi chạy tự chơi."); return; }
        Arm("cold");
        EditorApplication.isPlaying = true;
    }

    [MenuItem("FoS/Dev/Tự chơi: không trồng rau (kiểm tra hết rau)")]
    static void Run_nofarm()
    {
        if (EditorApplication.isPlaying) { UnityEngine.Debug.Log("[Tour] Hãy dừng Play trước khi chạy tự chơi."); return; }
        Arm("nofarm");
        EditorApplication.isPlaying = true;
    }

    [MenuItem("FoS/Dev/Tự chơi: hỏi dồn mọi khách")]
    static void Run_pressall()
    {
        if (EditorApplication.isPlaying) { UnityEngine.Debug.Log("[Tour] Hãy dừng Play trước khi chạy tự chơi."); return; }
        Arm("pressall");
        EditorApplication.isPlaying = true;
    }

    [MenuItem("FoS/Dev/Tự chơi: bớt giá rồi hỏi dồn mọi khách")]
    static void Run_pressdisc()
    {
        if (EditorApplication.isPlaying) { UnityEngine.Debug.Log("[Tour] Hãy dừng Play trước khi chạy tự chơi."); return; }
        Arm("pressdisc");
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
