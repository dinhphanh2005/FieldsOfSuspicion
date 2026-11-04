using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Màn tiêu đề + cảnh mở đầu + ký tên.
//   Tiêu đề: ảnh cánh đồng phủ kín (zoom chậm), sương trôi, tên game lớn bên trái, menu chữ.
//   Ván mới → 4 khung chữ kể bối cảnh (bấm để tiếp, có "Bỏ qua") → ký tên thật → lá thư N1.
public class TitleView : MonoBehaviour
{
    DayManager dm;
    RectTransform bgRect, fogA, fogB;
    RawImage fogImgA, fogImgB;
    CanvasGroup titleGroup, menuGroup, settingsGroup, introGroup, nameGroup, blackGroup;
    RectTransform menuCol, settingsCol;
    TextMeshProUGUI feedback, introText, introHint, nameLine;
    TMP_InputField nameInput;
    Button introHit, skipButton;
    readonly List<(Button b, TextMeshProUGUI sub)> settingsItems = new List<(Button, TextMeshProUGUI)>();
    Button continueItem;
    TextMeshProUGUI continueSub;
    float t;
    bool advance;

    static readonly string[] IntroLines =
    {
        "Mùa thu năm ấy, con sông biên giới giữa Straka và Lenmar im lặng một cách đáng ngờ.",
        "Bên kia sông là làng Sellowe: ruộng cà rốt, một cái chợ phiên,\nvà đồn lính Lenmar của đại úy Doran.",
        "Văn phòng tình báo Straka cần một người ở đó.\nKhông phải lính. Một người bình thường, biết trồng rau, biết bán hàng,\nvà biết lắng nghe.",
        "Họ gọi người ấy là Wren.",
    };

