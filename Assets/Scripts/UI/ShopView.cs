using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Tiệm buổi chiều — bố cục kiểu visual novel:
//   nhân vật đứng sau quầy (giữa trái), bảng "Khách chiều nay" góc trên phải,
//   khung thoại ngang dưới đáy có thẻ tên; lựa chọn nằm trong khung thoại, cột bên phải.
//   Thanh nút dưới: "Sổ tay" (trái) · "Sang buổi tối" (phải).
//
// Một lượt khách: dẫn truyện khách bước vào → câu chào → các lượt hỏi–đáp → câu chào về → dẫn truyện khách rời đi.
// Khung thoại GIỮ CẢ CUỘC TRÒ CHUYỆN (cuộn được), đoạn cũ in mờ hơn. Hết việc để làm thì khách tự chào về.
//   - Bán hàng / Bớt giá: 1 lần mỗi khách (chọn một trong hai), tốn 1 bó rau.
//   - Hỏi chuyện: lần 1 = Hỏi chuyện, lần 2 = Hỏi dồn (cộng nghi ngờ vào kênh của khách).
//   - Tiễn khách: kết thúc sớm.
public class ShopView : MonoBehaviour
{
    enum Mode { Prompt, Closed, Open }

    // Một nút lựa chọn: chữ bên trái, ghi chú nhỏ bên phải
    class Choice
    {
        public Button button;
        public TextMeshProUGUI label, note;
    }

    const string NarrationColor = "7A6A5A";
    const string OldAlpha = "<alpha=#8C>";

    DayManager dm;
    Image portrait;
    RectTransform queueList, nameTag;
    TextMeshProUGUI nameText, dialogue, queueFoot;
    ScrollRect scroll;
    Choice[] choices;
    Button notebookBtn;
    TextMeshProUGUI barHint;

    Mode mode;
    DaySO day;
    List<CustomerVisit> activeVisits = new List<CustomerVisit>();
    int index;          // khách thứ mấy trong ngày
    int askCount;       // 0 chưa hỏi, 1 đã hỏi chuyện, 2 đã hỏi dồn
    bool traded;        // đã bán hoặc bớt giá
    bool leaving;       // khách đã chào về
    readonly List<string> log = new List<string>();   // các đoạn thô (còn {{...}}), hiện nối tiếp nhau
    int fresh;          // log[fresh..] là lượt mới nhất (in đậm), trước đó in mờ

    GameState S => dm.State;
    CustomerVisit Visit => day && index < activeVisits.Count ? activeVisits[index] : null;
    string Speaker => Visit != null && Visit.customer ? Visit.customer.displayName : "";

    public void Init(DayManager manager)
    {
        dm = manager;
        var stage = UIFactory.MakeStage(transform);

        BuildQueue(stage);

        // Nhân vật: chân chạm mặt quầy
        var pr = UIFactory.Place(stage, "Shop_Portrait", 250, 28, 330, 360);
        portrait = pr.gameObject.AddComponent<Image>();
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        portrait.enabled = false;

        // Mặt quầy gỗ chạy ngang, che phần dưới nhân vật
        var counter = UIFactory.Place(stage, "Shop_Counter", 0, 372, 1280, 40);
        UIFactory.MakeBox(counter, UIFactory.Hex("5A3D29"), false);
        var edge = UIFactory.Place(counter, "Edge", 0, 0, 1280, 4);
        UIFactory.MakeBox(edge, UIFactory.Hex("82603F"), false);

        BuildDialogueBox(stage);

        var bar = UIFactory.MakeActionBar(transform);
        notebookBtn = UIFactory.BarButton(bar, "Sổ tay", () => dm.OpenNotebook());
        barHint = UIFactory.BarHint(bar, DefaultHint);
        UIFactory.BarSpacer(bar);
        UIFactory.BarButton(bar, "Sang buổi tối", () => dm.EndPhase(), primary: true);
    }

