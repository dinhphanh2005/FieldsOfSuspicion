using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Ban đêm: lên lộ trình trên bản đồ → chọn mảnh tin → "Đi". Hoặc "Ở nhà tối nay".
// Luật lộ trình: bắt đầu ở Nhà, chỉ đi theo cạnh nối, không quay lại nút cũ, tối đa 4 bước, kết thúc ở Hốc cây sồi.
// Bấm lại nút cuối của lộ trình = bỏ bước đó.
// Sprint 1: rủi ro cố định theo bảng mục 9 PDF (N1–N2). Ngập ven sông, Cầu Bắc ẩn rủi ro: Sprint 2.
public class NightView : MonoBehaviour
{
    enum Risk { Safe, Low, Medium, High }

    class Node { public string id, name; public Vector2 pos; public Button button; }
    class Edge { public string a, b; public Risk risk; }

    const int MaxSteps = 4;

    static readonly Node[] Nodes =
    {
        new Node { id = "home",   name = "Nhà",            pos = new Vector2(-520, -60) },
        new Node { id = "well",   name = "Giếng làng",     pos = new Vector2(-390, 90) },
        new Node { id = "market", name = "Chợ",            pos = new Vector2(-200, 20) },
        new Node { id = "church", name = "Nhà thờ",        pos = new Vector2(-260, 175) },
        new Node { id = "bridge", name = "Cầu Bắc",        pos = new Vector2(-20, 175) },
        new Node { id = "river",  name = "Đường ven sông", pos = new Vector2(-300, -230) },
        new Node { id = "oak",    name = "Hốc cây sồi",    pos = new Vector2(-30, -60) },
    };

    static readonly Edge[] Edges =
    {
        new Edge { a = "home",   b = "river",  risk = Risk.Safe },
        new Edge { a = "river",  b = "oak",    risk = Risk.Safe },
        new Edge { a = "home",   b = "well",   risk = Risk.Low },
        new Edge { a = "well",   b = "market", risk = Risk.Medium },
        new Edge { a = "market", b = "oak",    risk = Risk.Medium },
        new Edge { a = "well",   b = "church", risk = Risk.Safe },
        new Edge { a = "market", b = "church", risk = Risk.Low },
        new Edge { a = "church", b = "bridge", risk = Risk.Safe },
        new Edge { a = "bridge", b = "oak",    risk = Risk.Low },
    };

    DayManager dm;
    TextMeshProUGUI routeText;
    RectTransform intelList;
    Button goBtn;

    readonly List<string> route = new List<string>();
    readonly Dictionary<string, Button> intelButtons = new Dictionary<string, Button>();
    string selectedIntel;

    GameState S => dm.State;
    string Last => route[route.Count - 1];

    public void Init(DayManager manager)
    {
        dm = manager;

        // Nút "Tiếp" cũ của panel đêm không dùng nữa: thay bằng "Đi" / "Ở nhà tối nay"
        var oldNext = transform.Find("Btn_Next");
        if (oldNext) oldNext.gameObject.SetActive(false);

        BuildMap();
        BuildSidePanel();
    }

    void BuildMap()
    {
        var map = UIFactory.MakeRect(transform, "Night_Map", Vector2.zero, Vector2.one);

        foreach (var e in Edges)
        {
            Vector2 a = Find(e.a).pos, b = Find(e.b).pos;
            var line = UIFactory.MakeRectAt(map, $"Edge_{e.a}_{e.b}", (a + b) / 2, new Vector2(Vector2.Distance(a, b), 7));
            line.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
            UIFactory.MakeBox(line, RiskColor(e.risk), false);
        }

        foreach (var n in Nodes)
        {
            var rt = UIFactory.MakeRectAt(map, "Node_" + n.id, n.pos, new Vector2(170, 48));
            string id = n.id;
            n.button = UIFactory.MakeButton(rt, "Button", n.name, () => OnNode(id), 19);
        }

        var legend = UIFactory.MakeRect(transform, "Night_Legend", new Vector2(0.02f, 0.03f), new Vector2(0.58f, 0.1f));
        UIFactory.MakeText(legend,
            $"Rủi ro:  <color=#{Hex(Risk.Safe)}>an toàn</color> · <color=#{Hex(Risk.Low)}>thấp</color> · " +
            $"<color=#{Hex(Risk.Medium)}>vừa</color> · <color=#{Hex(Risk.High)}>cao</color>",
            18, UIFactory.Light, TextAlignmentOptions.BottomLeft);
    }

