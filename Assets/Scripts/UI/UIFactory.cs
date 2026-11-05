using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public static class UIFactory
{
    public const float HudHeight = 44;
    public const float BarHeight = 68;

    public static readonly Color Ink       = Hex("3A2E25");
    public static readonly Color Paper     = Hex("F4ECD8");
    public static readonly Color Light     = Hex("EDE3D1");
    public static readonly Color Muted     = Hex("8C7B6B");
    public static readonly Color NightBox  = Hex("2F2924");
    public static readonly Color Highlight = Hex("E0B04B");
    public static readonly Color ButtonTan = Hex("E2D4B5");
    public static readonly Color Frame     = Hex("2A221C");
    public static readonly Color FrameLine = Hex("4A3B2F");
    public static readonly Color Red       = Hex("8C3A2B");
    public static readonly Color Gold      = Hex("E0B04B");
    public static readonly Color PaperEdge = Hex("C9B994");

    public static TMP_FontAsset TitleFont;
    static readonly Dictionary<string, Sprite> spriteByName = new Dictionary<string, Sprite>();

    public static void SetSpriteCatalog(Sprite[] sprites)
    {
        spriteByName.Clear();
        if (sprites == null) return;
        foreach (var sprite in sprites)
        {
            if (!sprite) continue;
            spriteByName[sprite.name] = sprite;

            if (sprite.name.EndsWith("_0"))
            {
                var alias = sprite.name.Substring(0, sprite.name.Length - 2);
                if (!spriteByName.ContainsKey(alias)) spriteByName[alias] = sprite;
            }
        }
    }

    public static Sprite SpriteAsset(string name) =>
        spriteByName.TryGetValue(name, out var sprite) ? sprite : null;

    public static void Skin(Image image, string name, Image.Type type = Image.Type.Simple)
    {
        var sprite = SpriteAsset(name);
        if (!image || !sprite) return;
        image.sprite = sprite;
        image.type = type;
        image.preserveAspect = false;
    }

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out var c);
        return c;
    }

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

    public static RectTransform MakeRectAt(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var rt = MakeRect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        return rt;
    }

    public static RectTransform Place(Transform parent, string name, float x, float y, float w, float h)
    {
        var rt = MakeRect(parent, name, new Vector2(0, 1), new Vector2(0, 1));
        rt.pivot = new Vector2(0, 1);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, -y);
        return rt;
    }

    public const float RefWidth = 1280;
    public const float RefHeight = 720;

    // Canvas dùng chế độ Expand nên màn 16:10 hay 4:3 rộng/cao hơn 1280x720; nội dung đặt trong khung cố định ở giữa để không lệch hay tràn.
    public static RectTransform MakeSafe(Transform parent, string name, float w, float h)
    {
        var rt = MakeRect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = Vector2.zero;
        return rt;
    }

    public static RectTransform MakeStage(Transform panel)
    {
        var rt = MakeRect(panel, "Stage", Vector2.zero, Vector2.one);
        rt.offsetMin = new Vector2(0, BarHeight);
        rt.offsetMax = new Vector2(0, -HudHeight);
        return MakeSafe(rt, "Stage_Safe", RefWidth, RefHeight - BarHeight - HudHeight);
    }

    public static Image MakeBox(RectTransform rt, Color color, bool raycast = true)
    {
        var img = rt.gameObject.AddComponent<LayoutSafeImage>();
        img.color = color;
        img.raycastTarget = raycast;
        return img;
    }

    public static Image MakeSurface(RectTransform rt, Color color, float radius = 7, bool raycast = true)
    {
        var img = MakeBox(rt, color, raycast);
        Round(img, radius);
        var outline = rt.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(PaperEdge.r, PaperEdge.g, PaperEdge.b, 0.58f);
        outline.effectDistance = new Vector2(1, -1);
        return img;
    }

    static Sprite rounded;
    static Sprite RoundedSprite
    {
        get
        {
            if (rounded) return rounded;
            const int r = 16, size = r * 2 + 2;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0, Mathf.Abs(x + 0.5f - size / 2f) - 1) , dy = Mathf.Max(0, Mathf.Abs(y + 0.5f - size / 2f) - 1);
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - d + 0.5f)));
            }
            tex.Apply();
            rounded = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0,
                                    SpriteMeshType.FullRect, new Vector4(r, r, r, r));
            return rounded;
        }
    }

    static Sprite arrow;
    public static Sprite ArrowSprite
    {
        get
        {
            if (arrow) return arrow;
            const int n = 32;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float half = (n / 2f) * (1f - (x + 0.5f) / n);
                float d = half - Mathf.Abs(y + 0.5f - n / 2f);
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(d + 0.5f)));
            }
            tex.Apply();
            arrow = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100);
            return arrow;
        }
    }

    public static void Round(Image img, float radius)
    {
        img.sprite = RoundedSprite;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 16f / Mathf.Max(1, radius);
    }

    static Sprite noise;
    public static Image AddGrain(RectTransform parent, float alpha)
    {
        if (!noise)
        {
            const int n = 64;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point };
            var rnd = new System.Random(7);
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float v = (float)rnd.NextDouble();
                tex.SetPixel(x, y, v > 0.5f ? new Color(1, 1, 1, (v - 0.5f) * 0.6f) : new Color(0, 0, 0, (0.5f - v) * 0.8f));
            }
            tex.Apply();
            noise = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100);
        }
        var rt = MakeRect(parent, "Grain", Vector2.zero, Vector2.one);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = noise;
        img.type = Image.Type.Tiled;
        img.pixelsPerUnitMultiplier = 0.5f;
        img.color = new Color(1, 1, 1, alpha);
        img.raycastTarget = false;
        return img;
    }

    public static void AddShadow(Component c, float distance = 5)
    {
        var s = c.gameObject.AddComponent<Shadow>();
        s.effectColor = new Color(0, 0, 0, 0.4f);
        s.effectDistance = new Vector2(distance * 0.4f, -distance);
    }

    public static Outline AddOutline(Component c, Color color, float width = 2)
    {
        var o = c.gameObject.AddComponent<Outline>();
        o.effectColor = color;
        o.effectDistance = new Vector2(width, -width);
        return o;
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
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.Normal;
        return t;
    }

    public static TextMeshProUGUI MakeTitle(RectTransform rt, string text, float size, Color color,
                                            TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var t = MakeText(rt, text, size, color, align);
        if (TitleFont) t.font = TitleFont;
        else t.fontStyle = FontStyles.Bold;
        return t;
    }

    public static TextMeshProUGUI MakeLabel(RectTransform rt, string text, Color color, float size = 15)
    {
        var t = MakeTitle(rt, text, size, color);
        t.characterSpacing = 6;
        return t;
    }

    public static TextMeshProUGUI AddLine(RectTransform list, string text, float size, Color color,
                                          TextAlignmentOptions align = TextAlignmentOptions.TopLeft, bool title = false)
    {
        var rt = MakeRect(list, "Line", Vector2.zero, Vector2.one);
        return title ? MakeTitle(rt, text, size, color, align) : MakeText(rt, text, size, color, align);
    }

    public static Image AddRule(RectTransform list, Color color, float thickness = 1)
    {
        var rt = MakeRect(list, "Rule", Vector2.zero, Vector2.one);
        Size(rt, Mathf.Max(2, thickness));
        return MakeBox(rt, color, false);
    }

    public static Button MakeButton(Transform parent, string name, string label, UnityAction onClick, float fontSize = 18)
    {
        var rt = MakeRect(parent, name, Vector2.zero, Vector2.one);
        var img = MakeSurface(rt, Paper, 5);
        Skin(img, "btn_choice_normal");
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        ApplyColors(btn);
        btn.onClick.AddListener(() => GameAudio.Sfx("click", 0.7f));
        if (onClick != null) btn.onClick.AddListener(onClick);
        var labelRect = MakeRect(rt, "Label", Vector2.zero, Vector2.one, 6);
        MakeText(labelRect, label, fontSize, Ink, TextAlignmentOptions.Center);
        var feel = rt.gameObject.AddComponent<ButtonFeel>();
        feel.content = labelRect;
        return btn;
    }

    public static void ApplyColors(Button btn)
    {
        var cb = btn.colors;
        cb.normalColor      = new Color(0.92f, 0.92f, 0.92f, 1);
        cb.highlightedColor = Color.white;
        cb.pressedColor     = new Color(0.80f, 0.80f, 0.80f, 1);
        cb.selectedColor    = new Color(0.92f, 0.92f, 0.92f, 1);
        cb.disabledColor    = new Color(0.66f, 0.64f, 0.61f, 1);
        cb.fadeDuration     = 0.08f;
        btn.colors = cb;
    }

    static void ApplySpriteStates(Button btn)
    {
        var hover = SpriteAsset("btn_choice_hover");
        var pressed = SpriteAsset("btn_choice_pressed");
        var disabled = SpriteAsset("btn_choice_disabled");
        if (!hover || !pressed || !disabled) return;
        btn.transition = Selectable.Transition.SpriteSwap;
        btn.spriteState = new SpriteState
        {
            highlightedSprite = hover,
            pressedSprite = pressed,
            disabledSprite = disabled
        };
    }

    public static void SetInteractable(Button b, bool on)
    {
        b.interactable = on;
        foreach (var t in b.GetComponentsInChildren<TMP_Text>()) t.alpha = on ? 1f : 0.5f;
    }

    public static void SetLabel(Button b, string label) => b.GetComponentInChildren<TMP_Text>().text = label;

    public static void SetColor(Button b, Color c) => ((Image)b.targetGraphic).color = c;

    public static RectTransform MakeActionBar(Transform panel)
    {
        var bar = MakeRect(panel, "ActionBar", Vector2.zero, new Vector2(1, 0));
        bar.pivot = new Vector2(0.5f, 0);
        bar.sizeDelta = new Vector2(0, BarHeight);
        MakeBox(bar, Frame);

        var line = MakeRect(bar, "TopLine", new Vector2(0, 1), Vector2.one);
        line.sizeDelta = new Vector2(0, 1);
        line.pivot = new Vector2(0.5f, 1);
        MakeBox(line, FrameLine, false);
        line.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

        var h = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(24, 24, 11, 11);
        h.spacing = 12;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;
        return bar;
    }

    public static Button BarButton(RectTransform bar, string label, UnityAction onClick, bool primary = false)
    {
        var b = MakeButton(bar, "Btn_" + (primary ? "Primary" : "Secondary"), label, onClick, 16);
        var h = b.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(20, 20, 0, 0);
        h.spacing = 8;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;
        var t = b.GetComponentInChildren<TextMeshProUGUI>();
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.margin = Vector4.zero;
        b.GetComponent<ButtonFeel>().content = null;
        string iconName = IconFor(label);
        var iconSprite = string.IsNullOrEmpty(iconName) ? null : SpriteAsset(iconName);
        if (iconSprite)
        {
            var iconRt = MakeRect(b.transform, "Icon", Vector2.zero, Vector2.one);
            iconRt.SetAsFirstSibling();
            var le = iconRt.gameObject.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = 20;
            le.flexibleWidth = 0;
            var icon = iconRt.gameObject.AddComponent<Image>();
            icon.sprite = iconSprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }
        if (primary)
        {
            SetColor(b, Red);
            Skin((Image)b.targetGraphic, "btn_primary");
            t.color = Light;
            var primaryLayout = b.gameObject.AddComponent<LayoutElement>();
            primaryLayout.minWidth = 200;
        }
        else Skin((Image)b.targetGraphic, "btn_secondary");
        AddShadow(b, 2);
        return b;
    }

    static string IconFor(string label)
    {
        string s = (label ?? string.Empty).ToLowerInvariant();
        if (s.Contains("ruộng") || s.Contains("thu hoạch")) return "icon_carrot";
        if (s.Contains("sổ tay") || s.Contains("gấp sổ")) return "icon_notebook";
        if (s.Contains("bảng tin")) return "icon_board";
        if (s.Contains("ở nhà") || s.Contains("nghỉ")) return "icon_stay_home";
        if (s.Contains("giao") || s.Contains("hốc cây") || s.Contains("đêm") || s.Contains("buổi tối") || s.Contains("lộ trình")) return "icon_lantern";
        if (s.Contains("tiệm") || s.Contains("bán")) return "icon_coin";
        if (s.Contains("thư") || s.Contains("báo") || s.Contains("lệnh")) return "icon_letter";
        if (s.Contains("chơi lại") || s.Contains("tiếp tục")) return "icon_sun";
        return null;
    }

    public static void AddButtonIcon(Button button, string spriteName)
    {
        var sprite = SpriteAsset(spriteName);
        if (!sprite) return;
        var label = button.GetComponentInChildren<TMP_Text>();
        if (!label) return;
        var iconRt = MakeRect(label.transform, "Icon", new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        iconRt.pivot = new Vector2(0, 0.5f);
        iconRt.sizeDelta = new Vector2(20, 20);
        iconRt.anchoredPosition = new Vector2(0, 0);
        var icon = iconRt.gameObject.AddComponent<Image>();
        Skin(icon, spriteName);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        label.margin = new Vector4(30, 0, 0, 0);
    }

    public static void BarSpacer(RectTransform bar)
    {
        var rt = MakeRect(bar, "Spacer", Vector2.zero, Vector2.one);
        rt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 0.25f;
    }

    public static TextMeshProUGUI BarHint(RectTransform bar, string text)
    {
        var rt = MakeRect(bar, "Hint", Vector2.zero, Vector2.one);
        var t = MakeText(rt, text, 14, Hex("9C8C7B"), TextAlignmentOptions.MidlineLeft);
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.enableAutoSizing = true;
        t.fontSizeMin = 10;
        t.fontSizeMax = 14;
        t.overflowMode = TextOverflowModes.Ellipsis;
        var layout = rt.gameObject.AddComponent<LayoutElement>();
        layout.minWidth = 72;
        layout.preferredWidth = 0;
        layout.flexibleWidth = 1;
        return t;
    }

    public static void StylePanel(GameObject panel)
    {
        foreach (Transform child in panel.transform)
            if (child.name == "Text (TMP)" || child.name.StartsWith("Btn_") || child.name == "Img_Customer")
                child.gameObject.SetActive(false);

        var bg = panel.GetComponent<Image>();
        if (bg) bg.color = Hex("3B2D22");
    }

    public static void SetBackground(GameObject panel, Sprite sprite, float dim = 0)
    {
        if (!sprite) return;
        var rt = MakeRect(panel.transform, "BG", Vector2.zero, Vector2.one);
        rt.SetAsFirstSibling();
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        var fit = rt.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fit.aspectRatio = sprite.rect.width / sprite.rect.height;

        if (dim > 0)
        {
            var d = MakeRect(panel.transform, "BG_Dim", Vector2.zero, Vector2.one);
            d.SetSiblingIndex(1);
            MakeBox(d, new Color(0, 0, 0, dim), false);
        }
    }

    public static T Get<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c ? c : go.AddComponent<T>();
    }

    public static LayoutElement Size(Component c, float minHeight)
    {
        var le = Get<LayoutElement>(c.gameObject);
        le.minHeight = minHeight;
        le.preferredHeight = minHeight;
        le.flexibleHeight = 0;
        return le;
    }

    public static void EqualWidth(Component c)
    {
        var le = Get<LayoutElement>(c.gameObject);
        le.minWidth = 0;
        le.preferredWidth = 0;
        le.flexibleWidth = 1;
    }

    public static LayoutElement Flexible(Component c, float minHeight)
    {
        var le = Get<LayoutElement>(c.gameObject);
        le.minHeight = minHeight;
        le.flexibleHeight = 1;
        return le;
    }

    public static VerticalLayoutGroup AutoHeight(RectTransform rt, int padX, int padTop, int padBottom, float spacing)
    {
        var v = VList(rt, spacing, 0);
        v.padding = new RectOffset(padX, padX, padTop, padBottom);
        rt.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return v;
    }

    public static void FitHeight(RectTransform rt, float maxHeight)
    {
        rt.localScale = Vector3.one;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        float h = rt.rect.height;
        float s = h > maxHeight ? maxHeight / h : 1f;
        rt.localScale = new Vector3(s, s, 1);
    }

    public static RectTransform MakeScroll(RectTransform root, out ScrollRect scroll)
    {
        MakeBox(root, new Color(0, 0, 0, 0));
        scroll = root.gameObject.AddComponent<ScrollRect>();

        var viewport = MakeRect(root, "Viewport", Vector2.zero, Vector2.one);
        viewport.gameObject.AddComponent<RectMask2D>();

        var content = MakeRect(viewport, "Content", new Vector2(0, 1), new Vector2(1, 1));
        content.pivot = new Vector2(0.5f, 1);
        var v = VList(content, 6, 0);
        v.childAlignment = TextAnchor.UpperLeft;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30;
        return content;
    }

    public static void ScrollToBottom(ScrollRect scroll)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
        scroll.verticalNormalizedPosition = 0;
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
