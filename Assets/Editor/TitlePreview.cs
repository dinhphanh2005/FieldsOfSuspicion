using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Chụp màn tiêu đề + phần mở đầu ngay trong Editor, KHÔNG vào Play mode, không đụng scene đang mở
// (dựng trong preview scene riêng rồi huỷ). Ảnh lưu vào DevShots/title_preview/.
public static class TitlePreview
{
    [MenuItem("FoS/Dev/Chụp màn tiêu đề (không chạy game)")]
    public static void Capture()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("[TitlePreview] Thoát Play mode trước."); return; }
        UIFactory.SetSpriteCatalog(AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites" })
            .Select(g => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(s => s && !AssetDatabase.GetAssetPath(s).StartsWith("Assets/Sprites/Characters/")).ToArray());

        var scene = EditorSceneManager.NewPreviewScene();
        var dir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "DevShots", "title_preview");
        Directory.CreateDirectory(dir);
        try
        {
            var camGo = new GameObject("PreviewCam");
            SceneManagerMove(camGo, scene);
            var cam = camGo.AddComponent<Camera>();
            cam.scene = scene;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            var rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;

            var canvasGo = new GameObject("PreviewCanvas", typeof(RectTransform));
            SceneManagerMove(canvasGo, scene);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            var root = UIFactory.MakeRect(canvasGo.transform, "Title_Screen", Vector2.zero, Vector2.one);
            var view = root.gameObject.AddComponent<TitleView>();
            view.Init(null);

            string[] names = { "menu", "settings", "intro_first", "intro_sign", "intro_signed" };
            for (int mode = 0; mode < names.Length; mode++)
            {
                view.PreviewState(mode);
                Canvas.ForceUpdateCanvases();
                foreach (var l in canvasGo.GetComponentsInChildren<LayoutGroup>(true))
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)l.transform);
                foreach (var t in canvasGo.GetComponentsInChildren<TMPro.TMP_Text>(true)) t.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases();
                cam.Render();
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                tex.Apply();
                RenderTexture.active = prev;
                File.WriteAllBytes(Path.Combine(dir, $"{mode}_{names[mode]}.png"), tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
            }
            cam.targetTexture = null;
            rt.Release();
            Object.DestroyImmediate(rt);
            Debug.Log("[TitlePreview] Đã chụp " + names.Length + " ảnh vào " + dir);
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    static void SceneManagerMove(GameObject go, UnityEngine.SceneManagement.Scene scene) =>
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
}