    void BuildQueue(RectTransform stage)
    {
        var q = UIFactory.Place(stage, "Shop_Queue", 880, 24, 376, 0);
        UIFactory.MakeSurface(q, new Color(0.165f, 0.133f, 0.11f, 0.94f), 8);
        UIFactory.AutoHeight(q, 22, 18, 18, 0);
        UIFactory.MakeLabel(UIFactory.MakeRect(q, "Title", Vector2.zero, Vector2.one), "KHÁCH CHIỀU NAY", UIFactory.Gold, 16);
        UIFactory.Size(UIFactory.MakeRect(q, "Gap", Vector2.zero, Vector2.one), 8);

        queueList = UIFactory.MakeRect(q, "Rows", Vector2.zero, Vector2.one);
        UIFactory.VList(queueList, 0, 0);

        UIFactory.Size(UIFactory.MakeRect(q, "Gap", Vector2.zero, Vector2.one), 10);
        queueFoot = UIFactory.AddLine(q, "", 14, UIFactory.Hex("9C8C7B"));
        queueFoot.lineSpacing = 6;
    }

    void BuildDialogueBox(RectTransform stage)
    {
        var box = UIFactory.Place(stage, "Shop_DialogueBox", 24, 376, 1232, 220);
        UIFactory.MakeSurface(box, new Color(0.957f, 0.925f, 0.847f, 0.99f), 8);
        UIFactory.Skin(box.GetComponent<Image>(), "dialogue_box");
        UIFactory.AddShadow(box, 8);

        // Thẻ tên dính mép trên trái khung thoại, rộng theo chữ
        nameTag = UIFactory.Place(stage, "Shop_NameTag", 52, 380, 0, 38);
        UIFactory.MakeSurface(nameTag, UIFactory.Hex("5B3A29"), 5, false);
        UIFactory.Skin(nameTag.GetComponent<Image>(), "name_tag");
        var h = UIFactory.HList(nameTag, 0, 0);
        h.padding = new RectOffset(26, 26, 0, 0);
        nameTag.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        nameText = UIFactory.MakeTitle(UIFactory.MakeRect(nameTag, "Text", Vector2.zero, Vector2.one), "", 19, UIFactory.Light, TextAlignmentOptions.Midline);
        nameText.textWrappingMode = TextWrappingModes.NoWrap;

        // Lời thoại: bên trái, cuộn được
        var scrollRoot = UIFactory.MakeRect(box, "Dialogue_Scroll", Vector2.zero, Vector2.one);
        scrollRoot.offsetMin = new Vector2(30, 28);       // viền hoa văn dưới của khung thoại dày ~20px
        scrollRoot.offsetMax = new Vector2(-340, -46);   // chừa chỗ cho thẻ tên, chữ không chui dưới thẻ
        var content = UIFactory.MakeScroll(scrollRoot, out scroll);
        dialogue = UIFactory.MakeText(UIFactory.MakeRect(content, "Dialogue", Vector2.zero, Vector2.one), "", 17, UIFactory.Ink);
        dialogue.lineSpacing = 10;
        dialogue.paragraphSpacing = 10;
        dialogue.raycastTarget = true;   // để bấm được cụm từ màu
        dialogue.gameObject.AddComponent<ClickableText>().LinkClicked += OnIntelClicked;

        // Đường kẻ dọc + cột lựa chọn bên phải
        var vr = UIFactory.MakeRect(box, "Divider", new Vector2(1, 0), new Vector2(1, 1));
        vr.sizeDelta = new Vector2(1, -28);
        vr.anchoredPosition = new Vector2(-318, 0);
        UIFactory.MakeBox(vr, new Color(0.23f, 0.18f, 0.15f, 0.2f), false);

        var col = UIFactory.MakeRect(box, "Choices", new Vector2(1, 0), new Vector2(1, 1));
        col.pivot = new Vector2(1, 0.5f);
        col.sizeDelta = new Vector2(290, 0);
        col.anchoredPosition = new Vector2(-22, 0);      // chừa viền hoa văn bên phải khung thoại
        var v = UIFactory.VList(col, 7, 0);
        v.padding = new RectOffset(10, 4, 20, 20);
        v.childAlignment = TextAnchor.MiddleCenter;

        choices = new Choice[4];
        for (int i = 0; i < choices.Length; i++)
        {
            int k = i;
            choices[i] = MakeChoice(col, () => OnChoice(k));
        }
    }

