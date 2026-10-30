using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Tiệm buổi chiều: khách lần lượt ghé, mọi tương tác nằm trong khung thoại.
// Bán hàng / Bớt giá: 1 lần mỗi khách (chọn một trong hai).
// Hỏi chuyện: lần 1 = Hỏi chuyện, lần 2 = Hỏi dồn. Sable thu tiền ở lần hỏi đầu.
// Tiễn khách: hiện câu chào tạm biệt, bấm lần nữa thì mời khách sau.
// Tiền: Bán hàng +10 xu, Bớt giá +6 xu (số tạm, chỉnh khi playtest). Nghi ngờ khi hỏi dồn: Sprint 2.
public class ShopView : MonoBehaviour
{
    const int SellPrice = 10;       // số tạm — chỉnh khi playtest
    const int DiscountPrice = 6;

    DayManager dm;
    Image portrait;
    TextMeshProUGUI nameText, dialogue, moneyText;
    Button sellBtn, discountBtn, askBtn, farewellBtn;

    DaySO day;
    int index;          // khách thứ mấy trong ngày
    int askCount;       // 0 chưa hỏi, 1 đã hỏi chuyện, 2 đã hỏi dồn
    bool traded;        // đã bán hoặc bớt giá
    bool leaving;       // đã bấm Tiễn khách
    string tomasLine;   // câu thô (còn {{...}})
    string replyLine;
    string note;        // dòng ghi chú nhỏ dưới thoại, ví dụ "+10 xu"

    GameState S => dm.State;
    CustomerVisit Visit => day && index < day.visits.Count ? day.visits[index] : null;

    public void Init(DayManager manager)
    {
        dm = manager;
        var img = transform.Find("Img_Customer");
        portrait = img ? img.GetComponent<Image>() : null;

        moneyText = UIFactory.MakeText(UIFactory.MakeRect(transform, "Shop_Money", new Vector2(0.72f, 0.88f), new Vector2(0.96f, 0.95f)),
                                       "", 22, UIFactory.Ink, TextAlignmentOptions.Right);

        var box = UIFactory.MakeRect(transform, "Shop_DialogueBox", new Vector2(0.42f, 0.12f), new Vector2(0.96f, 0.78f));
        UIFactory.MakeBox(box, UIFactory.Paper);

        nameText = UIFactory.MakeText(UIFactory.MakeRect(box, "Name", new Vector2(0, 0.86f), new Vector2(1, 1), 16),
                                      "", 26, UIFactory.Ink, TextAlignmentOptions.Left);
        nameText.fontStyle = FontStyles.Bold;

        dialogue = UIFactory.MakeText(UIFactory.MakeRect(box, "Dialogue", new Vector2(0, 0.24f), new Vector2(1, 0.86f), 20),
                                      "", 23, UIFactory.Ink);
        dialogue.gameObject.AddComponent<ClickableText>().LinkClicked += OnIntelClicked;

        var row = UIFactory.MakeRect(box, "Choices", new Vector2(0, 0), new Vector2(1, 0.22f), 14);
        UIFactory.HList(row, 10, 0);
        sellBtn     = UIFactory.MakeButton(row, "Btn_Sell", "Bán hàng", OnSell, 20);
        discountBtn = UIFactory.MakeButton(row, "Btn_Discount", "Bớt giá", OnDiscount, 20);
        askBtn      = UIFactory.MakeButton(row, "Btn_Ask", "Hỏi chuyện", OnAsk, 20);
        farewellBtn = UIFactory.MakeButton(row, "Btn_Farewell", "Tiễn khách", OnFarewell, 20);
        foreach (var b in new[] { sellBtn, discountBtn, askBtn, farewellBtn })
            UIFactory.SetColor(b, UIFactory.ButtonTan);
    }

    public void Begin(DaySO today)
    {
        day = today;
        index = 0;
        StartVisit();
    }

    void StartVisit()
    {
        askCount = 0;
        traded = false;
        leaving = false;
        tomasLine = null;
        note = null;

        var v = Visit;
        bool hasCustomer = v != null && v.customer;
        if (portrait) portrait.enabled = hasCustomer;
        sellBtn.transform.parent.gameObject.SetActive(hasCustomer);

        if (!hasCustomer)
        {
            nameText.text = "Tiệm vắng";
            replyLine = "Hôm nay không còn khách nào. Bấm Tiếp để sang buổi tối.";
            Render();
            return;
        }

        if (portrait) portrait.sprite = v.customer.portrait;
        nameText.text = v.customer.displayName;
        replyLine = Conditions.Pick(S, v.greeting, v.greetingVariants);
        Render();
    }

    void Choose(DialogueLine line, string noteText = null)
    {
        note = noteText;
        tomasLine = line.tomas;
        replyLine = Conditions.Pick(S, line.reply, line.replyVariants);
        Render();
    }

    void Render()
    {
        string speaker = Visit != null && Visit.customer ? Visit.customer.displayName : "";
        string text = tomasLine != null ? $"<b>Tomas:</b> {tomasLine}\n\n" : "";
        text += speaker != "" ? $"<b>{speaker}:</b> " : "";
        text += IntelMarkup.ToRichText(replyLine, S);
        if (note != null) text += $"\n\n<color=#8C7B6B><i>{note}</i></color>";
        dialogue.text = text;
        moneyText.text = $"Tiền: {S.money} xu";
        RefreshButtons();
    }

    void RefreshButtons()
    {
        var v = Visit;
        if (v == null) return;

        UIFactory.SetInteractable(sellBtn, !traded && !leaving);
        UIFactory.SetInteractable(discountBtn, !traded && !leaving);

        bool canPay = askCount > 0 || S.money >= v.askCost;
        UIFactory.SetInteractable(askBtn, askCount < 2 && !leaving && canPay);
        UIFactory.SetLabel(askBtn, askCount == 0
            ? (v.askCost > 0 ? $"Hỏi chuyện ({v.askCost} xu)" : "Hỏi chuyện")
            : "Hỏi dồn");

        bool hasNext = index + 1 < day.visits.Count;
        UIFactory.SetLabel(farewellBtn, !leaving ? "Tiễn khách" : (hasNext ? "Mời khách sau" : "Hết khách"));
    }

    void OnSell()
    {
        traded = true;
        S.money += SellPrice;
        Choose(Visit.sell, $"+{SellPrice} xu");
    }

    void OnDiscount()
    {
        traded = true;
        S.money += DiscountPrice;
        Choose(Visit.discount, $"+{DiscountPrice} xu (đã bớt giá)");
    }

    void OnAsk()
    {
        var v = Visit;
        if (askCount == 0)
        {
            if (S.money < v.askCost) return;
            S.money -= v.askCost;
            askCount = 1;
            Choose(v.ask, v.askCost > 0 ? $"-{v.askCost} xu" : null);
        }
        else if (askCount == 1)
        {
            askCount = 2;
            Choose(v.press);
        }
    }

    void OnFarewell()
    {
        if (!leaving)
        {
            leaving = true;
            Choose(Visit.farewell);
            return;
        }
        index++;
        StartVisit();
    }

    void OnIntelClicked(string id)
    {
        if (S.AddFragment(id)) GameEvents.RaiseIntelCaptured(id);
        Render();
    }
}
