// Xét kết thúc ở mỗi Tổng kết, theo thang ưu tiên:
// Bại lộ > Bị đuổi nhà > Đào tẩu > Được rút về > Bị bỏ rơi.
// Ba kết thúc sau chỉ xét ở Tổng kết N7, dựa trên báo cáo cuối (hồ sơ isFinal).
public static class EndingResolver
{
    public const string RookClue = "K_ROOK";
    public const string ShrikeClue = "K_SHRIKE";

    // Trả về null nếu ván chơi tiếp tục.
    public static EndingId? Resolve(GameState s, CaseSO finalCase)
    {
        if (s.SuspicionWindow >= SuspicionRules.BurnThreshold) return EndingId.Burned;
        if (s.rentWarnings > EconomyRules.MaxWarnings) return EndingId.Evicted;

        if (!s.IsLastDay) return null;

        var drop = finalCase ? s.DropFor(finalCase.id) : null;
        string accused = drop != null && drop.answers.Count > 0 ? drop.answers[0] : null;
        if (accused == RookClue) return EndingId.Defection;
        if (accused == ShrikeClue) return EndingId.Extraction;
        return EndingId.LeftInCold;
    }

    // Bằng chứng gửi kèm báo cáo cuối có chứng minh được không (đổi đoạn kết của Được rút về)
    public static bool StrongEvidence(GameState s, CaseSO finalCase)
    {
        var drop = finalCase ? s.DropFor(finalCase.id) : null;
        return drop != null && drop.answers.Count > 1 && CaseEvaluator.SlotCorrect(finalCase, 1, drop.answers[1]);
    }
}