    Choice MakeChoice(RectTransform col, UnityEngine.Events.UnityAction onClick)
    {
        var b = UIFactory.MakeButton(col, "Choice", "", onClick, 16);
        UIFactory.Size(b, 34);
        var label = b.GetComponentInChildren<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.rectTransform.offsetMin = new Vector2(14, 0);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.enableAutoSizing = true;
        label.fontSizeMin = 12;
        label.fontSizeMax = 16;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.rectTransform.offsetMax = new Vector2(-112, 0);
        var note = UIFactory.MakeText(UIFactory.MakeRect(b.transform, "Note", Vector2.zero, Vector2.one), "", 14,
                                      UIFactory.Hex(NarrationColor), TextAlignmentOptions.MidlineRight);
        note.fontSize = 12;
        note.textWrappingMode = TextWrappingModes.NoWrap;
        note.overflowMode = TextOverflowModes.Ellipsis;
        note.rectTransform.anchorMin = new Vector2(1, 0);
        note.rectTransform.anchorMax = new Vector2(1, 1);
        note.rectTransform.pivot = new Vector2(1, 0.5f);
        note.rectTransform.sizeDelta = new Vector2(94, 0);
        note.rectTransform.anchoredPosition = new Vector2(-14, 0);
        return new Choice { button = b, label = label, note = note };
    }

    // ---------------- Luồng ----------------

    public void Begin(DaySO today)
    {
        day = today;
        activeVisits = AllVisits();
        index = 0;
        mode = Mode.Prompt;
        if (barHint) barHint.text = DefaultHint;
        log.Clear();
        fresh = 0;

        portrait.enabled = false;
        nameText.text = "Tomas";
        log.Add(Narration("Buổi chiều. Tomas lau quầy, nhìn ra con phố vắng."));
        log.Add($"Mở tiệm thì mất {EconomyRules.OpenShopEnergy} năng lượng (còn {S.energy}). Trong kho còn {S.stock} bó rau." +
                (activeVisits.Count > 0 ? "" : "\n" + Narration("(Hôm nay không có khách ghé tiệm.)")));
        Render();
    }

    void OpenShop()
    {
        S.energy -= EconomyRules.OpenShopEnergy;
        S.shopOpenedToday = true;
        activeVisits = AllVisits();
        mode = Mode.Open;
        StartVisit();
    }

    const string DefaultHint = "Bấm cụm từ gạch chân màu đỏ trong lời khách để ghi vào sổ tay";

    void StayClosed()
    {
        // Nghỉ chiều nay: không tốn năng lượng (trước đây cộng thêm năng lượng, làm vượt mức thật)
        mode = Mode.Closed;
        log.Clear();
        fresh = 0;
        log.Add(Narration("Tomas treo tấm biển \"Hôm nay nghỉ\" lên cửa. Chiều nay không có khách, cũng không có tin."));
        Render();
    }

    void StartVisit()
    {
        askCount = 0;
        traded = false;
        leaving = false;
        log.Clear();
        fresh = 0;

        var v = Visit;
        bool hasCustomer = v != null && v.customer;
        portrait.enabled = hasCustomer && v.customer.portrait;

        if (!hasCustomer)
        {
            nameText.text = "Tomas";
            log.Add(Narration("Con phố đã vắng người. Tomas kéo cửa tiệm xuống, chờ trời tối."));
            Render();
            return;
        }

        if (portrait.enabled) portrait.sprite = v.customer.portrait;
        GameAudio.Sfx("bell", 0.6f);   // chuông cửa khi khách bước vào
        nameText.text = v.customer.displayName;

        string entrance = Conditions.Pick(S, v.entrance, v.entranceVariants);
        if (!string.IsNullOrEmpty(entrance)) log.Add(Narration(entrance));
        log.Add($"<b>{Speaker}:</b> {Conditions.Pick(S, v.greeting, v.greetingVariants)}");
        Render();
    }

    // Thêm một lượt hỏi–đáp vào cuối cuộc trò chuyện
    void Say(DialogueLine line, string note = null)
    {
        fresh = log.Count;
        log.Add($"<b>Tomas:</b> {line.tomas}");
        log.Add($"<b>{Speaker}:</b> {Conditions.Pick(S, line.reply, line.replyVariants)}" +
                (note != null ? $"  <size=85%><color=#{NarrationColor}>({note})</color></size>" : ""));
    }

