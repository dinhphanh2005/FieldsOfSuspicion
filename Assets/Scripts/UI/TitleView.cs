using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleView : MonoBehaviour
{
    DayManager dm;
    CanvasGroup menuGroup, settingsGroup, introGroup, signGroup, stampGroup, blackGroup;
    TextMeshProUGUI feedback, introHint;
    Button continueNote, introHit, skipButton;
    TextMeshProUGUI continueSub;
    readonly List<TextMeshProUGUI> settingSubs = new List<TextMeshProUGUI>();
    readonly List<TextMeshProUGUI> orderLines = new List<TextMeshProUGUI>();
    TMP_InputField nameInput;
    Image inputBack, underline;
    TextMeshProUGUI signHint;
    Button signBtn;
    const string SignHintText = "Gõ tên của bạn rồi bấm Ký tên (hoặc Enter).";
    const string RecipientBlank = "Người nhận: ……………………………";
    RectTransform stamp;
    bool advance, skipping;
    float t;

    static readonly string[] OrderLines =
    {
        RecipientBlank,
        "Nhiệm vụ: sống bảy ngày ở làng Sellowe, bên kia biên giới, trong vai Tomas Rell.",
        "Vỏ bọc: người trồng rau, thuê căn nhà và mảnh ruộng của bà Mabry.",
        "Chỉ thị chi tiết sẽ được gửi tới nhà vào sáng Chủ Nhật. Đọc xong thì đốt.",
    };

    static TextMeshProUGUI Bold(RectTransform rt, string text, float size, Color color, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var tx = UIFactory.MakeText(rt, text, size, color, align);
        tx.fontStyle = FontStyles.Bold;
        return tx;
    }

    public void Init(DayManager manager)
    {
        dm = manager;
        var root = (RectTransform)transform;

        UIFactory.MakeBox(root, UIFactory.Hex("2B2019"));
        UIFactory.AddGrain(root, 0.18f);
        var frame = UIFactory.Place(root, "Board_Frame", 56, 40, 1168, 640);
        UIFactory.MakeSurface(frame, UIFactory.Hex("5B3D28"), 10);
        UIFactory.AddShadow(frame, 14);
        var cork = UIFactory.MakeRect(frame, "Cork", Vector2.zero, Vector2.one, 16);
        UIFactory.MakeSurface(cork, UIFactory.Hex("A9805A"), 6);
        UIFactory.Skin(cork.GetComponent<Image>(), "board_cork");
        UIFactory.AddGrain(cork, 0.3f);

        var poster = Note(cork, "Poster", 60, 160, 600, 290, "notice_cream", 0f, "pin_red");
        Bold(UIFactory.Place(poster, "Kicker", 46, 46, 508, 22), "SELLOWE  ·  THÔNG BÁO", 14, UIFactory.Hex("8C3A2B")).characterSpacing = 4;
        var title = Bold(UIFactory.Place(poster, "Title", 44, 78, 520, 150), "CÁNH ĐỒNG\nNGỜ VỰC", 60, UIFactory.Ink);
        title.lineSpacing = -18;
        title.characterSpacing = 2;
        UIFactory.MakeBox(UIFactory.Place(poster, "Rule", 46, 236, 508, 2), UIFactory.Hex("3A2E25"), false);

        menuGroup = Column(cork, "Menu");
        var col = (RectTransform)menuGroup.transform;
        MenuNote(col, 0, "Ván mới", "Tuần đầu tiên ở Sellowe", "notice_cream", 1.4f, "pin_brass", () => StartCoroutine(Intro()), out _);
        continueNote = MenuNote(col, 1, "Tiếp tục", "", "notice_green", -1.0f, "pin_red", Continue, out continueSub);
        MenuNote(col, 2, "Cài đặt", "Âm lượng, màn hình", "notice_blue", 0.8f, "pin_blue", () => StartCoroutine(Swap(menuGroup, settingsGroup)), out _);
        MenuNote(col, 3, "Thoát", "Rời khỏi làng", "notice_pink", -1.6f, "pin_brass", DayManager.QuitGame, out _);

        settingsGroup = Column(cork, "Settings");
        var scol = (RectTransform)settingsGroup.transform;
        MenuNote(scol, 0, "Toàn màn hình", "", "notice_blue", -0.8f, "pin_red", () => { dm.ToggleFullscreen(); RefreshSettings(); }, out var s0);
        MenuNote(scol, 1, "Âm lượng", "", "notice_cream", 1.2f, "pin_brass", () => { dm.CycleVolume(); RefreshSettings(); }, out var s1);
        MenuNote(scol, 2, "Độ phân giải", "", "notice_green", -1.2f, "pin_blue", () => { dm.CycleResolution(); RefreshSettings(); }, out var s2);
        MenuNote(scol, 3, "Quay lại", "", "notice_pink", 1.0f, "pin_red", () => StartCoroutine(Swap(settingsGroup, menuGroup)), out _);
        settingSubs.AddRange(new[] { s0, s1, s2 });
        Hide(settingsGroup);

        feedback = UIFactory.MakeText(UIFactory.Place(cork, "Feedback", 700, 566, 420, 24), "", 14, UIFactory.Hex("8C3A2B"));

        BuildIntro(root);
        var black = UIFactory.MakeRect(root, "Black", Vector2.zero, Vector2.one);
        UIFactory.MakeBox(black, UIFactory.Hex("140F0C"), false);
        blackGroup = black.gameObject.AddComponent<CanvasGroup>();
        blackGroup.blocksRaycasts = false;
    }

    static RectTransform Note(RectTransform parent, string name, float x, float y, float w, float h, string paper, float tilt, string pin)
    {
        var rt = UIFactory.Place(parent, name, x, y, w, h);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x + w / 2, -(y + h / 2));
        rt.localRotation = Quaternion.Euler(0, 0, tilt);
        UIFactory.MakeSurface(rt, UIFactory.Paper, 4);
        UIFactory.Skin(rt.GetComponent<Image>(), paper);
        UIFactory.AddShadow(rt, 6);
        var p = UIFactory.MakeRectAt(rt, "Pin", Vector2.zero, new Vector2(18, 18));
        p.anchorMin = p.anchorMax = new Vector2(0.5f, 1);
        p.anchoredPosition = new Vector2(0, -14);
        UIFactory.Skin(UIFactory.MakeBox(p, Color.white, false), pin);
        return rt;
    }

    static CanvasGroup Column(RectTransform cork, string name)
    {
        var col = UIFactory.Place(cork, name, 700, 40, 420, 520);
        return col.gameObject.AddComponent<CanvasGroup>();
    }

    Button MenuNote(RectTransform col, int i, string label, string sub, string paper, float tilt, string pin, Action onClick, out TextMeshProUGUI subText)
    {
        var rt = Note(col, "Note_" + label, 10 + (i % 2 == 0 ? 0 : 26), i * 124, 360, 100, paper, tilt, pin);
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = rt.GetComponent<Image>();
        UIFactory.ApplyColors(b);
        b.onClick.AddListener(() => GameAudio.Sfx("page", 0.7f));
        b.onClick.AddListener(() => onClick());
        Bold(UIFactory.Place(rt, "Label", 30, 26, 300, 34), label, 26, UIFactory.Ink);
        subText = UIFactory.MakeText(UIFactory.Place(rt, "Sub", 30, 60, 300, 22), sub ?? "", 15, UIFactory.Muted);
        rt.gameObject.AddComponent<NoteFeel>();
        return b;
    }

    void BuildIntro(RectTransform root)
    {
        var intro = UIFactory.MakeRect(root, "Intro", Vector2.zero, Vector2.one);
        UIFactory.MakeBox(intro, UIFactory.Hex("1E1712"));
        UIFactory.AddGrain(intro, 0.15f);
        introGroup = intro.gameObject.AddComponent<CanvasGroup>();

        var hit = UIFactory.MakeRect(intro, "Hit", Vector2.zero, Vector2.one);
        var hitImg = UIFactory.MakeBox(hit, new Color(0, 0, 0, 0));
        introHit = hit.gameObject.AddComponent<Button>();
        introHit.targetGraphic = hitImg;
        introHit.transition = Selectable.Transition.None;
        introHit.onClick.AddListener(() => advance = true);

        var paper = UIFactory.MakeRectAt(intro, "Order", new Vector2(0, 16), new Vector2(680, 560));
        UIFactory.MakeSurface(paper, UIFactory.Hex("F2E9D3"), 4);
        UIFactory.AddGrain(paper, 0.12f);
        UIFactory.AddShadow(paper, 12);
        paper.GetComponent<Image>().raycastTarget = false;

        UIFactory.MakeText(UIFactory.Place(paper, "Office", 56, 48, 400, 20), "VĂN PHÒNG STRAKA  ·  TỐI MẬT", 13, UIFactory.Muted).characterSpacing = 3;
        UIFactory.MakeText(UIFactory.Place(paper, "No", 420, 48, 204, 20), "Số 47", 13, UIFactory.Muted, TextAlignmentOptions.TopRight);
        Bold(UIFactory.Place(paper, "Head", 56, 74, 568, 40), "LỆNH ĐIỀU ĐỘNG", 30, UIFactory.Ink);
        UIFactory.MakeBox(UIFactory.Place(paper, "Rule", 56, 122, 568, 2), UIFactory.Ink, false);

        var lines = UIFactory.Place(paper, "Lines", 56, 140, 568, 260);
        var v = UIFactory.VList(lines, 14, 0);
        v.childAlignment = TextAnchor.UpperLeft;
        foreach (var line in OrderLines)
        {
            var tx = UIFactory.AddLine(lines, line, 18, UIFactory.Ink);
            tx.lineSpacing = 8;
            orderLines.Add(tx);
        }

        var sign = UIFactory.Place(paper, "Sign", 56, 418, 568, 56);
        signGroup = sign.gameObject.AddComponent<CanvasGroup>();
        UIFactory.MakeText(UIFactory.Place(sign, "Label", 0, 0, 110, 56), "Ký nhận:", 18, UIFactory.Ink, TextAlignmentOptions.MidlineLeft);
        var inputRect = UIFactory.Place(sign, "Input", 110, 6, 300, 44);
        inputBack = UIFactory.MakeBox(inputRect, new Color(0.12f, 0.23f, 0.37f, 0.05f));
        underline = UIFactory.MakeBox(UIFactory.Place(inputRect, "Underline", 0, 41, 300, 3), UIFactory.Ink, false);
        nameInput = inputRect.gameObject.AddComponent<TMP_InputField>();
        nameInput.targetGraphic = inputBack;
        nameInput.transition = Selectable.Transition.None;
        nameInput.customCaretColor = true;
        nameInput.caretColor = UIFactory.Hex("1F3A5F");
        nameInput.caretWidth = 3;
        nameInput.caretBlinkRate = 1.2f;
        nameInput.selectionColor = new Color(0.12f, 0.23f, 0.37f, 0.25f);
        nameInput.textViewport = inputRect;
        nameInput.lineType = TMP_InputField.LineType.SingleLine;
        nameInput.characterLimit = 24;
        var textRect = UIFactory.MakeRect(inputRect, "Text", Vector2.zero, Vector2.one);
        textRect.offsetMin = new Vector2(8, 4); textRect.offsetMax = new Vector2(-8, -4);
        nameInput.textComponent = UIFactory.MakeText(textRect, "", 22, UIFactory.Hex("1F3A5F"), TextAlignmentOptions.MidlineLeft);
        nameInput.textComponent.fontStyle = FontStyles.Italic;
        var phRect = UIFactory.MakeRect(inputRect, "Placeholder", Vector2.zero, Vector2.one);
        phRect.offsetMin = new Vector2(8, 4); phRect.offsetMax = new Vector2(-8, -4);
        var ph = UIFactory.MakeText(phRect, "Bấm vào đây rồi gõ tên của bạn", 17, UIFactory.Muted, TextAlignmentOptions.MidlineLeft);
        ph.fontStyle = FontStyles.Italic;
        nameInput.placeholder = ph;
        nameInput.onSubmit.AddListener(_ => advance = true);
        signBtn = UIFactory.MakeButton(UIFactory.Place(sign, "SignBtn", 430, 6, 138, 44), "SignBtn", "Ký tên", () => advance = true, 17);
        UIFactory.SetColor(signBtn, UIFactory.Red);
        UIFactory.Skin((Image)signBtn.targetGraphic, "btn_primary");
        signBtn.GetComponentInChildren<TextMeshProUGUI>().color = UIFactory.Light;
        signHint = UIFactory.MakeText(UIFactory.Place(paper, "SignHint", 56, 482, 568, 22), SignHintText, 14, UIFactory.Hex("6B5B4C"));
        signHint.transform.SetParent(sign, true);

        stamp = UIFactory.MakeRectAt(paper, "Stamp", new Vector2(212, -164), new Vector2(170, 60));
        stamp.localRotation = Quaternion.Euler(0, 0, 9);
        var ink = new Color(0.66f, 0.17f, 0.12f, 0.85f);
        UIFactory.MakeBox(UIFactory.Place(stamp, "T", 0, 0, 170, 3), ink, false);
        UIFactory.MakeBox(UIFactory.Place(stamp, "B", 0, 57, 170, 3), ink, false);
        UIFactory.MakeBox(UIFactory.Place(stamp, "L", 0, 3, 3, 54), ink, false);
        UIFactory.MakeBox(UIFactory.Place(stamp, "R", 167, 3, 3, 54), ink, false);
        Bold(UIFactory.MakeRect(stamp, "Text", Vector2.zero, Vector2.one), "ĐÃ NHẬN", 25, ink, TextAlignmentOptions.Center).characterSpacing = 6;
        stampGroup = stamp.gameObject.AddComponent<CanvasGroup>();
        stampGroup.blocksRaycasts = false;

        introHint = UIFactory.MakeText(UIFactory.MakeRectAt(intro, "Hint", new Vector2(0, -320), new Vector2(600, 24)),
                                       "Bấm để đọc tiếp", 14, new Color(0.93f, 0.89f, 0.82f, 0.6f), TextAlignmentOptions.Center);
        skipButton = UIFactory.MakeButton(UIFactory.MakeRectAt(intro, "Skip", new Vector2(560, -320), new Vector2(120, 36)), "Skip", "Bỏ qua", SkipIntro, 14);
        UIFactory.SetColor(skipButton, new Color(1, 1, 1, 0.12f));
        skipButton.GetComponentInChildren<TextMeshProUGUI>().color = UIFactory.Light;
        Hide(introGroup);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        GameAudio.Music("day", 2f);
        GameState save = null;
        bool has = SaveSystem.HasSave && SaveSystem.TryLoad(out save, out _);
        continueNote.interactable = has;
        foreach (var tx in continueNote.GetComponentsInChildren<TMP_Text>()) tx.alpha = has ? 1f : 0.45f;
        continueSub.text = has ? $"Đang ở N{save.day} · {save.Weekday}" : "Chưa có ván lưu";
        feedback.text = "";
        RefreshSettings();
        Hide(settingsGroup); Hide(introGroup);
        Show(menuGroup);
        blackGroup.alpha = 1;
        StartCoroutine(Fade(blackGroup, 0, 0.9f));
    }

    void Continue()
    {
        string error = dm.ContinueGame();
        if (error != null) feedback.text = "Không mở được ván lưu: " + error;
        else gameObject.SetActive(false);
    }

    void RefreshSettings()
    {
        if (settingSubs.Count < 3) return;
        settingSubs[0].text = Screen.fullScreen ? "Đang bật" : "Đang tắt";
        settingSubs[1].text = $"{Mathf.RoundToInt(AudioListener.volume * 100)}%  · bấm để đổi";
        settingSubs[2].text = $"{Screen.width} × {Screen.height}";
    }

    void SkipIntro() { skipping = true; advance = true; }

    IEnumerator Intro()
    {
        menuGroup.interactable = false;
        yield return Fade(blackGroup, 1, 0.5f);
        GameAudio.Music("night", 1.5f);
        Show(introGroup);
        foreach (var l in orderLines) l.alpha = 0;
        Hide(signGroup);
        stampGroup.alpha = 0;
        nameInput.text = "";
        orderLines[0].text = RecipientBlank;
        signBtn.gameObject.SetActive(true);
        signHint.text = SignHintText;
        signHint.color = UIFactory.Hex("6B5B4C");
        introHit.interactable = true;
        skipButton.gameObject.SetActive(true);
        introHint.gameObject.SetActive(true);
        skipping = false;
        yield return Fade(blackGroup, 0, 0.5f);

        for (int i = 0; i < orderLines.Count; i++)
        {
            if (i > 0 && !skipping)
            {
                advance = false;
                while (!advance) yield return null;
            }
            if (i > 0) GameAudio.Sfx("page", 0.4f);
            yield return FadeText(orderLines[i], skipping ? 0.05f : 0.45f);
        }

        introHit.interactable = false;
        skipButton.gameObject.SetActive(false);
        introHint.gameObject.SetActive(false);
        yield return Fade(signGroup, 1, 0.4f);
        nameInput.ActivateInputField();
        while (true)
        {
            advance = false;
            while (!advance) yield return null;
            if (!string.IsNullOrWhiteSpace(nameInput.text)) break;
            signHint.text = "Cần ký tên trước đã — bấm vào dòng kẻ và gõ tên của bạn.";
            signHint.color = UIFactory.Red;
            nameInput.ActivateInputField();
        }
        string playerName = nameInput.text.Trim();
        nameInput.text = playerName;
        nameInput.interactable = false;
        signGroup.interactable = false;
        GameAudio.Sfx("pen");
        signHint.text = "";
        signBtn.gameObject.SetActive(false);
        orderLines[0].text = $"Người nhận: <color=#1F3A5F><i><noparse>{playerName}</noparse></i></color>";
        yield return FadeText(orderLines[0], 0.3f);

        yield return new WaitForSecondsRealtime(0.35f);
        GameAudio.Sfx("drop", 0.8f);
        for (float k = 0; k < 1; k += Time.unscaledDeltaTime / 0.18f)
        {
            stampGroup.alpha = k;
            stamp.localScale = Vector3.one * Mathf.Lerp(1.5f, 1f, k * k);
            yield return null;
        }
        stampGroup.alpha = 1; stamp.localScale = Vector3.one;

        introHint.text = "Bấm để bắt đầu";
        introHint.gameObject.SetActive(true);
        introHit.interactable = true;
        advance = false;
        while (!advance) yield return null;

        yield return Fade(blackGroup, 1, 0.7f);
        nameInput.interactable = true;
        introHint.text = "Bấm để đọc tiếp";
        dm.StartNewGame(playerName);
        gameObject.SetActive(false);
    }

    IEnumerator Swap(CanvasGroup from, CanvasGroup to)
    {
        RefreshSettings();
        from.interactable = false;
        yield return Fade(from, 0, 0.18f);
        Hide(from);
        Show(to, 0);
        yield return Fade(to, 1, 0.22f);
    }

    static void Hide(CanvasGroup g) { g.alpha = 0; g.interactable = g.blocksRaycasts = false; }
    static void Show(CanvasGroup g, float alpha = 1) { g.alpha = alpha; g.interactable = g.blocksRaycasts = true; }

    static IEnumerator FadeText(TMP_Text tx, float time)
    {
        for (float k = 0; k < 1; k += Time.unscaledDeltaTime / time) { tx.alpha = k; yield return null; }
        tx.alpha = 1;
    }

    static IEnumerator Fade(CanvasGroup g, float to, float time)
    {
        float from = g.alpha;
        for (float k = 0; k < 1; k += Time.unscaledDeltaTime / time)
        {
            g.alpha = Mathf.Lerp(from, to, k * k * (3 - 2 * k));
            yield return null;
        }
        g.alpha = to;
        if (g.gameObject.name != "Black") g.interactable = g.blocksRaycasts = to > 0.5f;
    }

