public static class IntelThreadNames
{
    public static string Of(IntelThread t)
    {
        switch (t)
        {
            case IntelThread.Convoy:  return "Đoàn xe quân nhu";
            case IntelThread.Granary: return "Kho thóc phía Tây";
            case IntelThread.Vaule:   return "Nhà Vaule";
            case IntelThread.Route:   return "Đường đi đêm";
            case IntelThread.Traitor: return "Kẻ phản bội";
            case IntelThread.Village: return "Chuyện trong làng";
            default:                  return t.ToString();
        }
    }

    public static string Of(ClueKind k)
    {
        switch (k)
        {
            case ClueKind.Who:      return "Ai";
            case ClueKind.What:     return "Cái gì";
            case ClueKind.Where:    return "Ở đâu";
            case ClueKind.When:     return "Khi nào";
            case ClueKind.State:    return "Tình hình";
            case ClueKind.Evidence: return "Bằng chứng";
            default:                return "Chi tiết";
        }
    }
}