    // Khách chào về + dẫn truyện rời đi
    void Leave()
    {
        leaving = true;
        Say(Visit.farewell);
        string exit = Conditions.Pick(S, Visit.exit, Visit.exitVariants);
        if (!string.IsNullOrEmpty(exit)) log.Add(Narration(exit));
    }

    // Sau mỗi lựa chọn: nếu không còn gì để làm thì khách tự chào về
    void AfterAction()
    {
        if (!leaving && !CanTrade && !CanAsk) Leave();
        Render();
    }

    bool HasStockFor(int quantity) => quantity < 0 ? S.stock > 0 : quantity == 0 || S.stock >= quantity;
    int QuantityAvailable(int requested) => requested < 0 ? S.stock : requested;
    bool CanSell => !traded && Visit != null && Visit.sellQuantity != 0 && HasStockFor(Visit.sellQuantity);   // -1 = mua hết kho
    bool CanDiscount => !traded && Visit != null && Visit.discount != null && HasStockFor(DiscountQuantity(Visit));
    bool CanTrade => CanSell || CanDiscount;
    bool CanAsk => Visit != null && askCount < 2 && (askCount > 0 || S.money >= Visit.askCost) && (askCount == 0 || Visit.press != null && !string.IsNullOrEmpty(Visit.press.reply));

    static string Narration(string text) => $"<i><color=#{NarrationColor}>{text}</color></i>";

    void Render()
    {
        var parts = new List<string>();
        for (int i = 0; i < log.Count; i++)
        {
            string rich = IntelMarkup.ToRichText(log[i], S);
            parts.Add(i < fresh ? OldAlpha + rich + "<alpha=#FF>" : rich);
        }
        dialogue.text = string.Join("\n", parts);
        RefreshChoices();
        RefreshQueue();
        RefreshNotebookButton();
        dialogue.ForceMeshUpdate();
        UIFactory.ScrollToBottom(scroll);
        if (isActiveAndEnabled) StartCoroutine(ScrollNextFrame());   // TMP đo xong chiều cao ở frame sau
    }

    System.Collections.IEnumerator ScrollNextFrame()
    {
        yield return null;
        UIFactory.ScrollToBottom(scroll);
    }

    public void RefreshNotebookButton() => UIFactory.SetLabel(notebookBtn, $"Sổ tay  ·  {S.fragments.Count} manh mối");

    // ---------------- Lựa chọn ----------------

    // Mỗi chế độ dùng các ô lựa chọn theo thứ tự; ô thừa thì ẩn
    void SetChoice(int i, string label, string note = "", bool on = true, bool gold = false)
    {
        var c = choices[i];
        c.button.gameObject.SetActive(true);
        c.label.text = label;
        c.note.text = note;
        c.note.gameObject.SetActive(!string.IsNullOrEmpty(note));
        c.label.rectTransform.offsetMax = new Vector2(string.IsNullOrEmpty(note) ? -14 : -112, 0);
        UIFactory.SetColor(c.button, gold ? UIFactory.Gold : UIFactory.ButtonTan);
        UIFactory.SetInteractable(c.button, on);
    }

    // Giá bán cố định theo từng khách (bỏ nút "Giá bán" 04/11: nó không ảnh hưởng gì thấy được)
    List<CustomerVisit> AllVisits() => day && day.visits != null
        ? day.visits.Where(v => v != null).ToList()
        : new List<CustomerVisit>();

