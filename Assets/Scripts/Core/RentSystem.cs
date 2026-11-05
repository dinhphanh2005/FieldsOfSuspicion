public static class RentSystem
{
    // Thiếu tiền không trừ âm mà tính một lần nhắc; quá MaxWarnings thì EndingResolver cho bị đuổi nhà.
    public static bool Settle(GameState state)
    {
        if (state.money >= EconomyRules.Rent)
        {
            state.money -= EconomyRules.Rent;
            return true;
        }
        state.rentWarnings++;
        state.lastRentMissDay = state.day;
        return false;
    }
}
