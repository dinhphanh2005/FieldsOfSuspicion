using System;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

// Sổ tay: mảnh tin đã ghi, tự xếp theo mạch.
public class NotebookView : MonoBehaviour
{
    DayManager dm;
    TextMeshProUGUI content;

    public void Init(DayManager manager)
    {
        dm = manager;
        var stage = UIFactory.MakeStage(transform);

        // Trang sổ: giấy kẻ lề đỏ, cuộn được khi ghi nhiều
        var page = UIFactory.Place(stage, "Notebook_Page", 140, 16, 1000, 576);
        UIFactory.MakeSurface(page, UIFactory.Hex("F3EAD6"), 8);
        UIFactory.Skin(page.GetComponent<UnityEngine.UI.Image>(), "notebook_page");
        UIFactory.AddShadow(page, 10);
        var tape = UIFactory.Place(page, "Tape", 836, 8, 118, 42);
        UIFactory.Skin(UIFactory.MakeBox(tape, Color.white, false), "tape");
        var margin = UIFactory.Place(page, "Margin", 70, 0, 2, 576);
        UIFactory.MakeBox(margin, UIFactory.Hex("C98B7A"), false);

        UIFactory.MakeTitle(UIFactory.Place(page, "Title", 96, 26, 860, 40), "Sổ tay của Tomas", 26, UIFactory.Ink);
        var sub = UIFactory.MakeText(UIFactory.Place(page, "Sub", 96, 64, 860, 24),
                                     "Các mảnh tin đã ghi, xếp theo mạch. Ban đêm chọn từ đây để giao.", 14, UIFactory.Muted);
        sub.fontStyle = FontStyles.Italic;

        var scrollRoot = UIFactory.Place(page, "Scroll", 96, 104, 860, 450);
        var list = UIFactory.MakeScroll(scrollRoot, out _);
        content = UIFactory.AddLine(list, "", 18, UIFactory.Ink);
        content.lineSpacing = 10;

        var bar = UIFactory.MakeActionBar(transform);
        UIFactory.BarSpacer(bar);
        UIFactory.BarButton(bar, "Gấp sổ lại", () => dm.CloseNotebook(), primary: true);
    }

    // Trạng thái mạch: Nhà Vaule "Được xác nhận" khi có >= 2 mảnh; Kho thóc "Bị mâu thuẫn" khi có cả F03 và F04.
    // Mạch "Ai là kẻ phản bội?" không bao giờ tự kết luận.
    static string ThreadStatus(IntelThread t, GameState s)
    {
        if (t == IntelThread.Vaule)
        {
            int n = s.fragments.FindAll(id => id == "F05" || id == "F06" || id == "F07").Count;
            if (n >= 2) return "  <color=#7FA07A>(Được xác nhận)</color>";
        }
        if (t == IntelThread.Granary && s.HasFragment("F03") && s.HasFragment("F04"))
            return "  <color=#D98A3D>(Bị mâu thuẫn)</color>";
        return "";
    }

    public void Refresh()
    {
        var s = dm.State;
        var sb = new StringBuilder();

        foreach (IntelThread thread in Enum.GetValues(typeof(IntelThread)))
        {
            var items = s.fragments.Select(dm.FindIntel).Where(i => i && i.thread == thread).ToList();
            if (items.Count == 0) continue;

            sb.Append($"<size=115%><b>{IntelThreadNames.Of(thread)}</b></size>{ThreadStatus(thread, s)}\n");
            foreach (var i in items)
            {
                var drop = s.deliveredDrops.Find(d => d.intelId == i.id);
                string truth = s.TruthStatus(i.id) switch
                {
                    IntelTruthStatus.Confirmed => " · Được xác nhận",
                    IntelTruthStatus.Contradicted => " · Bị mâu thuẫn",
                    IntelTruthStatus.Closed => " · Đã khép lại",
                    _ => " · Chưa xác minh"
                };
                string consequence = drop == null ? "" : drop.mode switch
                {
                    DeliveryMode.Scrambled => ", đã giao xáo trộn",
                    DeliveryMode.False => ", đã giao tin giả",
                    _ => ", đã giao"
                };
                sb.Append($"•  {i.text}  <size=75%><color=#9C8C7B>({i.source}{truth}{consequence})</color></size>\n");
            }
            sb.Append('\n');
        }

        content.text = sb.Length > 0 ? sb.ToString() : "<color=#8C7B6B>Sổ tay còn trống. Khi khách nói chuyện, bấm vào cụm từ gạch chân màu đỏ để ghi tin.</color>";
    }
}