    public void Init(DayManager manager, Sprite titleBg, Sprite introBg)
    {
        dm = manager;
        var root = (RectTransform)transform;

        // ---- Nền: ảnh phủ kín + zoom chậm ----
        UIFactory.MakeBox(root, UIFactory.Hex("120E0C"));
        bgRect = UIFactory.MakeRect(root, "BG", Vector2.zero, Vector2.one);
        if (titleBg)
        {
            var img = bgRect.gameObject.AddComponent<Image>();
            img.sprite = titleBg;
            img.raycastTarget = false;
            var fit = bgRect.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = titleBg.rect.width / titleBg.rect.height;
        }
        var fogTex = FogTexture();
        fogA = Fog(root, "FogA", fogTex, 0.30f, out fogImgA);
        fogB = Fog(root, "FogB", fogTex, 0.22f, out fogImgB);
        var shade = UIFactory.MakeRect(root, "Shade", Vector2.zero, new Vector2(0.72f, 1));
        var shadeImg = shade.gameObject.AddComponent<RawImage>();
        shadeImg.texture = Gradient(new Color(0.07f, 0.05f, 0.04f, 0.88f), new Color(0.07f, 0.05f, 0.04f, 0f));
        shadeImg.raycastTarget = false;

        // ---- Tên game ----
        var title = UIFactory.Place(root, "TitleBlock", 110, 120, 640, 260);
        titleGroup = title.gameObject.AddComponent<CanvasGroup>();
        UIFactory.MakeTitle(UIFactory.Place(title, "L1", 0, 0, 640, 76), "CÁNH ĐỒNG", 66, UIFactory.Paper).characterSpacing = 6;
        UIFactory.MakeTitle(UIFactory.Place(title, "L2", 0, 74, 640, 76), "NGỜ VỰC", 66, UIFactory.Gold).characterSpacing = 6;
        UIFactory.MakeBox(UIFactory.Place(title, "Rule", 4, 162, 64, 2), UIFactory.Gold, false);
        var en = UIFactory.MakeText(UIFactory.Place(title, "EN", 2, 176, 600, 28), "Fields of Suspicion", 20, new Color(0.93f, 0.89f, 0.82f, 0.8f));
        en.fontStyle = FontStyles.Italic;
        en.characterSpacing = 3;
        UIFactory.MakeText(UIFactory.Place(title, "Tag", 2, 210, 600, 24),
            "Một tuần ở Sellowe. Trồng rau ban ngày, chuyển tin ban đêm.", 16, new Color(0.93f, 0.89f, 0.82f, 0.6f));

        // ---- Menu ----
        menuCol = UIFactory.Place(root, "Menu", 110, 410, 420, 220);
        menuGroup = menuCol.gameObject.AddComponent<CanvasGroup>();
        UIFactory.VList(menuCol, 2, 0).childAlignment = TextAnchor.UpperLeft;
        Item(menuCol, "Ván mới", null, () => StartCoroutine(Intro()));
        continueItem = Item(menuCol, "Tiếp tục", "", Continue, out continueSub);
        Item(menuCol, "Cài đặt", null, () => StartCoroutine(Swap(menuGroup, settingsGroup)));
        Item(menuCol, "Thoát", null, DayManager.QuitGame);
        feedback = UIFactory.MakeText(UIFactory.Place(root, "Feedback", 110, 640, 600, 24), "", 14, UIFactory.Hex("E06A55"));

        // ---- Cài đặt (thay chỗ menu) ----
        settingsCol = UIFactory.Place(root, "Settings", 110, 410, 520, 240);
        settingsGroup = settingsCol.gameObject.AddComponent<CanvasGroup>();
        UIFactory.VList(settingsCol, 2, 0).childAlignment = TextAnchor.UpperLeft;
        settingsItems.Add((Item(settingsCol, "Toàn màn hình", "", () => { dm.ToggleFullscreen(); RefreshSettings(); }, out var s1), s1));
        settingsItems.Add((Item(settingsCol, "Âm lượng", "", () => { dm.CycleVolume(); RefreshSettings(); }, out var s2), s2));
        settingsItems.Add((Item(settingsCol, "Độ phân giải", "", () => { dm.CycleResolution(); RefreshSettings(); }, out var s3), s3));
        Item(settingsCol, "Quay lại", null, () => StartCoroutine(Swap(settingsGroup, menuGroup)));
        Hide(settingsGroup);

        var foot = UIFactory.MakeText(UIFactory.MakeRect(root, "Footer", new Vector2(1, 0), new Vector2(1, 0)), "v1.0  ·  Học phần IT1.236.3  ·  2026",
                                      13, new Color(0.93f, 0.89f, 0.82f, 0.45f), TextAlignmentOptions.BottomRight);
        foot.rectTransform.pivot = new Vector2(1, 0);
        foot.rectTransform.sizeDelta = new Vector2(500, 24);
        foot.rectTransform.anchoredPosition = new Vector2(-28, 20);

        BuildIntro(root, introBg);
        var black = UIFactory.MakeRect(root, "Black", Vector2.zero, Vector2.one);
        UIFactory.MakeBox(black, Color.black, false);
        blackGroup = black.gameObject.AddComponent<CanvasGroup>();
        blackGroup.blocksRaycasts = false;
    }

    // ---------------- Dựng phần tử ----------------

    Button Item(RectTransform parent, string label, string sub, Action onClick) => Item(parent, label, sub, onClick, out _);

    // Một mục menu chữ: nền trong suốt, rê chuột thì ▸ hiện ra bên trái và chữ nhích sang phải
    Button Item(RectTransform parent, string label, string sub, Action onClick, out TextMeshProUGUI subText)
    {
        var rt = UIFactory.MakeRect(parent, "Item_" + label, Vector2.zero, Vector2.one);
        UIFactory.Size(rt, 50);
        var hit = UIFactory.MakeBox(rt, new Color(1, 1, 1, 0));
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = hit;
        b.transition = Selectable.Transition.None;
        b.onClick.AddListener(() => GameAudio.Sfx("click"));
        b.onClick.AddListener(() => onClick());

        var content = UIFactory.MakeRect(rt, "Content", Vector2.zero, Vector2.one);
        var text = UIFactory.MakeTitle(UIFactory.Place(content, "Label", 30, 0, 360, 50), label, 26, UIFactory.Light, TextAlignmentOptions.MidlineLeft);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        subText = UIFactory.MakeText(UIFactory.Place(content, "Sub", 30 + text.preferredWidth + 16, 0, 260, 50), sub ?? "", 16,
                                     new Color(0.93f, 0.89f, 0.82f, 0.55f), TextAlignmentOptions.MidlineLeft);
        var markRt = UIFactory.Place(rt, "Marker", 2, 17, 16, 16);
        var mark = markRt.gameObject.AddComponent<Image>();
        mark.sprite = UIFactory.ArrowSprite;
        mark.color = new Color(UIFactory.Gold.r, UIFactory.Gold.g, UIFactory.Gold.b, 0);
        mark.raycastTarget = false;

        var feel = rt.gameObject.AddComponent<ButtonFeel>();
        feel.content = content;
        feel.marker = mark;
        feel.nudge = 8;
        return b;
    }

