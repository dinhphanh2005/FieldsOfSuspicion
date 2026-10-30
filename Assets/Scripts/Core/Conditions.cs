using System.Collections.Generic;

// Kiểm tra điều kiện hiện nội dung (tin tức, thẻ bảng tin, biến thể câu thoại).
public static class Conditions
{
    public static bool Check(GameState s, ShowCondition condition, string param)
    {
        var drop = s.LastNightDrop;   // null nếu đêm qua ở nhà
        switch (condition)
        {
            case ShowCondition.Always:                return true;
            case ShowCondition.DeliveredLastNight:    return drop != null && drop.intelId == param;
            case ShowCondition.NotDeliveredLastNight: return drop == null || drop.intelId != param;
            case ShowCondition.RouteVisitedLastNight: return drop != null && drop.route.Contains(param);
            default:                                  return false;
        }
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
