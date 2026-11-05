using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NightView : MonoBehaviour
{
    class Node { public string id, name; public Vector2 pos; }
    class Edge { public string a, b; public RouteRisk baseRisk; }

    class EdgeState
    {
        public Edge edge;
        public RouteRisk actual, shown;
        public bool closed;
        public Image line;
    }

    const int MaxSteps = 4;

    static readonly Node[] Nodes =
    {
        new Node { id = "home",   name = "Nhà",            pos = new Vector2(110, 330) },
        new Node { id = "well",   name = "Giếng làng",     pos = new Vector2(260, 200) },
        new Node { id = "market", name = "Chợ",            pos = new Vector2(440, 330) },
        new Node { id = "church", name = "Nhà thờ",        pos = new Vector2(450, 140) },
        new Node { id = "bridge", name = "Cầu Bắc",        pos = new Vector2(690, 150) },
        new Node { id = "river",  name = "Đường ven sông", pos = new Vector2(340, 500) },
        new Node { id = "oak",    name = "Hốc cây sồi",    pos = new Vector2(700, 400) },
    };

    static readonly Edge[] Edges =
    {
        new Edge { a = "home",   b = "river",  baseRisk = RouteRisk.Safe },
        new Edge { a = "river",  b = "oak",    baseRisk = RouteRisk.Safe },
        new Edge { a = "home",   b = "well",   baseRisk = RouteRisk.Low },
        new Edge { a = "well",   b = "market", baseRisk = RouteRisk.Medium },
        new Edge { a = "market", b = "oak",    baseRisk = RouteRisk.Medium },
        new Edge { a = "well",   b = "church", baseRisk = RouteRisk.Safe },
        new Edge { a = "market", b = "church", baseRisk = RouteRisk.Low },
        new Edge { a = "church", b = "bridge", baseRisk = RouteRisk.Safe },
        new Edge { a = "bridge", b = "oak",    baseRisk = RouteRisk.Low },
    };

    DayManager dm;
    TextMeshProUGUI mapSub, routeTitle, routeText, notesText, barHint;
    RectTransform side, intelList;
    ScrollRect intelScroll;
    Button goBtn;

    readonly List<EdgeState> edges = new List<EdgeState>();
    readonly Dictionary<string, Button> nodeButtons = new Dictionary<string, Button>();
    readonly List<string> route = new List<string>();
    readonly Dictionary<string, Button> reportButtons = new Dictionary<string, Button>();
    string selectedCase;

    GameState S => dm.State;
    string Last => route[route.Count - 1];

    public void Init(DayManager manager)
    {
        dm = manager;
        var stage = UIFactory.MakeStage(transform);
        BuildMap(stage);
        BuildSidePanel(stage);

        var bar = UIFactory.MakeActionBar(transform);
        UIFactory.BarButton(bar, "Sổ tay", () => dm.OpenNotebook(true));
        UIFactory.BarButton(bar, "Làm lại lộ trình", ResetRoute);
        UIFactory.BarButton(bar, "Ở nhà tối nay", () => dm.CompleteNight(null));
        barHint = UIFactory.BarHint(bar, "");
        UIFactory.BarSpacer(bar);
        goBtn = UIFactory.BarButton(bar, "Đi giao tin", OnGo, primary: true);
    }

    void BuildMap(RectTransform stage)
    {
        var card = UIFactory.Place(stage, "Night_MapCard", 24, 16, 812, 576);
        UIFactory.MakeSurface(card, UIFactory.Hex("E6D7B8"), 8);
        UIFactory.Skin(card.GetComponent<Image>(), "map_night");
        UIFactory.AddShadow(card, 10);
        UIFactory.AddGrain(card, 0.12f);

        var head = UIFactory.Place(card, "Head", 0, 0, 812, 46);
        UIFactory.MakeBox(head, new Color(0.1f, 0.08f, 0.07f, 0.82f), false);
        var mapTitleRect = UIFactory.MakeRect(head, "Title", Vector2.zero, Vector2.one, 0);
        mapTitleRect.anchorMax = new Vector2(0.4f, 1);
        var mapTitle = UIFactory.MakeTitle(mapTitleRect, "Sellowe về đêm", 20, UIFactory.Gold,
                                           TextAlignmentOptions.MidlineLeft);
        mapTitle.margin = new Vector4(22, 0, 0, 0);
        mapSub = UIFactory.MakeText(UIFactory.MakeRect(head, "Sub", new Vector2(0.4f, 0), Vector2.one, 0), "", 14,
                                    UIFactory.Hex("CDBFA8"), TextAlignmentOptions.MidlineRight);
        mapSub.margin = new Vector4(0, 0, 22, 0);
        mapSub.enableAutoSizing = true;
        mapSub.fontSizeMin = 10;
        mapSub.fontSizeMax = 14;
        mapSub.textWrappingMode = TextWrappingModes.NoWrap;
        mapSub.overflowMode = TextOverflowModes.Ellipsis;

        foreach (var e in Edges)
        {
            Vector2 a = FindNode(e.a).pos, b = FindNode(e.b).pos;
            var line = UIFactory.Place(card, $"Edge_{e.a}_{e.b}", 0, 0, Vector2.Distance(a, b), 7);
            line.pivot = new Vector2(0.5f, 0.5f);
            line.anchoredPosition = new Vector2((a.x + b.x) / 2, -(a.y + b.y) / 2);
            line.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(-(b.y - a.y), b.x - a.x) * Mathf.Rad2Deg);
            edges.Add(new EdgeState { edge = e, line = UIFactory.MakeBox(line, Color.white, false) });
        }

        foreach (var n in Nodes)
        {
            var rt = UIFactory.Place(card, "Node_" + n.id, 0, 0, 148, 42);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(n.pos.x, -n.pos.y);
            string id = n.id;
            var b = UIFactory.MakeButton(rt, "Button", n.name, () => OnNode(id), 16);
            UIFactory.Skin((Image)b.targetGraphic, n.id == "home" ? "node_plate_home" : "node_plate_normal");
            string nodeIcon = n.id == "home" ? "icon_home" : n.id == "well" ? "icon_bell" : n.id == "market" ? "icon_coins" :
                              n.id == "church" ? "icon_calendar" : n.id == "bridge" ? "icon_moon" : n.id == "river" ? "icon_lantern" : "icon_oak";
            var nodeIconRt = UIFactory.MakeRect(b.transform, "LocationIcon", new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            nodeIconRt.pivot = new Vector2(0, 0.5f);
            nodeIconRt.sizeDelta = new Vector2(22, 22);
            nodeIconRt.anchoredPosition = new Vector2(7, 0);
            UIFactory.Skin(UIFactory.MakeBox(nodeIconRt, Color.white, false), nodeIcon);
            var nodeLabel = b.GetComponentInChildren<TextMeshProUGUI>();
            nodeLabel.rectTransform.offsetMin = new Vector2(33, 6);
            nodeLabel.rectTransform.offsetMax = new Vector2(-10, -6);
            var nodeFeel = b.GetComponent<ButtonFeel>();
            if (nodeFeel) nodeFeel.content = null;
            nodeLabel.textWrappingMode = TextWrappingModes.NoWrap;
            nodeLabel.enableAutoSizing = true;
            nodeLabel.fontSizeMin = 12;
            nodeLabel.fontSizeMax = 16;
            UIFactory.AddOutline(b.targetGraphic, n.id == "oak" ? UIFactory.Red : new Color(0.23f, 0.18f, 0.15f, 0.35f));
            UIFactory.AddShadow(b, 2);
            nodeButtons[id] = b;
        }

        var legend = UIFactory.Place(card, "Legend", 0, 536, 812, 40);
        UIFactory.MakeBox(legend, new Color(0.1f, 0.08f, 0.07f, 0.82f), false);
        var items = UIFactory.MakeRect(legend, "Items", Vector2.zero, Vector2.one);
        var h = UIFactory.HList(items, 8, 0);
        h.padding = new RectOffset(22, 22, 0, 0);
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlHeight = false;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        LegendText(items, "Rủi ro:");
        LegendItem(items, "risk_safe", "an toàn");
        LegendItem(items, "risk_low", "thấp");
        LegendItem(items, "risk_mid", "vừa");
        LegendItem(items, "risk_high", "cao");
        LegendItem(items, "risk_unknown", "chưa rõ");
        LegendItem(items, "closed", "đường đóng");
    }

    static void LegendItem(RectTransform parent, string spriteName, string label)
    {
        var bar = UIFactory.MakeRect(parent, "Swatch", Vector2.zero, Vector2.one);
        bool closed = spriteName == "closed";
        bar.sizeDelta = new Vector2(22, closed ? 5 : 22);
        var le = bar.gameObject.AddComponent<LayoutElement>();
        le.minWidth = le.preferredWidth = 22;
        var swatch = UIFactory.MakeBox(bar, closed ? UIFactory.Hex(ClosedHex) : Color.white, false);
        if (!closed) UIFactory.Skin(swatch, spriteName);
        LegendText(parent, label + "   ");
    }

    static void LegendText(RectTransform parent, string text)
    {
        var rt = UIFactory.MakeRect(parent, "Label", Vector2.zero, Vector2.one);
        rt.sizeDelta = new Vector2(0, 40);
        var t = UIFactory.MakeText(rt, text, 14, UIFactory.Light, TextAlignmentOptions.MidlineLeft);
        t.textWrappingMode = TextWrappingModes.NoWrap;
    }

    void BuildSidePanel(RectTransform stage)
    {
        side = UIFactory.Place(stage, "Night_Side", 852, 16, 404, 576);
        UIFactory.MakeSurface(side, UIFactory.NightBox, 8);
        UIFactory.Skin(side.GetComponent<Image>(), "panel_night");
        UIFactory.AddShadow(side, 10);
        var v = UIFactory.VList(side, 4, 0);
        v.padding = new RectOffset(20, 20, 18, 18);

        UIFactory.MakeLabel(UIFactory.MakeRect(side, "NotesLabel", Vector2.zero, Vector2.one), "ĐÊM NAY", UIFactory.Gold);
        notesText = UIFactory.AddLine(side, "", 14.5f, UIFactory.Hex("E8C77A"));
        notesText.lineSpacing = 6;
        Gap(14);

        routeTitle = UIFactory.MakeLabel(UIFactory.MakeRect(side, "RouteLabel", Vector2.zero, Vector2.one), "", UIFactory.Gold);
        routeText = UIFactory.AddLine(side, "", 16, UIFactory.Light);
        routeText.lineSpacing = 6;
        Gap(14);

        UIFactory.MakeLabel(UIFactory.MakeRect(side, "IntelLabel", Vector2.zero, Vector2.one), "BÁO CÁO GỬI ĐÊM NAY", UIFactory.Gold);
        Gap(4);
        var listRoot = UIFactory.MakeRect(side, "IntelList", Vector2.zero, Vector2.one);
        UIFactory.Flexible(listRoot, 90);
        intelList = UIFactory.MakeScroll(listRoot, out intelScroll);
        intelList.GetComponent<VerticalLayoutGroup>().spacing = 7;
    }

    void Gap(float h) => UIFactory.Size(UIFactory.MakeRect(side, "Gap", Vector2.zero, Vector2.one), h);

    public void Begin()
    {
        ApplyDayRules();
        selectedCase = null;
        route.Clear();
        route.Add("home");
        Refresh();
    }

    void ApplyDayRules()
    {
        var notes = new List<string>();
        var rules = dm.Today ? dm.Today.night : null;

        foreach (var es in edges)
        {
            es.actual = es.edge.baseRisk;
            es.shown = es.edge.baseRisk;
            es.closed = false;

            var rule = rules?.Find(r => Same(r.a, r.b, es.edge.a, es.edge.b));
            if (rule != null)
            {
                bool known = string.IsNullOrEmpty(rule.hiddenUntil) || S.HasFragment(rule.hiddenUntil);
                es.actual = rule.risk;
                es.closed = rule.closed;
                if (known)
                {
                    es.shown = rule.risk;
                    if (!string.IsNullOrEmpty(rule.note)) notes.Add("• " + rule.note);
                }
            }

            es.line.color = es.closed ? UIFactory.Hex(ClosedHex) : UIFactory.Hex(RiskHex(es.shown));
            es.line.rectTransform.sizeDelta = new Vector2(es.line.rectTransform.sizeDelta.x, es.closed ? 3 : 7);
        }

        notesText.text = notes.Count > 0 ? string.Join("\n", notes) : "<color=#9C8C7B>Không nghe nói có gì bất thường.</color>";
    }

    void BuildReportList()
    {
        UIFactory.Clear(intelList);
        reportButtons.Clear();
        int shown = 0;

        foreach (var c in dm.AllCases)
        {
            if (!CaseEvaluator.IsOpen(c, S) || S.CaseDelivered(c.id) || !CaseEvaluator.InWindow(c, S.day)) continue;
            var answers = S.Draft(c).answers;
            bool complete = CaseEvaluator.IsComplete(c, answers);
            string id = c.id;
            string body = CaseEvaluator.Sentence(c, answers, dm.FindIntel, "<b>{0}</b>", "<color=#A89A88>[{0}?]</color>");
            string deadline = c.lastNight == S.day ? "  <color=#B8442E>hạn chót đêm nay</color>" : "";
            var b = UIFactory.MakeButton(intelList, "Report_" + id,
                $"<size=80%><color=#7A6A5A>{c.title.ToUpper()}</color>{deadline}</size>\n{body}",
                () => SelectReport(id, complete), 15);
            UIFactory.Round((Image)b.targetGraphic, 6);
            if (c.isFinal) UIFactory.AddButtonIcon(b, "icon_seal_straka");
            var t = b.GetComponentInChildren<TextMeshProUGUI>();
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.enableAutoSizing = false;
            t.fontSize = 14;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.rectTransform.offsetMin = new Vector2(14, 8);
            t.rectTransform.offsetMax = new Vector2(-10, -8);
            float textWidth = ReportListWidth - 24 - t.margin.x - t.margin.z;
            float textHeight = t.GetPreferredValues(t.text, textWidth, 0).y;
            UIFactory.Size(b, Mathf.Max(64, textHeight + 20));
            if (!complete) t.alpha = 0.75f;
            reportButtons[id] = b;
            shown++;
        }

        if (shown == 0)
            UIFactory.MakeText(UIFactory.MakeRect(intelList, "Empty", Vector2.zero, Vector2.one),
                "Đêm nay không có hồ sơ nào cần gửi. Có thể ở nhà — hoặc vẫn ra hốc cây sồi mà không gửi gì.",
                15, UIFactory.Hex("9C8C7B"));
        else
            UIFactory.MakeText(UIFactory.MakeRect(intelList, "Tip", Vector2.zero, Vector2.one),
                "Bản báo cáo còn thiếu ô thì mở Sổ tay, trang Hồ sơ, để ghép tiếp.", 13, UIFactory.Hex("9C8C7B"));
        LayoutRebuilder.ForceRebuildLayoutImmediate(intelList);
        intelScroll.verticalNormalizedPosition = 1;
    }

    const float ReportListWidth = 364;

    void SelectReport(string id, bool complete)
    {
        if (!complete) { dm.OpenNotebook(true, id); return; }
        selectedCase = selectedCase == id ? null : id;
        RefreshMapAndPanel();
    }

    void ResetRoute()
    {
        route.Clear();
        route.Add("home");
        RefreshMapAndPanel();
    }

    void OnNode(string id)
    {
        if (route.Count > 1 && id == Last) route.RemoveAt(route.Count - 1);
        else if (CanAdd(id)) route.Add(id);
        RefreshMapAndPanel();
    }

    int StepLimit => MaxSteps;
    int FreeSteps => S.energy / EconomyRules.StepEnergy;
    int OverSteps => Mathf.Max(0, route.Count - 1 - FreeSteps);

    bool CanAdd(string id) =>
        Last != "oak" && route.Count - 1 < StepLimit && !route.Contains(id) && OpenEdge(Last, id) != null;

    public void Refresh()
    {
        BuildReportList();
        if (selectedCase != null && !reportButtons.ContainsKey(selectedCase)) selectedCase = null;
        if (selectedCase != null && !CaseEvaluator.IsComplete(dm.FindCase(selectedCase), S.Draft(dm.FindCase(selectedCase)).answers))
            selectedCase = null;
        RefreshMapAndPanel();
    }

    void RefreshMapAndPanel()
    {
        foreach (var n in Nodes)
        {
            var b = nodeButtons[n.id];
            bool inRoute = route.Contains(n.id);
            UIFactory.SetInteractable(b, inRoute || CanAdd(n.id));
            UIFactory.Skin((Image)b.targetGraphic, inRoute ? "node_plate_route" : !CanAdd(n.id) && n.id != "home" ? "node_plate_disabled" : n.id == "home" ? "node_plate_home" : "node_plate_normal");
            UIFactory.SetColor(b, Color.white);
        }

        foreach (var kv in reportButtons)
            UIFactory.SetColor(kv.Value, kv.Key == selectedCase ? UIFactory.Hex("F6DFA0") : UIFactory.Paper);

        int steps = route.Count - 1;
        mapSub.text = FreeSteps >= StepLimit
            ? $"Từ Nhà tới Hốc cây sồi · tối đa {StepLimit} bước (còn {S.energy} năng lượng)"
            : $"Còn {S.energy} năng lượng: từ bước thứ {FreeSteps + 1} là <color=#E0B04B>đi quá sức, mỗi bước +{EconomyRules.OverexertSuspicion} nghi ngờ</color>";
        routeTitle.text = $"LỘ TRÌNH · {steps}/{StepLimit} BƯỚC";
        var names = route.ConvertAll(id => FindNode(id).name);
        routeText.text = $"<color=#E0B04B>{string.Join("  ›  ", names)}</color>\n<size=85%><color=#9C8C7B>" +
            (Last == "oak"
                ? $"Đã tới hốc cây sồi. Đi hết {Mathf.Min(steps, FreeSteps) * EconomyRules.StepEnergy} năng lượng." +
                  (OverSteps > 0 ? $" <color=#E0B04B>Đi quá sức {OverSteps} bước: +{OverSteps * EconomyRules.OverexertSuspicion} nghi ngờ.</color>" : "")
                : $"Bấm các điểm nối tiếp nhau. Mỗi bước −{EconomyRules.StepEnergy} năng lượng; bấm lại điểm cuối để lùi.") +
            "</color></size>";

        bool ready = Last == "oak";
        UIFactory.SetInteractable(goBtn, ready);
        UIFactory.SetLabel(goBtn, selectedCase != null ? "Đi giao báo cáo" : "Ra hốc cây sồi");
        barHint.text = !ready ? "Bấm các điểm trên bản đồ để đi tới Hốc cây sồi"
                     : selectedCase == null ? "Chưa chọn báo cáo: ra hốc cây sồi mà không gửi gì"
                     : "";

        LayoutRebuilder.ForceRebuildLayoutImmediate(side);
    }

    void OnGo()
    {
        string invalidReason = RoutePlanner.Validate(route, StepLimit, IsOpenEdge);
        if (invalidReason != null) { barHint.text = invalidReason; return; }
        foreach (var es in RouteEdges())
            S.AddSuspicion(es.actual == RouteRisk.High ? SuspicionChannel.Emel : SuspicionChannel.Patrol,
                           SuspicionRules.Points(es.actual), $"đi qua {FindNode(es.edge.a).name} – {FindNode(es.edge.b).name}");
        if (OverSteps > 0)
            S.AddSuspicion(SuspicionChannel.Patrol, OverSteps * EconomyRules.OverexertSuspicion, $"đi quá sức {OverSteps} bước");
        S.energy = Mathf.Max(0, S.energy - (route.Count - 1) * EconomyRules.StepEnergy);

        var c = dm.FindCase(selectedCase);
        GameAudio.Sfx("drop");
        dm.CompleteNight(new DropRecord
        {
            day = S.day,
            caseId = c ? c.id : "",
            answers = c ? new List<string>(S.Draft(c).answers) : new List<string>(),
            route = new List<string>(route),
        });
    }

    IEnumerable<EdgeState> RouteEdges()
    {
        for (int i = 1; i < route.Count; i++) yield return OpenEdge(route[i - 1], route[i]);
    }

    EdgeState OpenEdge(string a, string b) =>
        edges.Find(es => !es.closed && Same(es.edge.a, es.edge.b, a, b));

    bool IsOpenEdge(string a, string b) => OpenEdge(a, b) != null;

    static bool Same(string a1, string b1, string a2, string b2) =>
        (a1 == a2 && b1 == b2) || (a1 == b2 && b1 == a2);

    static Node FindNode(string id) => System.Array.Find(Nodes, n => n.id == id);

    const string ClosedHex = "A39787";

    static string RiskHex(RouteRisk r)
    {
        switch (r)
        {
            case RouteRisk.Safe:   return "4F7F4A";
            case RouteRisk.Low:    return "B8901F";
            case RouteRisk.Medium: return "C96524";
            default:               return "A82C1F";
        }
    }
}
