using System;

[Serializable]
public class GameState
{
    public const int LastDay = 7;
    public int day = 1;   // N1 = Sunday.
    public int money = 20; 
    public bool IsLastDay => day >= LastDay;
    
    static readonly string[] DayNames =
        { "Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };
    
    public string DayLabel => $"N{day} — {DayNames[Math.Clamp(day, 1, LastDay) - 1]}";
    
    public void NextDay()
    {
        day++;
    }
}