#if UNITY_EDITOR
    public void PreviewState(int mode)
    {
        Hide(menuGroup); Hide(settingsGroup); Hide(introGroup);
        blackGroup.alpha = 0;
        continueSub.text = "Đang ở N3 · Thứ Ba";
        RefreshSettings();
        if (mode == 0) { Show(menuGroup); return; }
        if (mode == 1) { Show(settingsGroup); return; }
        Show(introGroup);
        for (int i = 0; i < orderLines.Count; i++) orderLines[i].alpha = mode == 2 && i > 0 ? 0 : 1;
        signGroup.alpha = mode >= 3 ? 1 : 0;
        stampGroup.alpha = mode == 4 ? 1 : 0;
        signBtn.gameObject.SetActive(mode != 4);
        nameInput.text = mode == 4 ? "Minh" : "";
        orderLines[0].text = mode == 4 ? "Người nhận: <color=#1F3A5F><i>Minh</i></color>" : RecipientBlank;
        skipButton.gameObject.SetActive(mode < 3);
        introHint.gameObject.SetActive(mode != 3);
        introHint.text = mode == 4 ? "Bấm để bắt đầu" : "Bấm để đọc tiếp";
    }
#endif

    void Update()
    {
        t += Time.unscaledDeltaTime;
        if (introHint && introHint.gameObject.activeInHierarchy) introHint.alpha = 0.45f + 0.2f * Mathf.Sin(t * 2.4f);
        if (nameInput && nameInput.interactable && signGroup.alpha > 0.5f)
        {
            bool focused = nameInput.isFocused;
            bool empty = string.IsNullOrWhiteSpace(nameInput.text);
            underline.color = focused ? UIFactory.Red : Color.Lerp(UIFactory.Ink, UIFactory.Red, empty ? 0.5f + 0.5f * Mathf.Sin(t * 3f) : 0f);
            inputBack.color = new Color(0.12f, 0.23f, 0.37f, focused ? 0.10f : 0.05f);
            UIFactory.SetInteractable(signBtn, !empty);
        }
    }
}
