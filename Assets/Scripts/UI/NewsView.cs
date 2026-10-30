using System.Linq;
using TMPro;
using UnityEngine;

// Màn Tin tức: 1 tin chính (priority cao nhất thỏa điều kiện) + mọi tin phụ thỏa điều kiện.
public class NewsView : MonoBehaviour
{
    TextMeshProUGUI headline, sub, side;

    public void Init()
    {
        headline = UIFactory.MakeText(UIFactory.MakeRect(transform, "News_Headline", new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.76f)),
                                      "", 40, UIFactory.Ink, TextAlignmentOptions.Bottom);
        headline.fontStyle = FontStyles.Bold;
        sub = UIFactory.MakeText(UIFactory.MakeRect(transform, "News_Sub", new Vector2(0.15f, 0.3f), new Vector2(0.85f, 0.45f)),
                                 "", 24, UIFactory.Ink, TextAlignmentOptions.Top);
        side = UIFactory.MakeText(UIFactory.MakeRect(transform, "News_Side", new Vector2(0.15f, 0.13f), new Vector2(0.85f, 0.28f)),
                                  "", 20, UIFactory.Muted, TextAlignmentOptions.Top);
        side.fontStyle = FontStyles.Italic;
    }

    public void Show(DaySO day, GameState s)
    {
        if (!day)
        {
            headline.text = "(Chưa có nội dung cho ngày này)";
            sub.text = "";
            side.text = "";
            return;
        }

        var main = day.news
            .Where(n => n && !n.isSide && Conditions.Check(s, n.condition, n.param))
            .OrderByDescending(n => n.priority)
            .FirstOrDefault();

        headline.text = main ? main.headline : "";
        sub.text = main ? main.sub : "";
        side.text = string.Join("\n", day.news
            .Where(n => n && n.isSide && Conditions.Check(s, n.condition, n.param))
            .Select(n => n.headline));
    }
}
