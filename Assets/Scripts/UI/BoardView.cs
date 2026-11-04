using System.Text.RegularExpressions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Buổi sáng: bảng tin làng (bảng bần khung gỗ, thẻ ghim) + ruộng.
// - Mỗi thông báo là một thẻ; lệnh mật (Null Cipher) nằm lẫn với thông báo thường.
// - "Cách đọc lệnh": hiện mẩu giấy hướng dẫn và tô đỏ chữ cái đầu mỗi câu.
// - "Thu hoạch": 1 lần/ngày, tốn năng lượng, thêm rau vào kho.
public class BoardView : MonoBehaviour
{
    // Chữ cái đầu câu: đầu đoạn hoặc sau . ? ! và khoảng trắng
    static readonly Regex SentenceStart = new Regex(@"(^|[.?!]\s+)(\S)");

    DayManager dm;
    RectTransform cards, hintNote;
    Button farmBtn, hintBtn;
    TextMeshProUGUI barHint;
    RectTransform farmPanel;
    TextMeshProUGUI energyStat, apStat, stockStat;
    readonly List<Button> plotButtons = new List<Button>();
    readonly List<Image> plotArt = new List<Image>();
    int hintLevel;           // 0 = tắt · 1 = tô chữ đầu câu · 2 = giải sẵn lệnh
    bool hintIntroduced;     // ngày đầu tự bật mức 1, sau đó giữ lựa chọn của người chơi
    bool hintOn => hintLevel > 0;
    DaySO day;

    GameState S => dm.State;

    public void Init(DayManager manager)
    {
        dm = manager;
        var stage = UIFactory.MakeStage(transform);

        // Khung gỗ + mặt bần
        var frame = UIFactory.Place(stage, "Board_Frame", 24, 16, 1232, 576);
        UIFactory.MakeSurface(frame, UIFactory.Hex("5B3D28"), 10);
        UIFactory.AddShadow(frame, 10);
        var cork = UIFactory.MakeRect(frame, "Board_Cork", Vector2.zero, Vector2.one, 14);
        UIFactory.MakeSurface(cork, UIFactory.Hex("A9805A"), 6);
        UIFactory.Skin(cork.GetComponent<Image>(), "board_cork");
        UIFactory.AddGrain(cork, 0.35f);

        // Hàng thẻ: canh trên, mỗi thẻ cao theo chữ
        cards = UIFactory.MakeRect(cork, "Board_Cards", Vector2.zero, Vector2.one);
        cards.offsetMin = new Vector2(0, 102); // chừa riêng dải dưới cho hướng dẫn, không phủ lên thẻ
        var h = UIFactory.HList(cards, 20, 0);
        h.padding = new RectOffset(24, 24, 30, 24);
        h.childAlignment = TextAnchor.UpperLeft;
        h.childForceExpandHeight = false;

        // Mẩu giấy hướng dẫn đọc lệnh, ghim ở đáy bảng
        hintNote = UIFactory.MakeRect(cork, "Board_HintNote", new Vector2(0, 0), new Vector2(1, 0));
        hintNote.pivot = new Vector2(0.5f, 0);
        hintNote.sizeDelta = new Vector2(-48, 72);
        hintNote.anchoredPosition = new Vector2(0, 20);
        UIFactory.MakeSurface(hintNote, UIFactory.Hex("F6E7B8"), 5);
        UIFactory.AddShadow(hintNote, 5);
        UIFactory.MakeText(UIFactory.MakeRect(hintNote, "Text", Vector2.zero, Vector2.one, 12),
            "<b>Cách đọc lệnh mật:</b> lấy <b>chữ cái đầu của mỗi câu</b> (tô đỏ), bỏ dấu, ghép lại. " +
            "Ví dụ: \"<b>G</b>ia đình… <b>Í</b>t khi… <b>A</b>i cần… <b>Ở</b> ngay… <b>X</b>em hàng… <b>E</b>m trai…\" = GIAO XE.\n" +
            "Lệnh của Rook ký \"bà Rosa, xóm Bắc\"  ·  lệnh của Shrike ký \"ông Sev, lò gạch\". Thông báo khác chỉ là tin làng.",
            15, UIFactory.Ink, TextAlignmentOptions.MidlineLeft);
        hintNote.gameObject.SetActive(false);

        BuildFarmPanel(stage);
        var bar = UIFactory.MakeActionBar(transform);
        farmBtn = UIFactory.BarButton(bar, "Ruộng", ToggleFarm);
        hintBtn = UIFactory.BarButton(bar, "Gợi ý đọc lệnh", ToggleHint);
        UIFactory.BarButton(bar, "Sổ tay", () => dm.OpenNotebook(true));
        barHint = UIFactory.BarHint(bar, "");
        UIFactory.BarSpacer(bar);
        UIFactory.BarButton(bar, "Mở tiệm buổi chiều", () => dm.EndPhase(), primary: true);
    }

