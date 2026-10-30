using TMPro;
using UnityEngine;

// Bảng tin buổi sáng: mỗi thông báo là một thẻ; lệnh mật mã nằm lẫn với thông báo thường.
public class BoardView : MonoBehaviour
{
    RectTransform row;

    public void Init()
    {
        row = UIFactory.MakeRect(transform, "Board_Cards", new Vector2(0.04f, 0.13f), new Vector2(0.96f, 0.78f));
        UIFactory.HList(row, 16, 0);
    }

    public void Show(DaySO day, GameState s)
    {
        UIFactory.Clear(row);
        if (!day) return;

        foreach (var m in day.board)
        {
            if (!m || !Conditions.Check(s, m.condition, m.param)) continue;

            var card = UIFactory.MakeRect(row, "Card_" + m.name, Vector2.zero, Vector2.one);
            UIFactory.MakeBox(card, UIFactory.Paper);
            UIFactory.EqualWidth(card);
            UIFactory.VList(card, 12, 18);

            var title = UIFactory.MakeText(UIFactory.MakeRect(card, "Title", Vector2.zero, Vector2.one), m.title, 24, UIFactory.Ink);
            title.fontStyle = FontStyles.Bold;
            UIFactory.MakeText(UIFactory.MakeRect(card, "Body", Vector2.zero, Vector2.one), m.body, 19, UIFactory.Ink);
            var sig = UIFactory.MakeText(UIFactory.MakeRect(card, "Signature", Vector2.zero, Vector2.one), m.signature, 17, UIFactory.Muted);
            sig.fontStyle = FontStyles.Italic;
        }
    }
}
