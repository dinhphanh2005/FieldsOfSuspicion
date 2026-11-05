using System.Collections.Generic;
using System.Linq;

public static class CaseEvaluator
{
    public static bool IsComplete(CaseSO c, IReadOnlyList<string> answers)
    {
        if (!c || answers == null || answers.Count < c.slots.Count) return false;
        for (int i = 0; i < c.slots.Count; i++)
            if (string.IsNullOrEmpty(answers[i])) return false;
        return true;
    }

    public static bool SlotCorrect(CaseSO c, int slot, string answer) =>
        c && slot >= 0 && slot < c.slots.Count && !string.IsNullOrEmpty(answer) && c.slots[slot].correct.Contains(answer);

    // Hồ sơ đúng khi mọi ô đều điền và mỗi ô khớp một manh mối trong danh sách đáp án.
    public static bool IsCorrect(CaseSO c, IReadOnlyList<string> answers) =>
        IsComplete(c, answers) && Enumerable.Range(0, c.slots.Count).All(i => SlotCorrect(c, i, answers[i]));

    public static bool IsOpen(CaseSO c, GameState s)
    {
        if (!c) return false;
        if (string.IsNullOrWhiteSpace(c.opensWith)) return true;
        return c.opensWith.Split(',').Any(id => s.HasFragment(id.Trim()));
    }

    public static bool InWindow(CaseSO c, int day) => c && day >= c.firstNight && day <= c.lastNight;

    public static string Sentence(CaseSO c, IReadOnlyList<string> answers, System.Func<string, IntelSO> find,
                                  string filledFormat = "<b>{0}</b>", string emptyFormat = "<color=#9C8C7B>[{0}]</color>")
    {
        if (!c) return "";
        var parts = new object[c.slots.Count];
        for (int i = 0; i < c.slots.Count; i++)
        {
            string a = answers != null && i < answers.Count ? answers[i] : "";
            var clue = string.IsNullOrEmpty(a) ? null : find(a);
            parts[i] = clue ? string.Format(filledFormat, clue.keyword) : string.Format(emptyFormat, c.slots[i].label.ToLower());
        }
        try { return string.Format(c.template ?? "", parts); }
        catch (System.FormatException) { return c.template; }
    }
}
