public static class EndingResolver
{
    public const string RookClue = "K_ROOK";
    public const string ShrikeClue = "K_SHRIKE";

    // Bại lộ và bị đuổi nhà xét mỗi đêm; chỉ đêm cuối mới xét người bị tố trong hồ sơ cuối.
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

    // Tố đúng người nhưng ô bằng chứng sai vẫn được rút về, chỉ là kết thúc yếu.
    public static bool StrongEvidence(GameState s, CaseSO finalCase)
    {
        var drop = finalCase ? s.DropFor(finalCase.id) : null;
        return drop != null && drop.answers.Count > 1 && CaseEvaluator.SlotCorrect(finalCase, 1, drop.answers[1]);
    }
}
