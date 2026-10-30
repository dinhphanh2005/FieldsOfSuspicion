using System;
using System.Collections.Generic;

[Serializable]
public class GameState
{
    public const int LastDay = 7;
    public int day = 1;   // N1 = Sunday.
    public int money = 20; 
    public bool IsLastDay => day >= LastDay;

    // Mã các mảnh tin đã ghi vào sổ tay ("F02"...).
    // Lưu mã (string) chứ không lưu IntelSO, để JsonUtility lưu game được.
    public List<string> fragments = new();

    // Các đêm đã đi giao tin (đêm ở nhà thì không có bản ghi).
    public List<DropRecord> deliveredDrops = new();
    
    static readonly string[] DayNames =
        { "Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };
    
    public string DayLabel => $"N{day} — {DayNames[Math.Clamp(day, 1, LastDay) - 1]}";
    
    public void NextDay()
    {
        day++;
    }

    public bool HasFragment(string id) => fragments.Contains(id);

    // Trả về true nếu là mảnh tin MỚI (để UI biết vừa ghi được tin).
    public bool AddFragment(string id)
    {
        if (HasFragment(id)) return false;
        fragments.Add(id);
        return true;
    }

    // Bản ghi giao tin của đêm hôm qua; null nếu đêm qua ở nhà.
    public DropRecord LastNightDrop => deliveredDrops.Find(d => d.day == day - 1);
}