    public void Show(DaySO today, GameState s)
    {
        day = today;
        if (farmPanel) farmPanel.gameObject.SetActive(false);
        // Ngày đầu: bật sẵn gợi ý đọc lệnh mật để người chơi mới không bị lạc
        if (!hintIntroduced) { hintIntroduced = true; SetHint(1); return; }
        BuildCards();
        RefreshFarm();
    }

    void BuildCards()
    {
        UIFactory.Clear(cards);
        if (!day) return;

        int n = 0;
        foreach (var m in day.board)
        {
            if (!m || !Conditions.Check(S, m.condition, m.param)) continue;
            S.MarkMessageShown(m.name);

            var card = UIFactory.MakeRect(cards, "Card_" + m.name, Vector2.zero, Vector2.one);
            UIFactory.MakeSurface(card, n % 2 == 0 ? UIFactory.Paper : UIFactory.Hex("EFE4CC"), 5);
            string noticeSprite = n % 4 == 0 ? "notice_cream" : n % 4 == 1 ? "notice_blue" : n % 4 == 2 ? "notice_green" : "notice_pink";
            UIFactory.Skin(card.GetComponent<Image>(), noticeSprite);
            UIFactory.AddShadow(card, 6);
            UIFactory.EqualWidth(card);
            n++;

            // Đinh ghim
            var pin = UIFactory.MakeRectAt(card, "Pin", Vector2.zero, new Vector2(14, 14));
            pin.anchorMin = pin.anchorMax = new Vector2(0.5f, 1);
            pin.anchoredPosition = new Vector2(0, -12);
            var pinImage = UIFactory.MakeBox(pin, UIFactory.Red, false);
            UIFactory.Skin(pinImage, n % 3 == 0 ? "pin_brass" : n % 3 == 1 ? "pin_red" : "pin_blue");
            pin.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            var v = UIFactory.VList(card, 10, 0);
            v.padding = new RectOffset(20, 20, 30, 20);

            UIFactory.AddLine(card, m.title, 20, UIFactory.Ink, TextAlignmentOptions.TopLeft, title: true);
            string body = hintOn ? SentenceStart.Replace(m.body, x => $"{x.Groups[1].Value}<b><color=#B8442E>{x.Groups[2].Value}</color></b>") : m.body;
            UIFactory.AddLine(card, body, 16, UIFactory.Ink).lineSpacing = 8;
            if (hintLevel == 2 && m.handler != Handler.None)
            {
                string meaning = HandlerOrders.Meaning(m.answer);
                UIFactory.AddLine(card, $"<b>Lệnh: {HandlerOrders.Key(m.answer)}</b>" + (meaning != "" ? $" — {meaning}" : ""),
                                  15, UIFactory.Hex("8C3A2B"));
            }
            var sig = UIFactory.AddLine(card, m.signature, 15, UIFactory.Muted);
            sig.fontStyle = FontStyles.Italic;
        }
    }

    void ToggleHint() => SetHint((hintLevel + 1) % 3);

    void SetHint(int level)
    {
        hintLevel = level;
        if (farmPanel && farmPanel.gameObject.activeSelf) farmPanel.gameObject.SetActive(false);
        hintNote.gameObject.SetActive(hintOn);
        UIFactory.SetLabel(hintBtn, hintLevel == 0 ? "Gợi ý đọc lệnh" : hintLevel == 1 ? "Giải sẵn lệnh" : "Tắt gợi ý");
        UIFactory.SetColor(hintBtn, hintOn ? UIFactory.Gold : UIFactory.Paper);
        barHint.text = hintLevel == 1 ? "Chữ đỏ = chữ cái đầu mỗi câu · bấm lần nữa để xem lời giải"
                     : hintLevel == 2 ? "Lời giải hiện ngay dưới mỗi lệnh mật" : "";
        BuildCards();
        RefreshFarm();
    }

