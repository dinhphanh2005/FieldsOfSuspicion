using System;
using UnityEngine;

// Sự kiện của vòng chơi. Sprint 3: TelemetryLogger đăng ký các sự kiện này để ghi CSV.
public static class GameEvents
{
    public static event Action<string> IntelCaptured;      // mã mảnh tin vừa ghi
    public static event Action<DropRecord> DropDelivered;  // đêm vừa giao tin
    public static event Action<int> DayResolved;           // ngày vừa tổng kết

    public static void RaiseIntelCaptured(string id)
    {
        Debug.Log($"[Sự kiện] IntelCaptured {id}");
        IntelCaptured?.Invoke(id);
    }

    public static void RaiseDropDelivered(DropRecord drop)
    {
        Debug.Log($"[Sự kiện] DropDelivered N{drop.day}: {drop.intelId} qua {string.Join(" > ", drop.route)}");
        DropDelivered?.Invoke(drop);
    }

    public static void RaiseDayResolved(int day)
    {
        Debug.Log($"[Sự kiện] DayResolved N{day}");
        DayResolved?.Invoke(day);
    }
}
