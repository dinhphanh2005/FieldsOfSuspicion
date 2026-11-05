using System.Text.RegularExpressions;

public static class IntelMarkup
{
    const string FreshColor    = "B8442E";
    const string CapturedColor = "B3AAA0";
    static readonly Regex Pattern = new Regex(@"\{\{(.+?)\|(\w+)\}\}");

    public static string ToRichText(string raw, GameState s)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        return Pattern.Replace(raw, m =>
        {
            string phrase = m.Groups[1].Value;
            string id = m.Groups[2].Value;
            return s.HasFragment(id)
                ? $"<color=#{CapturedColor}>{phrase}</color>"
                : $"<link=\"{id}\"><color=#{FreshColor}><u>{phrase}</u></color></link>";
        });
    }
}
