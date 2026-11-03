using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Thanh trên cùng, hiện ở mọi pha:
//   trái: tên buổi (N2 · THỨ HAI · BUỔI CHIỀU) + tiền, rau, năng lượng, thư nhắc tiền nhà
//   phải: viên "Nghi ngờ: Thấp/Vừa/Cao" — bấm để xem chi tiết từng kênh (số liệu cho NCKH).
public class HudView : MonoBehaviour
{
    DayManager dm;
    TextMeshProUGUI phase, info, susLabel;
    Image susPill;
    RectTransform detailBox;
    TextMeshProUGUI detail;
    string lastPhase, lastInfo, lastSus, lastDetail;
    Image phaseIcon;

    public void Init(DayManager manager)
    {
        dm = manager;
        var root = (RectTransform)transform;
        UIFactory.Skin(UIFactory.MakeBox(root, UIFactory.Frame), "hud_bar");
        var phaseIconRt = UIFactory.Place(root, "PhaseIcon", 4, 11, 22, 22);
        phaseIcon = UIFactory.MakeBox(phaseIconRt, Color.white, false);
        UIFactory.Skin(phaseIcon, "icon_calendar");
        var line = UIFactory.MakeRect(root, "BottomLine", Vector2.zero, new Vector2(1, 0));
        line.pivot = new Vector2(0.5f, 0);
        line.sizeDelta = new Vector2(0, 1);
        UIFactory.MakeBox(line, UIFactory.FrameLine, false);

        // Tên buổi: chữ vàng font tiêu đề
        var pr = UIFactory.Place(root, "HUD_Phase", 32, 0, 318, UIFactory.HudHeight);
        phase = UIFactory.MakeTitle(pr, "", 16, UIFactory.Gold, TextAlignmentOptions.MidlineLeft);
        phase.characterSpacing = 2;
        phase.textWrappingMode = TextWrappingModes.NoWrap;
        phase.enableAutoSizing = true;
        phase.fontSizeMin = 11;
        phase.fontSizeMax = 16;
        phase.overflowMode = TextOverflowModes.Ellipsis;

        var sep = UIFactory.Place(root, "HUD_Sep", 352, 12, 1, 20);
        UIFactory.MakeBox(sep, UIFactory.FrameLine, false);

        var ir = UIFactory.Place(root, "HUD_Info", 372, 0, 680, UIFactory.HudHeight);
        info = UIFactory.MakeText(ir, "", 14, UIFactory.Light, TextAlignmentOptions.MidlineLeft);
        info.textWrappingMode = TextWrappingModes.NoWrap;
        info.enableAutoSizing = true;
        info.fontSizeMin = 10;
        info.fontSizeMax = 14;
        info.overflowMode = TextOverflowModes.Ellipsis;

        // Viên nghi ngờ ở góc phải
        var pill = UIFactory.MakeRect(root, "HUD_Suspicion", new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        pill.pivot = new Vector2(1, 0.5f);
        pill.sizeDelta = new Vector2(170, 30);
        pill.anchoredPosition = new Vector2(-20, 0);
        // Viên tròn màu trơn (không dùng ảnh meter_suspicion_*: ảnh đó có sẵn mắt + vạch,
        // đè chữ "Nghi ngờ" lên sẽ rối). Màu viên đổi theo mức.
        susPill = UIFactory.MakeBox(pill, UIFactory.Hex("3D4A33"));
        UIFactory.Round(susPill, 15);
        var suspicionIconRt = UIFactory.MakeRect(pill, "SuspicionIcon", new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        suspicionIconRt.pivot = new Vector2(0, 0.5f);
        suspicionIconRt.anchoredPosition = new Vector2(7, 0);
        suspicionIconRt.sizeDelta = new Vector2(17, 17);
        UIFactory.Skin(UIFactory.MakeBox(suspicionIconRt, Color.white, false), "icon_suspicion");
        var btn = pill.gameObject.AddComponent<Button>();
        btn.targetGraphic = susPill;
        UIFactory.ApplyColors(btn);
        btn.onClick.AddListener(ToggleDetail);

        var lr = UIFactory.MakeRect(pill, "Label", Vector2.zero, Vector2.one);
        lr.offsetMin = new Vector2(29, 0);
        lr.offsetMax = new Vector2(-12, 0);
        susLabel = UIFactory.MakeText(lr, "", 15, UIFactory.Light, TextAlignmentOptions.MidlineLeft);
        susLabel.enableAutoSizing = true;
        susLabel.fontSizeMin = 11;
        susLabel.fontSizeMax = 15;
        susLabel.textWrappingMode = TextWrappingModes.NoWrap;

        // Bảng chi tiết thả xuống dưới viên nghi ngờ
        detailBox = UIFactory.MakeRect(root, "HUD_Detail", Vector2.one, Vector2.one);
        detailBox.pivot = Vector2.one;
        detailBox.sizeDelta = new Vector2(440, 0);
        detailBox.anchoredPosition = new Vector2(-20, -UIFactory.HudHeight - 6);
        UIFactory.MakeBox(detailBox, new Color(0.16f, 0.13f, 0.11f, 0.97f));
        UIFactory.AddShadow(detailBox, 6);
        UIFactory.AutoHeight(detailBox, 20, 16, 16, 0);
        detail = UIFactory.AddLine(detailBox, "", 15, UIFactory.Light);
        detail.lineSpacing = 6;
        detailBox.gameObject.SetActive(false);
    }

    void ToggleDetail() => detailBox.gameObject.SetActive(!detailBox.gameObject.activeSelf);

    static string PhaseName(Phase p)
    {
        switch (p)
        {
            case Phase.News:      return "SÁNG SỚM";
            case Phase.Morning:   return "BUỔI SÁNG";
            case Phase.Afternoon: return "BUỔI CHIỀU";
            case Phase.Night:     return "BAN ĐÊM";
            case Phase.Resolve:   return "TỔNG KẾT";
            default:              return "KẾT THÚC";
        }
    }

    void Update()
    {
        var s = dm.State;
        if (s == null) return;

        string p = $"N{s.day} · {s.Weekday.ToUpper()} · {PhaseName(dm.CurrentPhase)}";
        if (p != lastPhase)
        {
            phase.text = p;
            string phaseSprite = dm.CurrentPhase == Phase.Night ? "icon_moon" : dm.CurrentPhase == Phase.Morning || dm.CurrentPhase == Phase.Afternoon ? "icon_sun" : "icon_calendar";
            UIFactory.Skin(phaseIcon, phaseSprite);
            lastPhase = p;
        }

        const string gap = "<space=22>";
        string i = $"Tiền <b>{s.money}</b> xu <color=#9C8C7B><size=85%>(nhà {EconomyRules.Rent}/đêm)</size></color>{gap}" +
                   $"Rau <b>{s.stock}</b> bó{gap}" +
                   $"Năng lượng <b>{s.energy}</b>/{EconomyRules.MaxEnergy}{gap}" +
                   $"Sổ tay <b>{s.fragments.Count}</b>" +
                   (s.rentWarnings > 0 ? $"{gap}<color=#E0B04B>Thư nhắc {s.rentWarnings}/{EconomyRules.MaxWarnings}</color>" : "");
        if (i != lastInfo) { info.text = i; lastInfo = i; }

        int w = s.SuspicionWindow;
        string tier = SuspicionRules.Tier(w);
        if (tier != lastSus)
        {
            susLabel.text = $"Nghi ngờ: {tier}";
            susPill.color = UIFactory.Hex(w >= SuspicionRules.HighFrom ? "5E2A22" : w >= SuspicionRules.MidFrom ? "5A4A26" : "3D4A33");
            lastSus = tier;
        }

        if (detailBox.gameObject.activeSelf)
        {
            var sb = new StringBuilder();
            sb.Append($"<b>Cửa sổ {SuspicionRules.WindowNights} đêm: {w} / {SuspicionRules.BurnThreshold}</b>" +
                      $"  <color=#9C8C7B>(cộng dồn cả ván: {s.SuspicionTotal})</color>\n");
            for (int c = 0; c < 4; c++)
            {
                var ch = (SuspicionChannel)c;
                sb.Append($"• {SuspicionRules.ChannelName(ch)}: {s.ChannelWindow(ch)}" +
                          (s.pendingByChannel[c] > 0 ? $"  <color=#E0B04B>(+{s.pendingByChannel[c]} hôm nay)</color>" : "") + "\n");
            }
            sb.Append("<color=#9C8C7B>Từng đêm: ");
            for (int n = 0; n < s.nightlySuspicion.Count; n++) sb.Append($"N{n + 1}={s.nightlySuspicion[n]}  ");
            sb.Append("\n<size=90%>Điểm hôm nay được chốt lúc tổng kết. Chỉ 3 đêm gần nhất được tính: " +
                      "nằm im vài đêm thì nghi ngờ giảm dần.</size></color>");
            string d = sb.ToString();
            if (d != lastDetail) { detail.text = d; lastDetail = d; }
        }
    }
}
