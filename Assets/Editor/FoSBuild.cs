#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class FoSBuild
{
    const string AppName = "FieldsOfSuspicion";
    static string Root => Path.Combine(Path.GetDirectoryName(Application.dataPath), "Builds");

    [MenuItem("FoS/Build/macOS")]
    public static void MacOS() => Run(BuildTarget.StandaloneOSX);

    [MenuItem("FoS/Build/Windows")]
    public static void Windows() => Run(BuildTarget.StandaloneWindows64);

    [MenuItem("FoS/Build/macOS + Windows")]
    public static void Both() => Run(BuildTarget.StandaloneOSX, BuildTarget.StandaloneWindows64);

    static void Run(params BuildTarget[] targets)
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[Build] Thoát Play mode trước khi build."); return; }
        Directory.CreateDirectory(Root);
        var original = EditorUserBuildSettings.activeBuildTarget;
        var log = new StringBuilder();
        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        foreach (var target in targets)
        {
            string path = target == BuildTarget.StandaloneOSX
                ? Path.Combine(Root, "macOS", AppName + ".app")
                : Path.Combine(Root, "Windows", AppName + ".exe");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = path,
                target = target,
                options = BuildOptions.None,
            });
            var s = report.summary;
            string line = $"{System.DateTime.Now:yyyy-MM-dd HH:mm} {target}: {s.result} · {s.totalErrors} lỗi · {s.totalWarnings} cảnh báo · " +
                          $"{s.totalSize / (1024f * 1024f):0.0} MB · {s.totalTime.TotalSeconds:0}s → {path}";
            Debug.Log("[Build] " + line);
            log.AppendLine(line);
            foreach (var step in report.steps)
                foreach (var m in step.messages.Where(m => m.type == LogType.Error || m.type == LogType.Exception))
                    log.AppendLine("   !! " + m.content);
        }
        File.AppendAllText(Path.Combine(Root, "build.log"), log.ToString());
        if (EditorUserBuildSettings.activeBuildTarget != original)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(original), original);
    }
}
#endif