    void BuildSidePanel()
    {
        var side = UIFactory.MakeRect(transform, "Night_Side", new Vector2(0.6f, 0.1f), new Vector2(0.97f, 0.78f));
        UIFactory.MakeBox(side, UIFactory.NightBox);
        UIFactory.VList(side, 10, 16);

        routeText = UIFactory.MakeText(UIFactory.MakeRect(side, "Route", Vector2.zero, Vector2.one), "", 20, UIFactory.Light);

        var label = UIFactory.MakeText(UIFactory.MakeRect(side, "IntelLabel", Vector2.zero, Vector2.one), "Mảnh tin giao đêm nay:", 20, UIFactory.Light);
        label.fontStyle = FontStyles.Bold;

        intelList = UIFactory.MakeRect(side, "IntelList", Vector2.zero, Vector2.one);
        UIFactory.VList(intelList, 6, 0);

        var buttons = UIFactory.MakeRect(side, "Buttons", Vector2.zero, Vector2.one);
        UIFactory.Size(buttons, 56);
        UIFactory.HList(buttons, 8, 0);
        UIFactory.MakeButton(buttons, "Btn_Reset", "Làm lại", ResetRoute, 19);
        goBtn = UIFactory.MakeButton(buttons, "Btn_Go", "Đi", OnGo, 19);
        UIFactory.MakeButton(buttons, "Btn_Stay", "Ở nhà tối nay", () => dm.CompleteNight(null), 19);
    }

    public void Begin()
    {
        selectedIntel = null;
        BuildIntelList();
        ResetRoute();
    }

    void BuildIntelList()
    {
        UIFactory.Clear(intelList);
        intelButtons.Clear();

        foreach (var id in S.fragments)
        {
            var intel = dm.FindIntel(id);
            if (!intel || intel.onlyLastNight) continue;
            if (S.deliveredDrops.Exists(d => d.intelId == id)) continue;   // đã giao thì không giao lại

            string captured = id;
            var b = UIFactory.MakeButton(intelList, "Intel_" + id,
                $"<b>[{IntelThreadNames.Of(intel.thread)}]</b> {intel.text}", () => SelectIntel(captured), 17);
            UIFactory.Size(b, 64);
            intelButtons[id] = b;
        }

        if (intelButtons.Count == 0)
            UIFactory.MakeText(UIFactory.MakeRect(intelList, "Empty", Vector2.zero, Vector2.one),
                               "Sổ tay chưa có tin nào để giao.", 18, UIFactory.Muted);
    }

    void SelectIntel(string id)
    {
        selectedIntel = id;
        Refresh();
    }

    void ResetRoute()
    {
        route.Clear();
        route.Add("home");
        Refresh();
    }

    void OnNode(string id)
    {
        if (route.Count > 1 && id == Last) route.RemoveAt(route.Count - 1);   // bỏ bước cuối
        else if (CanAdd(id)) route.Add(id);
        Refresh();
    }

    bool CanAdd(string id) =>
        Last != "oak" && route.Count - 1 < MaxSteps && !route.Contains(id) && Connected(Last, id);

    void Refresh()
    {
        foreach (var n in Nodes)
        {
            bool inRoute = route.Contains(n.id);
            UIFactory.SetInteractable(n.button, CanAdd(n.id) || (route.Count > 1 && n.id == Last) || inRoute);
            UIFactory.SetColor(n.button, inRoute ? UIFactory.Highlight : UIFactory.Paper);
        }

        foreach (var kv in intelButtons)
            UIFactory.SetColor(kv.Value, kv.Key == selectedIntel ? UIFactory.Highlight : UIFactory.Paper);

        var names = route.ConvertAll(id => Find(id).name);
        routeText.text = $"<b>Lộ trình</b> ({route.Count - 1}/{MaxSteps} bước)\n{string.Join(" > ", names)}"
                       + (Last == "oak" ? "" : "\n<color=#8C7B6B>Bấm các điểm nối tiếp nhau để tới Hốc cây sồi.</color>");

        UIFactory.SetInteractable(goBtn, Last == "oak" && selectedIntel != null);
    }

    void OnGo()
    {
        dm.CompleteNight(new DropRecord
        {
            day = S.day,
            intelId = selectedIntel,
            route = new List<string>(route),
        });
    }

    static Node Find(string id) => System.Array.Find(Nodes, n => n.id == id);

    static bool Connected(string a, string b) =>
        System.Array.Exists(Edges, e => (e.a == a && e.b == b) || (e.a == b && e.b == a));

    static Color RiskColor(Risk r) => UIFactory.Hex(Hex(r));

    static string Hex(Risk r)
    {
        switch (r)
        {
            case Risk.Safe:   return "7FA07A";
            case Risk.Low:    return "D6B85A";
            case Risk.Medium: return "D98A3D";
            default:          return "C0392B";
        }
    }
}
