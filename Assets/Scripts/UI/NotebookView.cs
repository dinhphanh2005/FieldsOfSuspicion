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
        content = UIFactory.MakeText(UIFactory.MakeRect(transform, "Notebook_Content", new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.76f)),
                                     "", 24, UIFactory.Light);
    }

    public void Refresh()
    {
        var s = dm.State;
        var sb = new StringBuilder();

        foreach (IntelThread thread in Enum.GetValues(typeof(IntelThread)))
        {
            var items = s.fragments.Select(dm.FindIntel).Where(i => i && i.thread == thread).ToList();
            if (items.Count == 0) continue;

            sb.Append($"<b>{IntelThreadNames.Of(thread)}</b>\n");
            foreach (var i in items)
            {
                bool delivered = s.deliveredDrops.Exists(d => d.intelId == i.id);
                sb.Append($"- {i.text} <size=70%><color=#B3AAA0>({i.source}{(delivered ? ", đã giao" : "")})</color></size>\n");
            }
            sb.Append('\n');
        }

        content.text = sb.Length > 0 ? sb.ToString() : "Sổ tay còn trống. Bấm vào cụm từ có màu trong lời khách để ghi tin.";
    }
}
