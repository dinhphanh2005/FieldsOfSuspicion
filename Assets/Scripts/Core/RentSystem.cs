public static class RentSystem
{
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
