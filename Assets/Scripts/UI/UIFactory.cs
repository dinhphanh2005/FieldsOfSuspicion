using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Hàm tiện ích dựng UI bằng code. Giao diện cố ý đơn giản (quyết định UI "nhanh, gọn").
public static class UIFactory
{
    public static readonly Color Ink       = Hex("3A2E25");   // chữ tối trên nền sáng
    public static readonly Color Paper     = Hex("F4ECD8");   // thẻ, khung thoại, nút
    public static readonly Color Light     = Hex("EDE3D1");   // chữ sáng trên nền tối
    public static readonly Color Muted     = Hex("8C7B6B");
    public static readonly Color NightBox  = Hex("332E2A");
    public static readonly Color Highlight = Hex("E0B04B");   // nút đang chọn / nút trong lộ trình
    public static readonly Color ButtonTan = Hex("D9C9A8");   // nút nằm trên nền giấy (khung thoại)

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out var c);
        return c;
    }

    // RectTransform con, kéo giãn theo anchor, chừa lề "pad" mỗi phía
    public static RectTransform MakeRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, float pad = 0)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(pad, pad);
        rt.offsetMax = new Vector2(-pad, -pad);
        return rt;
    }

    // RectTransform đặt theo tâm panel + kích thước (dùng cho bản đồ)
    public static RectTransform MakeRectAt(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var rt = MakeRect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        return rt;
    }

    public static Image MakeBox(RectTransform rt, Color color, bool raycast = true)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = raycast;
        return img;
    }

    public static TextMeshProUGUI MakeText(RectTransform rt, string text, float size, Color color,
                                           TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.richText = true;
        return t;
    }

    public static Button MakeButton(Transform parent, string name, string label, UnityAction onClick, float fontSize = 18)
    {
        var rt = MakeRect(parent, name, Vector2.zero, Vector2.one);
        var img = MakeBox(rt, Paper);
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        ApplyColors(btn);
        if (onClick != null) btn.onClick.AddListener(onClick);
        var labelRect = MakeRect(rt, "Label", Vector2.zero, Vector2.one, 6);
        var t = MakeText(labelRect, label, fontSize, Ink, TextAlignmentOptions.Center);
        t.raycastTarget = false;
        return btn;
    }

    // Màu trạng thái của nút. Nút bị tắt: xám đục (không trong suốt), để không lộ đường kẻ phía sau.
    public static void ApplyColors(Button btn)
    {
        var cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1);
        cb.pressedColor     = new Color(0.80f, 0.80f, 0.80f, 1);
        cb.selectedColor    = Color.white;
        cb.disabledColor    = new Color(0.62f, 0.60f, 0.57f, 1);
        btn.colors = cb;
    }

    // Bật/tắt nút + làm mờ chữ khi tắt
    public static void SetInteractable(Button b, bool on)
    {
        b.interactable = on;
        var t = b.GetComponentInChildren<TMP_Text>();
        if (t) t.alpha = on ? 1f : 0.45f;
    }

    // Chỉnh các phần tử dựng tay trong scene cho đồng bộ:
    // tiêu đề panel ("Text (TMP)") nằm dải trên cùng, không xuống dòng; các nút Btn_* to hơn, cách mép đều.
    public static void StylePanel(GameObject panel)
    {
        var title = panel.transform.Find("Text (TMP)");
        if (title)
        {
            var rt = (RectTransform)title;
            rt.anchorMin = new Vector2(0, 0.85f);
            rt.anchorMax = new Vector2(1, 0.97f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var t = title.GetComponent<TMP_Text>();
            t.alignment = TextAlignmentOptions.Center;
            t.fontSize = 36;
            t.fontStyle = FontStyles.Bold;
        }

        foreach (Transform child in panel.transform)
        {
            if (!child.name.StartsWith("Btn_")) continue;
            var rt = (RectTransform)child;
            var corner = rt.anchorMin;   // các nút này đều neo ở một góc
            rt.anchorMax = corner;
            rt.pivot = corner;
            rt.sizeDelta = new Vector2(200, 54);
            rt.anchoredPosition = new Vector2(corner.x > 0.5f ? -32 : 32, corner.y > 0.5f ? -32 : 32);
            var label = child.GetComponentInChildren<TMP_Text>();
            if (label) { label.fontSize = 22; label.color = Ink; }
            var btn = child.GetComponent<Button>();
            if (btn) { ApplyColors(btn); ((Image)btn.targetGraphic).color = Paper; }
        }
    }

    public static void SetLabel(Button b, string label) => b.GetComponentInChildren<TMP_Text>().text = label;

    public static void SetColor(Button b, Color c) => ((Image)b.targetGraphic).color = c;

    public static LayoutElement Size(Component c, float minHeight)
    {
        var le = c.gameObject.AddComponent<LayoutElement>();
        le.minHeight = minHeight;
        le.preferredHeight = minHeight;
        le.flexibleHeight = 0;   // không giãn chiếm chỗ trống trong VerticalLayoutGroup
        return le;
    }

    // Trong HorizontalLayoutGroup: các phần tử chia đều chiều ngang, bất kể chữ bên trong dài ngắn
    public static void EqualWidth(Component c)
    {
        var le = c.gameObject.AddComponent<LayoutElement>();
        le.minWidth = 0;
        le.preferredWidth = 0;
        le.flexibleWidth = 1;
    }

    public static VerticalLayoutGroup VList(RectTransform rt, float spacing, int pad)
    {
        var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = spacing;
        v.padding = new RectOffset(pad, pad, pad, pad);
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        return v;
    }

    public static HorizontalLayoutGroup HList(RectTransform rt, float spacing, int pad)
    {
        var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = spacing;
        h.padding = new RectOffset(pad, pad, pad, pad);
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;
        return h;
    }

    // Xóa hết con. Tắt trước rồi mới Destroy để layout bỏ qua ngay trong frame này.
    public static void Clear(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            var child = t.GetChild(i).gameObject;
            child.SetActive(false);
            Object.Destroy(child);
        }
    }
}
