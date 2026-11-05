using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class GameViewSizeMenu
{
    [MenuItem("FoS/Dev/Khung hình/16:9 (1280x720)")] static void Wide() => Set(1280, 720);
    [MenuItem("FoS/Dev/Khung hình/16:10 (1440x900)")] static void Mac() => Set(1440, 900);
    [MenuItem("FoS/Dev/Khung hình/4:3 (1024x768)")] static void Classic() => Set(1024, 768);
    [MenuItem("FoS/Dev/Khung hình/21:9 (2520x1080)")] static void Ultra() => Set(2520, 1080);
    [MenuItem("FoS/Dev/Khung hình/Nhỏ 800x600")] static void Small() => Set(800, 600);
    [MenuItem("FoS/Dev/Khung hình/Laptop 1366x768")] static void Laptop() => Set(1366, 768);
    [MenuItem("FoS/Dev/Khung hình/4K 3840x2160")] static void Uhd() => Set(3840, 2160);

    // Unity không có API công khai để đổi khung Game view, nên dùng reflection vào GameViewSizes (chỉ dùng trong Editor để kiểm tra bố cục).
    static void Set(int w, int h)
    {
        const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        var asm = typeof(Editor).Assembly;
        var sizesType = asm.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance").GetValue(null);
        var groupType = sizesType.GetProperty("currentGroupType", All).GetValue(sizes);
        var group = sizesType.GetMethod("GetGroup", All).Invoke(sizes, new[] { groupType });
        var groupT = group.GetType();
        int count = (int)groupT.GetMethod("GetTotalCount", All).Invoke(group, null);
        int index = -1;
        for (int i = 0; i < count; i++)
        {
            var size = groupT.GetMethod("GetGameViewSize", All).Invoke(group, new object[] { i });
            var st = size.GetType();
            if ((int)st.GetProperty("width", All).GetValue(size) == w && (int)st.GetProperty("height", All).GetValue(size) == h &&
                (int)st.GetProperty("sizeType", All).GetValue(size) == 1) { index = i; break; }
        }
        if (index < 0)
        {
            var sizeType = asm.GetType("UnityEditor.GameViewSize");
            var kind = Enum.ToObject(asm.GetType("UnityEditor.GameViewSizeType"), 1);
            var created = Activator.CreateInstance(sizeType, kind, w, h, $"FoS {w}x{h}");
            groupT.GetMethod("AddCustomSize", All).Invoke(group, new[] { created });
            index = (int)groupT.GetMethod("GetTotalCount", All).Invoke(group, null) - 1;
        }
        var gvType = asm.GetType("UnityEditor.GameView");
        var gv = EditorWindow.GetWindow(gvType, false, null, false);
        var prop = gvType.GetProperty("selectedSizeIndex", All);
        if (prop != null && prop.CanWrite) prop.SetValue(gv, index);
        else gvType.GetMethod("SizeSelectionCallback", All)?.Invoke(gv, new object[] { index, null });
        gv.Repaint();
        Debug.Log($"[Khung hình] Game view = {w}x{h} (mục {index})");
    }
}
