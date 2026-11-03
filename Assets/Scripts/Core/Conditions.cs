using System.Collections.Generic;

// Kiểm tra điều kiện hiện nội dung (tin tức, thẻ bảng tin, biến thể câu thoại).
public static class Conditions
{
    public static bool Check(GameState s, ShowCondition condition, string param)
    {
        var drop = s.LastNightDrop;   // null nếu đêm qua ở nhà
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
            default:                                  return false;
        }
    }

    // param "C3:4" → đã gửi hồ sơ C3 ĐÚNG vào đêm N4 hoặc sớm hơn
    static bool DeliveredBy(GameState s, string param)
    {
        if (string.IsNullOrEmpty(param)) return false;
        var parts = param.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[1], out int lastNight)) return false;
        return s.deliveredDrops.Exists(d => d.correct && d.caseId == parts[0] && d.day <= lastNight);
    }

    // Lấy biến thể ĐẦU TIÊN thỏa điều kiện; không có thì dùng câu mặc định.
    public static string Pick(GameState s, string fallback, List<LineVariant> variants)
    {
        if (variants != null)
            foreach (var v in variants)
                if (Check(s, v.condition, v.param)) return v.text;
        return fallback;
    }
}