    static void SetItemEnabled(Button b, bool on)
    {
        b.interactable = on;
        foreach (var tx in b.GetComponentsInChildren<TMP_Text>()) tx.alpha = on ? 1f : 0.35f;
    }

    RectTransform Fog(RectTransform root, string name, Texture tex, float alpha, out RawImage img)
    {
        var rt = UIFactory.MakeRect(root, name, Vector2.zero, new Vector2(1, 0.62f));
        img = rt.gameObject.AddComponent<RawImage>();
        img.texture = tex;
        img.color = new Color(1, 1, 1, alpha);
        img.raycastTarget = false;
        return rt;
    }

    // Mây sương: nhiễu Perlin nhiều tầng, đậm ở dưới, nhạt dần lên trên
    static Texture2D FogTexture()
    {
        const int w = 256, h = 128;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat };
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float n = 0, amp = 0.5f, f = 1f / 64f;
            for (int o = 0; o < 4; o++)
            {
                // lặp liền theo chiều ngang: trộn hai mẫu ở x và x - w
                float a = Mathf.PerlinNoise(x * f + o * 17, y * f + o * 31);
                float b = Mathf.PerlinNoise((x - w) * f + o * 17, y * f + o * 31);
                n += amp * Mathf.Lerp(a, b, x / (float)w);
                amp *= 0.5f; f *= 2f;
            }
            float v = Mathf.Clamp01((n - 0.3f) * 1.8f) * Mathf.Pow(1f - y / (float)h, 1.4f);
            tex.SetPixel(x, y, new Color(1, 1, 1, v));
        }
        tex.Apply();
        return tex;
    }

    static Texture2D Gradient(Color left, Color right)
    {
        var tex = new Texture2D(64, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int x = 0; x < 64; x++) tex.SetPixel(x, 0, Color.Lerp(left, right, Mathf.SmoothStep(0, 1, x / 63f)));
        tex.Apply();
        return tex;
    }

    void BuildIntro(RectTransform root, Sprite introBg)
    {
        var intro = UIFactory.MakeRect(root, "Intro", Vector2.zero, Vector2.one);
        UIFactory.MakeBox(intro, UIFactory.Hex("0B0908"));
        introGroup = intro.gameObject.AddComponent<CanvasGroup>();
        if (introBg)
        {
            var bg = UIFactory.MakeRect(intro, "BG", Vector2.zero, Vector2.one);
            var img = bg.gameObject.AddComponent<Image>();
            img.sprite = introBg;
            img.color = new Color(1, 1, 1, 0.35f);
            img.raycastTarget = false;
            var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = introBg.rect.width / introBg.rect.height;
        }
        // Bấm bất kỳ đâu để sang khung tiếp
        var hit = UIFactory.MakeRect(intro, "Hit", Vector2.zero, Vector2.one);
        var hitImg = UIFactory.MakeBox(hit, new Color(0, 0, 0, 0));
        var hb = introHit = hit.gameObject.AddComponent<Button>();
        hb.targetGraphic = hitImg;
        hb.transition = Selectable.Transition.None;
        hb.onClick.AddListener(() => advance = true);

        introText = UIFactory.MakeText(UIFactory.MakeRectAt(intro, "Line", new Vector2(0, 20), new Vector2(900, 240)), "", 26,
                                       UIFactory.Light, TextAlignmentOptions.Center);
        introText.lineSpacing = 18;
        introHint = UIFactory.MakeText(UIFactory.MakeRectAt(intro, "Hint", new Vector2(0, -290), new Vector2(600, 30)),
                                       "Bấm để tiếp tục", 14, new Color(0.93f, 0.89f, 0.82f, 0.5f), TextAlignmentOptions.Center);
        var skip = skipButton = UIFactory.MakeButton(UIFactory.MakeRectAt(intro, "Skip", new Vector2(560, -312), new Vector2(150, 40)), "Skip", "Bỏ qua  ›", SkipIntro, 15);
        UIFactory.SetColor(skip, new Color(1, 1, 1, 0.15f));
        skip.GetComponentInChildren<TextMeshProUGUI>().color = UIFactory.Light;

        // Ký tên
        var name = UIFactory.MakeRectAt(intro, "Name", new Vector2(0, 10), new Vector2(620, 260));
        nameGroup = name.gameObject.AddComponent<CanvasGroup>();
        nameLine = UIFactory.MakeText(UIFactory.Place(name, "Ask", 0, 0, 620, 70),
            "Trước khi trở thành Tomas Rell, cậu tên là gì?\n<size=70%><color=#B8AC98>Văn phòng ghi tên thật vào hồ sơ mật. Để trống thì hồ sơ ghi là Wren.</color></size>",
            24, UIFactory.Light, TextAlignmentOptions.Center);
        var inputRect = UIFactory.Place(name, "Input", 110, 100, 400, 52);
        var inputImage = UIFactory.MakeSurface(inputRect, UIFactory.Paper, 6);
        nameInput = inputRect.gameObject.AddComponent<TMP_InputField>();
        nameInput.targetGraphic = inputImage;
        nameInput.textViewport = inputRect;
        nameInput.lineType = TMP_InputField.LineType.SingleLine;
        nameInput.characterLimit = 24;
        var textRect = UIFactory.MakeRect(inputRect, "Text", Vector2.zero, Vector2.one);
        textRect.offsetMin = new Vector2(16, 4); textRect.offsetMax = new Vector2(-16, -4);
        nameInput.textComponent = UIFactory.MakeText(textRect, "", 20, UIFactory.Ink, TextAlignmentOptions.Midline);
        var phRect = UIFactory.MakeRect(inputRect, "Placeholder", Vector2.zero, Vector2.one);
        phRect.offsetMin = new Vector2(16, 4); phRect.offsetMax = new Vector2(-16, -4);
        var ph = UIFactory.MakeText(phRect, "Wren", 20, UIFactory.Muted, TextAlignmentOptions.Midline);
        ph.fontStyle = FontStyles.Italic;
        nameInput.placeholder = ph;
        nameInput.onSubmit.AddListener(_ => advance = true);
        var sign = UIFactory.MakeButton(UIFactory.Place(name, "Sign", 210, 176, 200, 48), "Sign", "Ký tên", () => advance = true, 18);
        UIFactory.SetColor(sign, UIFactory.Red);
        sign.GetComponentInChildren<TextMeshProUGUI>().color = UIFactory.Light;
        UIFactory.Skin((Image)sign.targetGraphic, "btn_primary");
        Hide(nameGroup);
        Hide(introGroup);
    }

    // ---------------- Luồng ----------------

    public void Show()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        GameAudio.Music("night", 2f);
        GameState save = null;
        bool has = SaveSystem.HasSave && SaveSystem.TryLoad(out save, out _);
        SetItemEnabled(continueItem, has);
        continueSub.text = has ? $"N{save.day} · {save.Weekday}" : "chưa có ván lưu";
        RefreshSettings();
        Hide(settingsGroup); Hide(introGroup); Hide(nameGroup);
        StartCoroutine(Entrance());
    }

    IEnumerator Entrance()
    {
        Hide(titleGroup); Hide(menuGroup);
        blackGroup.alpha = 1;
        yield return Fade(blackGroup, 0, 1.2f);
        yield return Fade(titleGroup, 1, 0.8f);
        yield return Fade(menuGroup, 1, 0.5f);
    }

    void Continue()
    {
        string error = dm.ContinueGame();
        if (error != null) feedback.text = "Không thể tải ván lưu: " + error;
        else gameObject.SetActive(false);
    }

    void RefreshSettings()
    {
        if (settingsItems.Count < 3) return;
        settingsItems[0].sub.text = Screen.fullScreen ? "Bật" : "Tắt";
        settingsItems[1].sub.text = $"{Mathf.RoundToInt(AudioListener.volume * 100)}%   <size=80%>(bấm để đổi)</size>";
        settingsItems[2].sub.text = $"{Screen.width} × {Screen.height}";
    }

    bool skipping;
    void SkipIntro() { skipping = true; advance = true; }

    IEnumerator Intro()
    {
        menuGroup.interactable = false;
        yield return Fade(blackGroup, 1, 0.6f);
        Show(introGroup, true);
        Hide(nameGroup);
        introHit.interactable = true;
        skipButton.gameObject.SetActive(true);
        nameInput.text = "";
        introText.gameObject.SetActive(true);
        introHint.gameObject.SetActive(true);
        yield return Fade(blackGroup, 0, 0.6f);

        skipping = false;
        foreach (var line in IntroLines)
        {
            if (skipping) break;
            introText.text = line;
            introText.alpha = 0;
            for (float k = 0; k < 1; k += Time.unscaledDeltaTime / 0.9f) { introText.alpha = k; yield return null; }
            introText.alpha = 1;
            advance = false;
            while (!advance) yield return null;   // chỉ sang khung tiếp khi người chơi bấm
            for (float k = 1; k > 0; k -= Time.unscaledDeltaTime / 0.5f) { introText.alpha = k; yield return null; }
            introText.alpha = 0;
        }

        // Ký tên
        introText.gameObject.SetActive(false);
        introHint.gameObject.SetActive(false);
        introHit.interactable = false;   // đang ký tên: bấm ra ngoài không được tính là "tiếp"
        skipButton.gameObject.SetActive(false);
        yield return Fade(nameGroup, 1, 0.6f);
        nameGroup.blocksRaycasts = nameGroup.interactable = true;
        nameInput.ActivateInputField();
        advance = false;
        while (!advance) yield return null;
        GameAudio.Sfx("pen");
        string playerName = string.IsNullOrWhiteSpace(nameInput.text) ? "Wren" : nameInput.text.Trim();
        yield return Fade(nameGroup, 0, 0.5f);
        Hide(nameGroup);

        introText.gameObject.SetActive(true);
        introText.text = $"<size=80%><color=#B8AC98>Hồ sơ mật: {playerName}. Mật danh: Wren. Vỏ bọc: Tomas Rell.</color></size>\n\nSáng Chủ Nhật. Có một lá thư nằm dưới khe cửa.";
        for (float k = 0; k < 1; k += Time.unscaledDeltaTime / 0.9f) { introText.alpha = k; yield return null; }
        introText.alpha = 1;
        introHint.gameObject.SetActive(true);
        introHit.interactable = true;
        advance = false;
        while (!advance) yield return null;
        introHint.gameObject.SetActive(false);

        yield return Fade(blackGroup, 1, 0.8f);
        dm.StartNewGame(playerName);
        gameObject.SetActive(false);
    }

    IEnumerator Swap(CanvasGroup from, CanvasGroup to)
    {
        RefreshSettings();
        yield return Fade(from, 0, 0.2f);
        Hide(from);
        yield return Fade(to, 1, 0.25f);
    }

    static void Hide(CanvasGroup g) { g.alpha = 0; g.interactable = g.blocksRaycasts = false; }
    static void Show(CanvasGroup g, bool instant) { g.alpha = instant ? 1 : g.alpha; g.interactable = g.blocksRaycasts = true; }

    static IEnumerator Fade(CanvasGroup g, float to, float time)
    {
        float from = g.alpha;
        for (float k = 0; k < 1; k += Time.unscaledDeltaTime / time)
        {
            g.alpha = Mathf.Lerp(from, to, k * k * (3 - 2 * k));
            yield return null;
        }
        g.alpha = to;
        bool on = to > 0.5f;
        if (g.name != "Black") g.interactable = g.blocksRaycasts = on;
    }

    void Update()
    {
        t += Time.unscaledDeltaTime;
        // Ảnh nền zoom vào/ra rất chậm, sương trôi hai lớp khác tốc độ
        if (bgRect) bgRect.localScale = Vector3.one * (1.03f + 0.03f * Mathf.Sin(t * 0.05f));
        if (fogImgA) fogImgA.uvRect = new Rect(t * 0.006f, 0, 1.6f, 1);
        if (fogImgB) fogImgB.uvRect = new Rect(-t * 0.004f + 0.37f, 0.1f, 1.2f, 1);
        if (introHint && introHint.gameObject.activeInHierarchy) introHint.alpha = 0.35f + 0.25f * Mathf.Sin(t * 2.5f);
    }
}
