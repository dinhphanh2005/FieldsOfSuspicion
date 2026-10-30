// Tên mạch tin hiện cho người chơi.
public static class IntelThreadNames
{
    public static string Of(IntelThread t)
    {
        switch (t)
        {
            case IntelThread.Convoy:  return "Đoàn xe";
            case IntelThread.Granary: return "Kho thóc";
            case IntelThread.Vaule:   return "Nhà Vaule";
            case IntelThread.Route:   return "Lộ trình";
            case IntelThread.Traitor: return "Ai là kẻ phản bội?";
            case IntelThread.Report:  return "Báo cáo";
            default:                  return t.ToString();
        }
    }
}
