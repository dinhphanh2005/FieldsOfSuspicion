using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingView : MonoBehaviour
{
    TextMeshProUGUI title, body, stats;
    RectTransform card;

    public void Init()
    {
        var stage = UIFactory.MakeStage(transform);

        card = UIFactory.MakeRect(stage, "Ending_Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        card.sizeDelta = new Vector2(900, 0);
        UIFactory.MakeSurface(card, UIFactory.Hex("F2E9D3"), 10);
        UIFactory.Skin(card.GetComponent<UnityEngine.UI.Image>(), "panel_ending");
        UIFactory.AddShadow(card, 10);
        UIFactory.AutoHeight(card, 60, 44, 40, 0);

        title = UIFactory.AddLine(card, "", 38, UIFactory.Red, TextAlignmentOptions.Center, title: true);
        title.characterSpacing = 4;
        UIFactory.Size(UIFactory.MakeRect(card, "Gap", Vector2.zero, Vector2.one), 14);
        UIFactory.AddRule(card, UIFactory.PaperEdge, 1);
        UIFactory.Size(UIFactory.MakeRect(card, "Gap", Vector2.zero, Vector2.one), 18);
        body = UIFactory.AddLine(card, "", 19, UIFactory.Ink);
        body.lineSpacing = 10;
        UIFactory.Size(UIFactory.MakeRect(card, "Gap", Vector2.zero, Vector2.one), 22);
        UIFactory.AddRule(card, UIFactory.PaperEdge, 1);
        UIFactory.Size(UIFactory.MakeRect(card, "Gap", Vector2.zero, Vector2.one), 12);
        stats = UIFactory.AddLine(card, "", 15, UIFactory.Hex("5C4D40"));
        stats.lineSpacing = 6;

        var bar = UIFactory.MakeActionBar(transform);
        UIFactory.BarHint(bar, "Một ván chỉ một tuần. Thử lại với những lựa chọn khác để thấy kết thúc khác.");
        UIFactory.BarSpacer(bar);
        UIFactory.BarButton(bar, "Chơi lại", Replay, primary: true);
    }

    public void Show(EndingSO ending, GameState s, DayManager dm)
    {
        title.text = ending ? ending.title.ToUpper() : "KẾT THÚC";
        string text = ending ? ending.text : "(Thiếu EndingSO cho kết thúc này)";
        if (ending && ending.id == EndingId.Extraction)
        {
            string extra = EndingResolver.StrongEvidence(s, dm.FinalCase) ? ending.strongEvidence : ending.weakEvidence;
            if (!string.IsNullOrEmpty(extra)) text += "\n\n" + extra;
        }
        body.text = text;

        var sb = new StringBuilder();
        sb.Append("<b>HỒ SƠ NHIỆM VỤ</b>  <color=#8C7B6B><size=90%>— " + s.playerName + ", mật danh Wren</size></color>\n");
        foreach (var c in dm.AllCases)
        {
            var drop = s.DropFor(c.id);
            string verdict = drop == null ? "<color=#8C7B6B>không gửi</color>"
                           : drop.correct ? "<color=#4F7F4A>ĐÚNG</color>"
                           : c.isFinal && drop.answers.Count > 0 && CaseEvaluator.SlotCorrect(c, 0, drop.answers[0])
                               ? "<color=#A8741F>ĐÚNG NGƯỜI · bằng chứng yếu</color>"
                           : "<color=#A82C1F>SAI</color>";
            sb.Append($"• {c.title}: {verdict}");
            if (drop != null) sb.Append("  <size=85%><color=#8C7B6B>« " + StripTags(CaseEvaluator.Sentence(c, drop.answers, dm.FindIntel, "{0}", "…")) + " »</color></size>");
            sb.Append('\n');
        }

        int rookAll = s.handlerOrderResults.Count(r => r.handler == Handler.Rook);
        int rookDone = s.handlerOrderResults.Count(r => r.handler == Handler.Rook && r.followed);
        int shrikeAll = s.handlerOrderResults.Count(r => r.handler == Handler.Shrike);
        int shrikeDone = s.handlerOrderResults.Count(r => r.handler == Handler.Shrike && r.followed);
        sb.Append($"Làm theo lệnh Rook: {rookDone}/{rookAll} · lệnh Shrike: {shrikeDone}/{shrikeAll}\n");
        sb.Append($"Manh mối đã ghi: {s.fragments.Count} · Tiền còn: {s.money} xu · Kết thúc ở đêm {GameState.WeekdayOf(s.day).ToLower()}\n");
        sb.Append($"<color=#8C7B6B><size=90%>Nghi ngờ 3 đêm cuối: {s.SuspicionWindow}/{SuspicionRules.BurnThreshold} · cộng dồn cả ván: {s.SuspicionTotal}</size></color>");
        stats.text = sb.ToString();
        UIFactory.FitHeight(card, 590);
    }

    static string StripTags(string s) => System.Text.RegularExpressions.Regex.Replace(s ?? "", "<.*?>", "");

    void Replay() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