    void RefreshChoices()
    {
        foreach (var c in choices) c.button.gameObject.SetActive(false);

        switch (mode)
        {
            case Mode.Prompt:
                SetChoice(0, "Mở tiệm", $"−{EconomyRules.OpenShopEnergy} năng lượng", S.energy >= EconomyRules.OpenShopEnergy, gold: true);
                SetChoice(1, "Nghỉ chiều nay");
                return;
            case Mode.Closed:
                return;   // chỉ còn nút "Sang buổi tối" ở thanh dưới
        }

        var v = Visit;
        if (v == null || !v.customer) return;   // hết khách: dùng nút "Sang buổi tối" ở thanh dưới

        if (leaving)
        {
            SetChoice(0, index + 1 < activeVisits.Count ? "Mời khách sau" : "Đóng tiệm", "", true, gold: true);
            return;
        }

        string sellLabel = traded ? "Đã giao dịch" : v.sellQuantity == 0 ? "Khách không mua rau" :
            v.sellQuantity < 0 ? $"Bán hết kho ({S.stock} bó)" : $"Bán {v.sellQuantity} bó";
        int sellCount = QuantityAvailable(v.sellQuantity);
        int sellRevenue = v.sellPrice * Mathf.Max(0, sellCount);
        SetChoice(0, sellLabel, sellRevenue > 0 ? $"+{sellRevenue} xu" : "", CanSell);
        int discountCount = QuantityAvailable(DiscountQuantity(v));
        int discountRevenue = DiscountUnitPrice(v) * Mathf.Max(0, discountCount);
        string discountLabel = string.IsNullOrEmpty(v.discountActionLabel)
            ? (v.discountAccepted ? "Bớt giá" : "Đề nghị bớt giá") + (!traded && discountCount > 0 ? $" · {discountCount} bó" : "")
            : v.discountActionLabel;
        SetChoice(1, discountLabel, discountRevenue > 0 ? $"+{discountRevenue} xu" : "", CanDiscount);
        SetChoice(2, askCount == 0 ? "Hỏi chuyện" : "Hỏi dồn",
                  askCount == 0 ? (v.askCost > 0 ? $"−{v.askCost} xu" : "") : "dễ bị để ý", CanAsk);
        SetChoice(3, "Tiễn khách");
    }

    void OnChoice(int i)
    {
        switch (mode)
        {
            case Mode.Prompt:
                if (i == 0) OpenShop();
                else StayClosed();
                return;
            case Mode.Closed:
                dm.EndPhase();
                return;
        }

        var v = Visit;
        if (v == null || !v.customer) { dm.EndPhase(); return; }
        if (leaving) { index++; StartVisit(); return; }

        switch (i)
        {
            case 0: OnSell(); break;
            case 1: OnDiscount(); break;
            case 2: OnAsk(); break;
            case 3: Leave(); Render(); break;
        }
    }

    void OnSell()
    {
        var v = Visit;
        if (!CanSell) return;
        traded = true;
        int quantity = QuantityAvailable(v.sellQuantity);
        int revenue = v.sellPrice * quantity;
        S.stock -= quantity;
        if (v.sellPrice > 0) S.salesToday += quantity;
        S.money += revenue;
        Say(v.sell, $"{quantity} bó rau · +{revenue} xu");
        AfterAction();
    }

    void OnDiscount()
    {
        var v = Visit;
        if (!CanDiscount) return;
        traded = true;
        int quantity = QuantityAvailable(DiscountQuantity(v));
        int revenue = DiscountUnitPrice(v) * quantity;
        S.stock -= quantity;
        if (v.discountPrice > 0) S.salesToday += quantity;
        S.money += revenue;
        string outcome = quantity > 0 ? (revenue > 0 ? $"{quantity} bó rau · +{revenue} xu" : $"tặng {quantity} bó rau") : "không xuất rau · không thu tiền";
        Say(v.discount, outcome);
        AfterAction();
    }

    int DiscountUnitPrice(CustomerVisit visit) => visit.discountAccepted ? visit.discountPrice : visit.sellPrice;

    // Bớt giá = bán đúng số bó khách định mua, nhưng rẻ hơn mỗi bó (2 bó × 6 xu thay vì 2 bó × 10 xu).
    // Riêng biếu không (giá 0) và trao đổi đặc biệt (đổi rau lấy tin) giữ số bó ghi trong kịch bản.
    static int DiscountQuantity(CustomerVisit visit) =>
        visit.discountPrice > 0 && string.IsNullOrEmpty(visit.discountActionLabel) && visit.sellQuantity != 0
            ? visit.sellQuantity
            : visit.discountQuantity;

