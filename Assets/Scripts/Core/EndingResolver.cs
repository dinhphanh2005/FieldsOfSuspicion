// Xét kết thúc ở mỗi Tổng kết, theo thang ưu tiên:
// Bại lộ > Bị đuổi nhà > Đào tẩu > Được rút về > Bị bỏ rơi.
// Ba kết thúc sau chỉ xét ở Tổng kết N7.
public static class EndingResolver
{
    // Trả về null nếu ván chơi tiếp tục.
    public static EndingId? Resolve(GameState s)
    {
        if (s.SuspicionWindow >= SuspicionRules.BurnThreshold) return EndingId.Burned;
        if (s.rentWarnings > EconomyRules.MaxWarnings) return EndingId.Evicted;

        if (!s.IsLastDay) return null;

        var drop = s.TonightDrop;
        if (drop != null && drop.mode == DeliveryMode.Truthful && drop.intelId == "B1") return EndingId.Defection;
        if (drop != null && drop.mode == DeliveryMode.Truthful && drop.intelId == "B2") return EndingId.Extraction;
        return EndingId.LeftInCold;
    }
}
