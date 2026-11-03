using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Sáng sớm: đọc thư (nếu có) rồi đọc báo.
//   - Thư: tin có isLetter (lá thư N1) và thư nhắc tiền nhà của bà Mabry. Mỗi lá một trang giấy thư.
//   - Báo "TIN SELLOWE": 1 tin chính (priority cao nhất thỏa điều kiện) + các tin phụ, chia cột.
// Nút chính trên thanh dưới: "Đọc tiếp" / "Đọc báo" khi còn thư, "Ra bảng tin làng" ở trang báo.
public class NewsView : MonoBehaviour
{
    class Letter { public string title, note, body; }

    DayManager dm;
    RectTransform stage, letterSheet, newsSheet, letterContent, newsContent;
    Button nextBtn;
    TextMeshProUGUI hint;

    readonly List<Letter> letters = new List<Letter>();
    int page;           // 0..letters.Count-1 = thư, letters.Count = báo
    DaySO day;
    GameState state;
    string recap;
    readonly List<NewsSO> shown = new List<NewsSO>();

    public void Init(DayManager manager)
    {
        dm = manager;
        stage = UIFactory.MakeStage(transform);

        // Giấy thư: rộng 600, cao theo chữ, giữa sân khấu, hơi lệch
        letterSheet = Sheet("Letter_Sheet", 600, UIFactory.Hex("F2E9D3"), -1f);
        letterContent = letterSheet;
        UIFactory.AutoHeight(letterSheet, 52, 40, 36, 8);

        // Tờ báo: rộng 780
        newsSheet = Sheet("News_Sheet", 780, UIFactory.Hex("F1E8D2"), -0.6f);
        newsContent = newsSheet;
        UIFactory.AutoHeight(newsSheet, 44, 26, 30, 0);

        var bar = UIFactory.MakeActionBar(transform);
        hint = UIFactory.BarHint(bar, "");
        UIFactory.BarSpacer(bar);
        nextBtn = UIFactory.BarButton(bar, "", Next, primary: true);
    }

    RectTransform Sheet(string name, float width, Color color, float tilt)
    {
        var rt = UIFactory.MakeRect(stage, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, 0);
        rt.localRotation = Quaternion.Euler(0, 0, tilt);
        UIFactory.MakeSurface(rt, color, 4);
        UIFactory.Skin(rt.GetComponent<Image>(), name == "Letter_Sheet" ? "paper_letter" : "paper_newspaper");
        UIFactory.AddShadow(rt, 10);
        return rt;
    }

    // Trả về mọi thư/tin đã hiện (để DayManager xử lý grantsIntel)
    public List<NewsSO> Show(DaySO today, GameState s, string rentLetter = null, string nightRecap = "")
    {
        day = today;
        state = s;
        recap = nightRecap ?? "";
        letters.Clear();
        shown.Clear();

        if (rentLetter != null)
            letters.Add(new Letter
            {
                title = "Thư của bà Mabry",
                note = $"Thư nhắc tiền nhà, lần {s.rentWarnings}/{EconomyRules.MaxWarnings}",
                body = rentLetter,
            });

        if (day)
            foreach (var n in day.news.Where(n => n && n.isLetter && Conditions.Check(s, n.condition, n.param)))
            {
                letters.Add(new Letter { title = n.headline, note = n.letterNote, body = n.sub });
                shown.Add(n);
            }

        BuildNewspaper();
        page = 0;
        ShowPage();
        return new List<NewsSO>(shown);
    }

    void Next()
    {
        if (page < letters.Count) { page++; ShowPage(); }
        else dm.EndPhase();
    }

    void ShowPage()
    {
        bool isLetter = page < letters.Count;
        letterSheet.gameObject.SetActive(isLetter);
        newsSheet.gameObject.SetActive(!isLetter);

        if (isLetter)
        {
            BuildLetter(letters[page]);
            UIFactory.FitHeight(letterSheet, 580);
            hint.text = letters.Count > 1 ? $"Thư {page + 1}/{letters.Count}" : "Có thư nhét dưới khe cửa sáng nay";
            UIFactory.SetLabel(nextBtn, page + 1 < letters.Count ? "Đọc thư tiếp" : "Đọc báo");
        }
        else
        {
            UIFactory.FitHeight(newsSheet, 580);
            hint.text = recap;
            UIFactory.SetLabel(nextBtn, "Ra bảng tin làng");
        }
    }

