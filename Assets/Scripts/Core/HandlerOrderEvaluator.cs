using System;
using System.Linq;

public static class HandlerOrderEvaluator
{
    public static void ResolveDay(GameState state, DaySO today, DaySO[] allDays, Func<string, IntelSO> findClue)
    {
        if (today == null || today.board == null) return;
        var drop = state.TonightDrop;
        string sent = drop != null ? drop.caseId : null;
        string accused = drop != null && drop.answers.Count > 0 ? drop.answers[0] : null;

        if (state.day == GameState.LastDay)
        {
            var yesterday = allDays == null ? null : allDays.FirstOrDefault(d => d && d.day == state.day - 1);
            if (yesterday != null)
                foreach (var message in yesterday.board.Where(m => m && m.handler != Handler.None && state.WasMessageShown(m.name)))
                {
                    string answer = Normalize(message.answer);
                    if (answer == "MAI BAO TEN")
                        AddResult(state, message, state.day - 1, sent == "C4", "Báo cáo cuối đã được gửi đêm thứ Bảy.");
                    else if (answer == "ROOK PHAN")
                        AddResult(state, message, state.day - 1, sent == "C4" && accused == EndingResolver.RookClue, "Shrike muốn Rook bị tố.");
                }
        }

        foreach (var message in today.board)
        {
            if (!message || message.handler == Handler.None || !state.WasMessageShown(message.name)) continue;
            string answer = Normalize(message.answer);
            if (answer == "MAI BAO TEN" || answer == "ROOK PHAN") continue;
            bool followed = answer switch
            {
                "MO TIEM"    => state.shopOpenedToday,
                "LANG NGHE"  => state.capturedToday.Count > 0,
                "GIAO XE"    => sent == "C1",
                "GIAO KHO"   => sent == "C2",
                "THEO VAULE" => state.capturedToday.Any(id => { var c = findClue?.Invoke(id); return c && c.thread == IntelThread.Vaule; }),
                "QUA CHO"    => drop != null && drop.route.Contains("market"),
                "GIAO VAULE" => sent == "C3",
                "BO VAULE"   => sent != "C3",
                "GIU YEN"    => drop == null,
                "QUA CAU"    => drop != null && drop.route.Contains("bridge"),
                "DEM CUOI"   => sent == "C4",
                "BAO ROOK"   => sent == "C4" && accused == EndingResolver.RookClue,
                _ => false
            };
            AddResult(state, message, state.day, followed, "Đối chiếu với hành động trong ngày.");
        }
    }

    static string Normalize(string value) => (value ?? "").Trim().ToUpperInvariant();

    static void AddResult(GameState state, HandlerMessageSO message, int day, bool followed, string note)
    {
        if (state.handlerOrderResults.Exists(r => r.messageId == message.name)) return;
        state.handlerOrderResults.Add(new HandlerOrderResult
        {
            day = day,
            messageId = message.name,
            handler = message.handler,
            instruction = message.answer,
            followed = followed,
            outcome = note
        });
        GameEvents.RaiseHandlerOrderResolved(message.name, message.handler, followed);
    }
}
