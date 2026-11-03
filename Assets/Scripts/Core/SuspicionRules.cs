// Các con số của hệ thống nghi ngờ (đóng góp NCKH: cửa sổ trượt, nhiều kênh).
// Số khởi điểm theo mục 8 PDF; chỉnh qua playtest.
public static class SuspicionRules
{
    public const int Low = 1;
    public const int Medium = 2;
    public const int High = 4;

    public const int WindowNights = 3;     // chỉ tính 3 đêm gần nhất
    public const int BurnThreshold = 12;   // ngưỡng gợi ý trong kịch bản; cửa sổ 3 đêm

    public const int MidFrom = 4;          // nhãn: Thấp < 4, Vừa 4–7, Cao >= 8 (Emel đổi câu theo nhãn này)
    public const int HighFrom = 8;

    // Trọng số mỗi kênh trong tổng. Để 1 = cộng thường; chỉnh khi playtest (ví dụ Emel nặng hơn).
    public static float Weight(SuspicionChannel c)
    {
        switch (c)
        {
            case SuspicionChannel.Emel:   return 1f;
            case SuspicionChannel.Perrin: return 1f;
            case SuspicionChannel.Sable:  return 1f;
            default:                      return 1f;   // Patrol
        }
    }

    public static string ChannelName(SuspicionChannel c)
    {
        switch (c)
        {
            case SuspicionChannel.Emel:   return "Emel / đồn Lenmar";
            case SuspicionChannel.Perrin: return "Perrin / nhà Vaule";
            case SuspicionChannel.Sable:  return "Sable";
            default:                      return "Lính tuần, dân làng";
        }
    }

    public static string Tier(int window) =>
        window >= HighFrom ? "Cao" : window >= MidFrom ? "Vừa" : "Thấp";

    public static int Points(RouteRisk r)
    {
        switch (r)
        {
            case RouteRisk.Low:    return Low;
            case RouteRisk.Medium: return Medium;
            case RouteRisk.High:   return High;
            default:               return 0;
        }
    }
}