    void BuildLetter(Letter l)
    {
        UIFactory.Clear(letterContent);
        UIFactory.Skin(letterSheet.GetComponent<Image>(), l.title.Contains("Mabry") ? "paper_mabry" : "paper_letter");
        UIFactory.AddLine(letterContent, l.title, 28, UIFactory.Ink, TextAlignmentOptions.TopLeft, title: true);
        if (!string.IsNullOrEmpty(l.note))
        {
            var note = UIFactory.AddLine(letterContent, l.note, 15, UIFactory.Muted);
            note.fontStyle = FontStyles.Italic;
        }
        UIFactory.Size(UIFactory.MakeRect(letterContent, "Gap", Vector2.zero, Vector2.one), 8);

        // Mỗi đoạn một dòng chữ; đoạn cuối ngắn (lời dặn, chữ ký) thì in nghiêng, canh phải
        var paras = (l.body ?? "").Split(new[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < paras.Length; i++)
        {
            bool sign = i == paras.Length - 1 && paras.Length > 2 && paras[i].Length < 40;
            var t = UIFactory.AddLine(letterContent, paras[i].Trim(), sign ? 17 : 16, UIFactory.Ink,
                                      sign ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft);
            t.lineSpacing = 10;
            if (sign) t.fontStyle = FontStyles.Italic;
        }
    }

    NewsSO BuildNewspaper()
    {
        UIFactory.Clear(newsContent);

        // Keep the masthead art and TMP label on separate GameObjects: Unity UI permits
        // only one Graphic per object, so putting Image beside TMP causes a warning and
        // leaves a null component that fails while building the newspaper.
        var mastRow = UIFactory.MakeRect(newsContent, "Masthead", Vector2.zero, Vector2.one);
        UIFactory.Size(mastRow, 72);
        var mastImage = UIFactory.MakeBox(mastRow, Color.white, false);
        mastImage.color = new Color(0, 0, 0, 0);   // ảnh news_masthead có sẵn chữ → không dùng, tránh chữ chồng hai lớp
        var mastTextRect = UIFactory.MakeRect(mastRow, "Masthead_Title", Vector2.zero, Vector2.one, 8);
        var mast = UIFactory.MakeTitle(mastTextRect, "TIN SELLOWE", 46, UIFactory.Ink, TextAlignmentOptions.Center);
        mast.characterSpacing = 18;

        // Dòng ngày tháng giữa hai đường kẻ
        UIFactory.Size(UIFactory.MakeRect(newsContent, "Gap", Vector2.zero, Vector2.one), 6);
        UIFactory.AddRule(newsContent, UIFactory.Ink, 3);
        var dl = UIFactory.MakeRect(newsContent, "Dateline", Vector2.zero, Vector2.one);
        UIFactory.Size(dl, 26);
        var datelineLeft = UIFactory.MakeRect(dl, "L", Vector2.zero, Vector2.one, 2);
        datelineLeft.anchorMax = new Vector2(0.68f, 1);
        var datelineLeftText = UIFactory.MakeText(datelineLeft, $"Số ra sáng {state.Weekday}, ngày thứ {state.day}",
                           13, UIFactory.Muted, TextAlignmentOptions.MidlineLeft);
        datelineLeftText.textWrappingMode = TextWrappingModes.NoWrap;
        datelineLeftText.overflowMode = TextOverflowModes.Ellipsis;
        var datelineRight = UIFactory.MakeRect(dl, "R", new Vector2(0.68f, 0), Vector2.one, 2);
        UIFactory.MakeText(datelineRight, "Một xu một tờ",
                           13, UIFactory.Muted, TextAlignmentOptions.MidlineRight);
        UIFactory.AddRule(newsContent, UIFactory.Ink, 1);
        UIFactory.Size(UIFactory.MakeRect(newsContent, "Gap", Vector2.zero, Vector2.one), 20);

        if (!day)
        {
            UIFactory.AddLine(newsContent, "(Chưa có nội dung cho ngày này)", 24, UIFactory.Muted, TextAlignmentOptions.Center);
            return null;
        }

        var main = day.news
            .Where(n => n && !n.isSide && !n.isLetter && Conditions.Check(state, n.condition, n.param))
            .OrderByDescending(n => n.priority)
            .FirstOrDefault();

        if (main)
        {
            shown.Add(main);
            var h = UIFactory.AddLine(newsContent, main.headline, 30, UIFactory.Ink, TextAlignmentOptions.Center, title: true);
            h.lineSpacing = -8;
            if (!string.IsNullOrEmpty(main.sub))
            {
                UIFactory.Size(UIFactory.MakeRect(newsContent, "Gap", Vector2.zero, Vector2.one), 10);
                var lead = UIFactory.AddLine(newsContent, main.sub, 18, UIFactory.Ink, TextAlignmentOptions.Top);
                lead.lineSpacing = 8;
                lead.margin = new Vector4(30, 0, 30, 0);
            }
        }

        // Tin phụ: chia cột đều, ngăn bằng đường kẻ dọc
        var side = day.news.Where(n => n && n.isSide && !n.isLetter && Conditions.Check(state, n.condition, n.param))
                           .OrderByDescending(n => n.priority).Take(3).ToList();   // tối đa 3 cột cho dễ đọc
        shown.AddRange(side);
        if (side.Count > 0)
        {
            UIFactory.Size(UIFactory.MakeRect(newsContent, "Gap", Vector2.zero, Vector2.one), 20);
            UIFactory.AddRule(newsContent, UIFactory.Ink, 1);
            UIFactory.Size(UIFactory.MakeRect(newsContent, "Gap", Vector2.zero, Vector2.one), 14);

            var cols = UIFactory.MakeRect(newsContent, "Columns", Vector2.zero, Vector2.one);
            var hl = UIFactory.HList(cols, 0, 0);
            hl.childForceExpandWidth = false;   // đường kẻ dọc giữ đúng 1px
            for (int i = 0; i < side.Count; i++)
            {
                if (i > 0)
                {
                    var vr = UIFactory.MakeRect(cols, "VRule", Vector2.zero, Vector2.one);
                    var le = vr.gameObject.AddComponent<LayoutElement>();
                    le.minWidth = le.preferredWidth = 1;
                    le.flexibleWidth = 0;
                    UIFactory.MakeBox(vr, new Color(0.23f, 0.18f, 0.15f, 0.35f), false);
                }
                var col = UIFactory.MakeRect(cols, "Col", Vector2.zero, Vector2.one);
                UIFactory.EqualWidth(col);
                var v = UIFactory.VList(col, 6, 0);
                v.padding = new RectOffset(18, 18, 0, 0);
                UIFactory.AddLine(col, side[i].headline, 18, UIFactory.Ink, TextAlignmentOptions.TopLeft, title: true);
                if (!string.IsNullOrEmpty(side[i].sub))
                    UIFactory.AddLine(col, side[i].sub, 15, UIFactory.Ink).lineSpacing = 6;
            }
        }
        return main;
    }
}
