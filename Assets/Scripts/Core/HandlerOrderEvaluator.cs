using System;
using System.Linq;

// Giải quyết lệnh bằng hành động người chơi, không yêu cầu nhập lời giải Null Cipher.
public static class HandlerOrderEvaluator
{
    public static void ResolveDay(GameState state, DaySO today, DaySO[] allDays)
    {
        if (today == null || today.board == null) return;
        var drop = state.TonightDrop;

        // Lệnh N6 yêu cầu báo tên vào đêm N7 nên chỉ được chấm khi có báo cáo cuối ván.
        if (state.day == 7)
        {
            var yesterday = allDays == null ? null : allDays.FirstOrDefault(d => d && d.day == 6);
            if (yesterday != null)
                foreach (var message in yesterday.board.Where(m => m && m.handler != Handler.None))
                {
                    string answer = Normalize(message.answer);
                    if (answer == "MAI BAO TEN")
                        AddResult(state, message, state.day - 1, IsTruthful(drop) && (drop.intelId == "B1" || drop.intelId == "B2"), "Báo cáo được kiểm tra vào đêm cuối.");
                    else if (answer == "ROOK PHAN")
                        AddResult(state, message, state.day - 1, IsTruthful(drop) && drop.intelId == "B1", "Shrike yêu cầu báo cáo buộc tội Rook.");
                }
        }

        foreach (var message in today.board)
        {
            if (!message || message.handler == Handler.None || !state.WasMessageShown(message.name)) continue;
            string answer = Normalize(message.answer);
            if (answer == "MAI BAO TEN" || (state.day == 6 && answer == "ROOK PHAN")) continue;
            bool followed = answer switch
            {
                "MO TIEM" => state.shopOpenedToday,
                "LANG NGHE" => state.capturedToday.Count > 0,
                "GIAO XE" => IsTruthful(drop) && drop.intelId == "F02",
                "GIAO KHO" => IsTruthful(drop) && drop.intelId == "F03",
                "THEO VAULE" => state.capturedToday.Any(IsVauleClue),
                "QUA CHO" => drop != null && drop.route.Contains("market"),
                "GIAO VAULE" => IsTruthful(drop) && drop.intelId == "F07",
                "BO VAULE" => drop == null || !IsVauleClue(drop.intelId),
                "GIU YEN" => drop == null,
                "QUA CAU" => drop != null && drop.route.Contains("bridge"),
                "DEM CUOI" => IsTruthful(drop) && (drop.intelId == "B1" || drop.intelId == "B2"),
                "BAO ROOK" => IsTruthful(drop) && drop.intelId == "B1",
                _ => false
            };
            AddResult(state, message, state.day, followed, "Đối chiếu với hành động trong ngày.");
        }
    }

    static bool IsVauleClue(string id) => id == "F05" || id == "F06" || id == "F07";
    static bool IsTruthful(DropRecord drop) => drop != null && drop.mode == DeliveryMode.Truthful;
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