    void BuildFarmPanel(RectTransform stage)
    {
        farmPanel = UIFactory.MakeRect(stage, "Farm_Overlay", Vector2.zero, Vector2.one);
        UIFactory.MakeBox(farmPanel, new Color(0.08f, 0.07f, 0.06f, 0.78f));
        var card = UIFactory.MakeRect(farmPanel, "Farm_Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        card.sizeDelta = new Vector2(920, 470);   // khung gỗ của ảnh dày ~40px: nội dung đặt lùi vào trong
        UIFactory.MakeSurface(card, UIFactory.Hex("E7D7B5"), 8);
        UIFactory.Skin(card.GetComponent<Image>(), "farm_card");
        UIFactory.AddShadow(card, 12);
        UIFactory.MakeTitle(UIFactory.Place(card, "Title", 52, 44, 816, 36), "MẢNH RUỘNG CỦA TOMAS", 24, UIFactory.Ink);
        UIFactory.MakeText(UIFactory.Place(card, "Help", 52, 82, 816, 30),
            "Gieo cà rốt tốn 1 AP, không dùng rau trong kho. Cây chín sau 3 buổi sáng; thu hoạch tốn 1 AP.", 15, UIFactory.Muted);
        var grid = UIFactory.Place(card, "Plots", 52, 118, 816, 200);
        var h = UIFactory.HList(grid, 12, 0);
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;
        for (int i = 0; i < EconomyRules.StartingPlotCount; i++)
        {
            int plotId = i;
            var b = UIFactory.MakeButton(grid, "Plot_" + i, "", () => OnPlot(plotId), 16);
            UIFactory.Size(b, 184);
            // Ô ruộng: thẻ giấy trơn bo góc (ảnh nút ngang bị kéo dọc sẽ có dải tối đè lên chữ)
            UIFactory.Round((Image)b.targetGraphic, 8);
            b.transition = Selectable.Transition.ColorTint;
            var label = b.GetComponentInChildren<TextMeshProUGUI>();
            label.fontSize = 13;
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.anchorMin = new Vector2(0, 0);
            label.rectTransform.anchorMax = new Vector2(1, 0.36f);
            label.rectTransform.offsetMin = new Vector2(5, 4);
            label.rectTransform.offsetMax = new Vector2(-5, -2);

            // Keep crop art separate from the Button targetGraphic. SpriteSwap can
            // replace the targetGraphic for a growing (disabled) plot; the crop
            // illustration must remain visible in every interaction state.
            var artRect = UIFactory.MakeRect(b.transform, "Plot_Art", new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            artRect.pivot = new Vector2(0.5f, 1);
            artRect.sizeDelta = new Vector2(106, 106);
            artRect.anchoredPosition = new Vector2(0, -8);
            artRect.SetAsFirstSibling();
            var art = UIFactory.MakeBox(artRect, Color.white, false);
            art.preserveAspect = true;
            plotArt.Add(art);
            plotButtons.Add(b);
        }
        var statsRect = UIFactory.Place(card, "Stats", 52, 334, 600, 28);
        var statsLayout = statsRect.gameObject.AddComponent<HorizontalLayoutGroup>();
        statsLayout.spacing = 18;
        statsLayout.childAlignment = TextAnchor.MiddleLeft;
        statsLayout.childControlWidth = true;
        statsLayout.childControlHeight = true;
        statsLayout.childForceExpandWidth = false;
        statsLayout.childForceExpandHeight = true;
        energyStat = AddFarmStat(statsRect, "Energy", "icon_lantern", 205);
        apStat = AddFarmStat(statsRect, "AP", "icon_ap", 150);
        stockStat = AddFarmStat(statsRect, "Stock", "icon_carrot", 180);
        var close = UIFactory.MakeButton(UIFactory.Place(card, "Close", 690, 372, 178, 42), "Close", "Đóng ruộng", ToggleFarm, 16);
        UIFactory.SetColor(close, UIFactory.Hex("DCCBA8"));
        farmPanel.gameObject.SetActive(false);
    }

    TextMeshProUGUI AddFarmStat(RectTransform parent, string name, string iconName, float width)
    {
        var item = UIFactory.MakeRect(parent, name, Vector2.zero, Vector2.one);
        var itemLayout = item.gameObject.AddComponent<LayoutElement>();
        itemLayout.preferredWidth = width;
        itemLayout.flexibleWidth = 0;
        var row = item.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 8;
        row.childAlignment = TextAnchor.MiddleLeft;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = true;

        var iconRect = UIFactory.MakeRect(item, "Icon", Vector2.zero, Vector2.one);
        var iconLayout = iconRect.gameObject.AddComponent<LayoutElement>();
        iconLayout.preferredWidth = iconLayout.preferredHeight = 20;
        iconLayout.flexibleWidth = iconLayout.flexibleHeight = 0;
        UIFactory.Skin(UIFactory.MakeBox(iconRect, Color.white, false), iconName);
        iconRect.GetComponent<Image>().preserveAspect = true;

        var textRect = UIFactory.MakeRect(item, "Value", Vector2.zero, Vector2.one);
        var textLayout = textRect.gameObject.AddComponent<LayoutElement>();
        textLayout.flexibleWidth = 1;
        var text = UIFactory.MakeText(textRect, "", 14, UIFactory.Ink, TextAlignmentOptions.MidlineLeft);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        return text;
    }

    void ToggleFarm()
    {
        farmPanel.gameObject.SetActive(!farmPanel.gameObject.activeSelf);
        RefreshFarm();
    }

    void OnPlot(int plotId)
    {
        var plot = S.farmPlots.Find(p => p.plotId == plotId);
        if (plot == null) return;
        if (string.IsNullOrEmpty(plot.cropId)) FarmSystem.Plant(S, plotId, dm.FindCrop(FarmSystem.CarrotId));
        else if (plot.ready) FarmSystem.Harvest(S, plotId, dm.AllCrops);
        RefreshFarm();
    }

    void RefreshFarm()
    {
        if (S == null) return;
        FarmSystem.EnsurePlots(S);
        if (farmBtn) UIFactory.SetLabel(farmBtn, $"Ruộng · {S.actionPoints} AP · {S.stock} bó");
        if (plotButtons.Count > 0)
        {
            foreach (var b in plotButtons)
            {
                int id = plotButtons.IndexOf(b);
                var plot = S.farmPlots.Find(p => p.plotId == id);
                bool empty = string.IsNullOrEmpty(plot.cropId);
                string state = empty ? "Ô trống\nGieo cà rốt · 1 AP" : plot.ready ? "Cà rốt chín\nThu hoạch · 1 AP" : $"Cà rốt\nĐang lớn {plot.daysGrowing}/{EconomyRules.CarrotGrowDays}";
                b.GetComponentInChildren<TextMeshProUGUI>().text = state;
                string plotSprite = empty ? "plot_empty" : plot.ready ? "plot_ready" : plot.daysGrowing == 0 ? "plot_seeded" : "plot_growing";
                UIFactory.Skin(plotArt[id], plotSprite);
                plotArt[id].preserveAspect = true;
                // Không làm mờ ô đang lớn (chữ sẽ không đọc được); bấm vào ô đang lớn chỉ không có tác dụng
                bool canAct = S.actionPoints > 0 && (plot.ready || empty);
                b.interactable = true;
                foreach (var tx in b.GetComponentsInChildren<TMP_Text>()) tx.alpha = 1f;
                if (!canAct && (plot.ready || empty)) state += "\n<color=#A8442E>Hết AP hôm nay</color>";
                b.GetComponentInChildren<TextMeshProUGUI>().text = state;
                UIFactory.SetColor(b, UIFactory.Hex(plot.ready ? "F3E3B0" : "F4ECD8"));
            }
        }
        if (energyStat) energyStat.text = $"Năng lượng {S.energy}/{EconomyRules.MaxEnergy}";
        if (apStat) apStat.text = $"AP {S.actionPoints}/{EconomyRules.MaxActionPoints}";
        if (stockStat) stockStat.text = $"Rau trong kho {S.stock}";
    }
}
