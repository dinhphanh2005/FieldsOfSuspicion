using System.Collections.Generic;

public static class Conditions
{
    public static bool Check(GameState s, ShowCondition condition, string param)
    {
        var drop = s.LastNightDrop;
        bool lastNightCase = drop != null && drop.caseId == param;
        int window = s.SuspicionWindow;
        switch (condition)
        {
            case ShowCondition.Always:                return true;
            case ShowCondition.DeliveredLastNight:    return lastNightCase;
            case ShowCondition.NotDeliveredLastNight: return !lastNightCase;
            case ShowCondition.RouteVisitedLastNight: return drop != null && drop.route.Contains(param);
            case ShowCondition.HasFragment:           return s.HasFragment(param);
            case ShowCondition.DeliveredBy:           return DeliveredBy(s, param);
            case ShowCondition.SuspicionLow:          return window < SuspicionRules.MidFrom;
            case ShowCondition.SuspicionMid:          return window >= SuspicionRules.MidFrom && window < SuspicionRules.HighFrom;
            case ShowCondition.SuspicionHigh:         return window >= SuspicionRules.HighFrom;
            case ShowCondition.CorrectLastNight:      return lastNightCase && drop.correct;
            case ShowCondition.WrongLastNight:        return lastNightCase && !drop.correct;
            case ShowCondition.CaseCorrect:           return s.deliveredDrops.Exists(d => d.caseId == param && d.correct);
            case ShowCondition.CaseNeverDelivered:    return !s.CaseDelivered(param);
            case ShowCondition.SaidThisVisit:         return s.SaidThisVisit.Contains(param);
            default:                                  return false;
        }
    }

    static bool DeliveredBy(GameState s, string param)
    {
        if (string.IsNullOrEmpty(param)) return false;
        var parts = param.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[1], out int lastNight)) return false;
        return s.deliveredDrops.Exists(d => d.correct && d.caseId == parts[0] && d.day <= lastNight);
    }

    public static string Pick(GameState s, string fallback, List<LineVariant> variants)
    {
        if (variants != null)
            foreach (var v in variants)
                if (Check(s, v.condition, v.param)) return v.text;
        return fallback;
    }
}