    void OnAsk()
    {
        var v = Visit;
        if (askCount == 0)
        {
            if (S.money < v.askCost) return;
            S.money -= v.askCost;
            askCount = 1;
            Say(v.ask, v.askCost > 0 ? $"−{v.askCost} xu" : null);
        }
        else if (askCount == 1)
        {
            askCount = 2;
            S.AddSuspicion(v.customer.channel, v.pressSuspicion, $"hỏi dồn {Speaker}");
            Say(v.press, $"{Speaker} có vẻ bắt đầu để ý tới cậu");
        }
        AfterAction();
    }

    void OnIntelClicked(string id)
    {
        var clue = dm.FindIntel(id);
        bool isNew = !S.HasFragment(id);
        dm.CaptureIntel(id);
        if (isNew && clue && barHint) barHint.text = $"<color=#E0B04B>Đã ghi vào sổ tay:</color> {clue.keyword}  <color=#9C8C7B>[{IntelThreadNames.Of(clue.kind)}]</color>";
        Render();
    }

    // ---------------- Bảng khách ----------------

    void RefreshQueue()
    {
        UIFactory.Clear(queueList);
        var queue = AllVisits();
        int count = queue.Count;

        if (mode == Mode.Prompt)
        {
            QueueRow("Chưa mở tiệm", count > 0 ? $"{count} người hay ghé" : "", false);
            queueFoot.text = $"Mở tiệm tốn {EconomyRules.OpenShopEnergy} năng lượng. Tiền bán rau để trả tiền nhà: {EconomyRules.Rent} xu mỗi đêm.";
            return;
        }
        if (mode == Mode.Closed)
        {
            QueueRow("Tiệm nghỉ chiều nay", "", false);
            queueFoot.text = "Không bán được rau, cũng không nghe được tin gì.";
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var visit = mode == Mode.Open ? activeVisits[i] : queue[i];
            var c = visit.customer;
            if (!c) continue;
            string status = i < index || (i == index && leaving) ? "đã về" : i == index ? "đang ở quầy" : "đang chờ";
            QueueRow($"{i + 1}. {c.displayName}", status, i == index && !leaving);
        }
        var v = Visit;
        queueFoot.text = v != null
            ? $"{(v.sellQuantity > 0 ? $"Khách lấy {v.sellQuantity} bó" : v.sellQuantity < 0 ? "Khách lấy hết kho" : "Khách không mua rau")} · {v.sellPrice} xu/bó\nTrong kho {S.stock} bó rau · tiền nhà đêm nay {EconomyRules.Rent} xu."
            : $"Hết khách. Trong kho còn {S.stock} bó rau.";
    }

    void QueueRow(string left, string right, bool current)
    {
        var row = UIFactory.MakeRect(queueList, "Row", Vector2.zero, Vector2.one);
        UIFactory.Size(row, 34);
        var line = UIFactory.MakeRect(row, "Line", new Vector2(0, 1), Vector2.one);
        line.pivot = new Vector2(0.5f, 1);
        line.sizeDelta = new Vector2(0, 1);
        UIFactory.MakeBox(line, new Color(0.93f, 0.89f, 0.82f, 0.12f), false);
        var leftRect = UIFactory.MakeRect(row, "L", Vector2.zero, Vector2.one);
        leftRect.anchorMax = new Vector2(0.62f, 1);
        leftRect.offsetMin = new Vector2(0, 0);
        leftRect.offsetMax = new Vector2(-8, 0);
        var leftText = UIFactory.MakeText(leftRect, left, 15,
                                          current ? Color.white : UIFactory.Light, TextAlignmentOptions.MidlineLeft);
        leftText.textWrappingMode = TextWrappingModes.NoWrap;
        leftText.overflowMode = TextOverflowModes.Ellipsis;
        var rightRect = UIFactory.MakeRect(row, "R", new Vector2(0.62f, 0), Vector2.one);
        var rightText = UIFactory.MakeText(rightRect, right, 14,
                                           current ? UIFactory.Gold : UIFactory.Hex("9C8C7B"), TextAlignmentOptions.MidlineRight);
        rightText.textWrappingMode = TextWrappingModes.NoWrap;
        rightText.overflowMode = TextOverflowModes.Ellipsis;
    }
}
