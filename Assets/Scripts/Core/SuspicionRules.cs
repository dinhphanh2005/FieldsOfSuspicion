public static class SuspicionRules
{
    public const int Low = 1;
    public const int Medium = 2;
    public const int High = 4;

    public const int WindowNights = 3;
    public const int BurnThreshold = 12;

    public const int MidFrom = 4;
    public const int HighFrom = 8;

    public static float Weight(SuspicionChannel c)
    {
        switch (c)
        {
            case SuspicionChannel.Emel:   return 1f;
            case SuspicionChannel.Perrin: return 1f;
            case SuspicionChannel.Sable:  return 1f;
            default:                      return 1f;
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
