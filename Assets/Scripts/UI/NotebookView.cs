using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotebookView : MonoBehaviour
{
    DayManager dm;
    RectTransform page;
    RectTransform cluesRoot, casesRoot, caseList, caseDetail;
    ScrollRect detailScroll;
    TextMeshProUGUI cluesText, subText, barHint;
    Button tabClues, tabCases;

    bool showingCases;
    string selectedCase;
    int activeSlot;

    GameState S => dm.State;

    public void Init(DayManager manager)
    {
        dm = manager;
        var stage = UIFactory.MakeStage(transform);

        page = UIFactory.Place(stage, "Notebook_Page", 90, 12, 1100, 584);
        UIFactory.MakeSurface(page, UIFactory.Hex("F3EAD6"), 8);
        UIFactory.Skin(page.GetComponent<Image>(), "notebook_page");
        UIFactory.AddShadow(page, 10);
        var tape = UIFactory.Place(page, "Tape", 940, -10, 118, 40);
        UIFactory.Skin(UIFactory.MakeBox(tape, new Color(1, 1, 1, 0.9f), false), "tape");

        UIFactory.MakeTitle(UIFactory.Place(page, "Title", 96, 18, 560, 38), "Sổ tay của Tomas", 26, UIFactory.Ink);
        subText = UIFactory.MakeText(UIFactory.Place(page, "Sub", 96, 54, 600, 22), "", 14, UIFactory.Muted);
        subText.fontStyle = FontStyles.Italic;

        tabClues = Tab(page, "Tab_Clues", 712, 22, () => SwitchTab(false));
        tabCases = Tab(page, "Tab_Cases", 896, 22, () => SwitchTab(true));
        UIFactory.MakeBox(UIFactory.Place(page, "TabLine", 96, 70, 964, 1), new Color(0.23f, 0.18f, 0.15f, 0.25f), false);

        cluesRoot = UIFactory.Place(page, "Clues", 96, 82, 964, 486);
        var cl = UIFactory.MakeScroll(UIFactory.MakeRect(cluesRoot, "Scroll", Vector2.zero, Vector2.one), out _);
        cluesText = UIFactory.AddLine(cl, "", 17, UIFactory.Ink);
        cluesText.lineSpacing = 6;
        cluesText.paragraphSpacing = 4;

        casesRoot = UIFactory.Place(page, "Cases", 96, 82, 964, 486);
        var listRoot = UIFactory.Place(casesRoot, "CaseList", 0, 0, 286, 486);
        caseList = UIFactory.MakeScroll(listRoot, out _);
        caseList.GetComponent<VerticalLayoutGroup>().spacing = 8;
        UIFactory.MakeBox(UIFactory.Place(casesRoot, "Divider", 302, 0, 1, 486), new Color(0.23f, 0.18f, 0.15f, 0.25f), false);
        var detailRoot = UIFactory.Place(casesRoot, "CaseDetail", 318, 0, 646, 486);
        caseDetail = UIFactory.MakeScroll(detailRoot, out detailScroll);
        caseDetail.GetComponent<VerticalLayoutGroup>().spacing = 8;

        var bar = UIFactory.MakeActionBar(transform);
        barHint = UIFactory.BarHint(bar, "");
        UIFactory.BarSpacer(bar);
        UIFactory.BarButton(bar, "Gấp sổ lại", () => dm.CloseNotebook(), primary: true);
    }

    Button Tab(RectTransform parent, string name, float x, float y, UnityEngine.Events.UnityAction onClick)
    {
        var b = UIFactory.MakeButton(UIFactory.Place(parent, name, x, y, 172, 38), name, "", onClick, 16);
        return b;
    }

    public void Open(bool casesTab, string caseId)
    {
        if (!string.IsNullOrEmpty(caseId)) selectedCase = caseId;
        SwitchTab(casesTab);
    }

    void SwitchTab(bool cases)
    {
        if (cases != showingCases) GameAudio.Sfx("page", 0.6f);
        showingCases = cases;
        cluesRoot.gameObject.SetActive(!cases);
        casesRoot.gameObject.SetActive(cases);
        Refresh();
    }

    public void Refresh()
    {
        var openCases = OpenCases();
        UIFactory.SetLabel(tabClues, $"Manh mối · {S.fragments.Count}");
        UIFactory.SetLabel(tabCases, $"Hồ sơ · {openCases.Count}");
        UIFactory.SetColor(tabClues, showingCases ? UIFactory.Paper : UIFactory.Gold);
        UIFactory.SetColor(tabCases, showingCases ? UIFactory.Gold : UIFactory.Paper);

        if (showingCases)
        {
            subText.text = "Ghép manh mối vào chỗ trống. Sổ tay không cho biết ghép đúng hay sai.";
            BuildCaseList(openCases);
            BuildCaseDetail();
            barHint.text = dm.CurrentPhase == Phase.Night
                ? "Ghép xong thì gấp sổ lại, chọn bản báo cáo ở bảng bên phải bản đồ."
                : "Mỗi đêm chỉ gửi được một bản báo cáo. Hồ sơ nào cũng có hạn chót.";
        }
        else
        {
            subText.text = "Mẩu nào cũng nhỏ. Ghép lại mới thành tin.";
            BuildClues();
            barHint.text = "Gạch chân đỏ trong lời khách = có thể ghi vào sổ";
        }
    }

    List<CaseSO> OpenCases() => dm.AllCases.Where(c => CaseEvaluator.IsOpen(c, S)).ToList();

    void BuildClues()
    {
        var sb = new StringBuilder();
        foreach (IntelThread thread in Enum.GetValues(typeof(IntelThread)))
        {
            var items = S.fragments.Select(dm.FindIntel).Where(i => i && i.thread == thread).ToList();
            if (items.Count == 0) continue;

            sb.Append($"<size=112%><b>{IntelThreadNames.Of(thread)}</b></size>\n");
            foreach (var i in items)
            {
                string fresh = S.capturedToday.Contains(i.id) ? "  <color=#B8442E><size=75%>MỚI</size></color>" : "";
                sb.Append($"•  <b>{i.keyword}</b>  <size=75%><color=#9C8C7B>[{IntelThreadNames.Of(i.kind)}] · {i.source}</color></size>{fresh}\n");
                sb.Append($"<indent=24><size=88%><color=#5C4D40>{i.text}</color></size></indent>\n");
            }
            sb.Append('\n');
        }

        cluesText.text = sb.Length > 0 ? sb.ToString()
            : "<color=#8C7B6B>Sổ tay còn trống. Buổi chiều, khi khách nói chuyện, bấm vào cụm từ gạch chân màu đỏ để ghi lại.</color>";
    }

    string Status(CaseSO c)
    {
        var drop = S.DropFor(c.id);
        if (drop != null) return $"Đã gửi đêm {GameState.WeekdayOf(drop.day).ToLower()}";
        if (S.day > c.lastNight) return "Quá hạn — tin đã cũ";
        bool complete = CaseEvaluator.IsComplete(c, S.Draft(c).answers);
        if (S.day < c.firstNight) return complete ? $"Đã ghép xong · gửi từ đêm {GameState.WeekdayOf(c.firstNight).ToLower()}" : $"Gửi từ đêm {GameState.WeekdayOf(c.firstNight).ToLower()}";
        int filled = S.Draft(c).answers.Count(a => !string.IsNullOrEmpty(a));
        string deadline = c.lastNight == S.day ? "hạn chót đêm nay" : $"hạn chót đêm {GameState.WeekdayOf(c.lastNight).ToLower()}";
        return complete ? $"Sẵn sàng gửi · {deadline}" : $"Đang ghép {filled}/{c.slots.Count} · {deadline}";
    }

    void BuildCaseList(List<CaseSO> openCases)
    {
        UIFactory.Clear(caseList);
        if (openCases.Count == 0)
        {
            UIFactory.AddLine(caseList, "Chưa có hồ sơ nào. Hồ sơ mở ra khi cậu nghe được chuyện đáng báo cáo.", 15, UIFactory.Muted);
            return;
        }
        if (string.IsNullOrEmpty(selectedCase) || !openCases.Exists(c => c.id == selectedCase))
            selectedCase = (openCases.FirstOrDefault(c => !S.CaseDelivered(c.id) && S.day <= c.lastNight) ?? openCases[0]).id;

        foreach (var c in openCases)
        {
            string id = c.id;
            bool selected = id == selectedCase;
            bool closed = S.CaseDelivered(id) || S.day > c.lastNight;
            var b = UIFactory.MakeButton(caseList, "Case_" + id,
                $"<b>{c.title}</b>\n<size=78%><color=#7A6A5A>{Status(c)}</color></size>",
                () => { selectedCase = id; activeSlot = 0; Refresh(); }, 16);
            UIFactory.Size(b, 62);
            var t = b.GetComponentInChildren<TextMeshProUGUI>();
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.rectTransform.offsetMin = new Vector2(14, 4);
            UIFactory.SetColor(b, selected ? UIFactory.Hex("F6DFA0") : closed ? UIFactory.Hex("D9CFBD") : Color.white);
        }

        int locked = dm.AllCases.Length - openCases.Count;
        if (locked > 0)
            UIFactory.AddLine(caseList, $"<i>Còn {locked} hồ sơ chưa mở.</i>", 13, UIFactory.Muted);
    }

    void BuildCaseDetail()
    {
        UIFactory.Clear(caseDetail);
        var c = dm.FindCase(selectedCase);
        if (!c || !CaseEvaluator.IsOpen(c, S)) return;

        var draft = S.Draft(c);
        var drop = S.DropFor(c.id);
        bool locked = drop != null || S.day > c.lastNight;
        var answers = drop != null ? drop.answers : draft.answers;
        if (activeSlot >= c.slots.Count) activeSlot = 0;

        UIFactory.AddLine(caseDetail, c.title, 24, UIFactory.Ink, TextAlignmentOptions.TopLeft, title: true);
        var brief = UIFactory.AddLine(caseDetail, c.brief, 15, UIFactory.Muted);
        brief.fontStyle = FontStyles.Italic;
        UIFactory.AddLine(caseDetail, Status(c), 14, UIFactory.Hex("8C3A2B"));

        var box = UIFactory.MakeRect(caseDetail, "Sentence", Vector2.zero, Vector2.one);
        UIFactory.MakeSurface(box, UIFactory.Hex("EADFC6"), 6);
        UIFactory.AutoHeight(box, 18, 14, 14, 0);
        var sentence = UIFactory.AddLine(box,
            "<size=75%><color=#8C7B6B>BẢN BÁO CÁO</color></size>\n" +
            CaseEvaluator.Sentence(c, answers, dm.FindIntel, "<b><color=#8C3A2B>{0}</color></b>", "<color=#A89A88>[{0}?]</color>"),
            20, UIFactory.Ink);
        sentence.lineSpacing = 6;

        if (locked)
        {
            string note = drop != null
                ? $"Bản báo cáo này đã nằm trong hốc cây sồi từ đêm {GameState.WeekdayOf(drop.day).ToLower()}. Kết quả ra sao, hãy đọc Tin tức."
                : "Hồ sơ đã quá hạn: chuyện đã xảy ra, tin bây giờ không còn giá trị.";
            UIFactory.AddLine(caseDetail, note, 15, UIFactory.Muted).fontStyle = FontStyles.Italic;
            return;
        }

        for (int i = 0; i < c.slots.Count; i++)
        {
            int slot = i;
            var row = UIFactory.MakeRect(caseDetail, "Slot_" + i, Vector2.zero, Vector2.one);
            UIFactory.Size(row, 40);
            var label = UIFactory.MakeText(UIFactory.Place(row, "Label", 0, 0, 130, 40), c.slots[i].label, 16, UIFactory.Ink, TextAlignmentOptions.MidlineLeft);
            label.fontStyle = FontStyles.Bold;
            var clue = dm.FindIntel(answers[i]);
            var b = UIFactory.MakeButton(UIFactory.Place(row, "Pick", 120, 2, 500, 36), "Pick",
                clue ? clue.keyword : "— bấm để chọn manh mối —", () => { activeSlot = slot; Refresh(); }, 16);
            UIFactory.SetColor(b, slot == activeSlot ? UIFactory.Hex("F6DFA0") : Color.white);
        }

        var kind = c.slots[activeSlot].kind;
        var options = S.fragments.Select(dm.FindIntel).Where(x => x && x.kind == kind).ToList();
        UIFactory.AddLine(caseDetail, $"<b>Ô \"{c.slots[activeSlot].label}\"</b> — chọn một manh mối loại <i>{IntelThreadNames.Of(kind)}</i>:", 15, UIFactory.Ink);
        if (options.Count == 0)
        {
            UIFactory.AddLine(caseDetail, "Cậu chưa ghi được manh mối nào loại này. Hãy hỏi chuyện khách ở tiệm, hoặc đọc kỹ Tin tức.", 15, UIFactory.Muted)
                     .fontStyle = FontStyles.Italic;
        }
        else
        {
            var gridRt = UIFactory.MakeRect(caseDetail, "Chips", Vector2.zero, Vector2.one);
            var grid = gridRt.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(204, 40);
            grid.spacing = new Vector2(8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            foreach (var o in options)
            {
                string id = o.id;
                bool chosen = answers[activeSlot] == id;
                var chip = UIFactory.MakeButton(gridRt, "Chip_" + id, o.keyword, () => Choose(c, id), 15);
                var t = chip.GetComponentInChildren<TextMeshProUGUI>();
                t.enableAutoSizing = true; t.fontSizeMin = 11; t.fontSizeMax = 15;
                UIFactory.SetColor(chip, chosen ? UIFactory.Gold : UIFactory.Hex("F4ECD8"));
            }
            var clear = UIFactory.MakeButton(gridRt, "Chip_Clear", "<i>để trống</i>", () => Choose(c, ""), 14);
            UIFactory.SetColor(clear, UIFactory.Hex("DCCBA8"));
        }

        UIFactory.AddLine(caseDetail,
            "<size=90%><color=#8C7B6B>Mẹo: các nguồn tin có thể mâu thuẫn nhau. Ai nói? Nói lúc nào? Người đó có lý do gì để nói dối không?</color></size>",
            14, UIFactory.Muted);
    }

    void Choose(CaseSO c, string clueId)
    {
        S.SetAnswer(c, activeSlot, clueId);
        var answers = S.Draft(c).answers;
        if (!string.IsNullOrEmpty(clueId))
            for (int k = 1; k <= c.slots.Count; k++)
            {
                int next = (activeSlot + k) % c.slots.Count;
                if (string.IsNullOrEmpty(answers[next])) { activeSlot = next; break; }
            }
        Refresh();
    }
}
