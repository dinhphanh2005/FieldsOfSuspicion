using System.Collections.Generic;

// Kiểm tra điều kiện hiện nội dung (tin tức, thẻ bảng tin, biến thể câu thoại).
public static class Conditions
{
    public static bool Check(GameState s, ShowCondition condition, string param)
    {
        var drop = s.LastNightDrop;   // null nếu đêm qua ở nhà
        int window = s.SuspicionWindow;
        switch (condition)
        {
            case ShowCondition.Always:                return true;
            case ShowCondition.DeliveredLastNight:    return drop != null && drop.mode == DeliveryMode.Truthful && drop.intelId == param;
            case ShowCondition.NotDeliveredLastNight: return drop == null || drop.mode != DeliveryMode.Truthful || drop.intelId != param;
            case ShowCondition.RouteVisitedLastNight: return drop != null && drop.route.Contains(param);
            case ShowCondition.HasFragment:           return s.HasFragment(param);
            case ShowCondition.DeliveredBy:           return DeliveredBy(s, param);
            case ShowCondition.SuspicionLow:          return window < SuspicionRules.MidFrom;
            case ShowCondition.SuspicionMid:          return window >= SuspicionRules.MidFrom && window < SuspicionRules.HighFrom;
            case ShowCondition.SuspicionHigh:         return window >= SuspicionRules.HighFrom;
            default:                                  return false;
        }
    }

    // param "F07:4" → đã giao F07 vào đêm N4 hoặc sớm hơn
    static bool DeliveredBy(GameState s, string param)
    {
        if (string.IsNullOrEmpty(param)) return false;
        var parts = param.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[1], out int lastNight)) return false;
        return s.deliveredDrops.Exists(d => d.mode == DeliveryMode.Truthful && d.intelId == parts[0] && d.day <= lastNight);
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
