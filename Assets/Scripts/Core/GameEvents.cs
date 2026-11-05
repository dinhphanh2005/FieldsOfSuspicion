using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<string> IntelCaptured;
    public static event Action<DropRecord> DropDelivered;
    public static event Action<int, string> SuspicionAdded;
    public static event Action<int> DayResolved;
    public static event Action<EndingId> EndingReached;
    public static event Action<string, Handler, bool> HandlerOrderResolved;

    public static void RaiseIntelCaptured(string id)
    {
        Debug.Log($"[Sự kiện] IntelCaptured {id}");
        IntelCaptured?.Invoke(id);
    }

    public static void RaiseDropDelivered(DropRecord drop)
    {
        Debug.Log($"[Sự kiện] DropDelivered N{drop.day}: {(drop.HasReport ? drop.caseId + (drop.correct ? " (đúng)" : " (sai)") : "không gửi gì")} qua {string.Join(" > ", drop.route)}");
        DropDelivered?.Invoke(drop);
    }

    public static void RaiseSuspicionAdded(int points, string reason)
    {
        Debug.Log($"[Nghi ngờ] +{points} ({reason})");
        SuspicionAdded?.Invoke(points, reason);
    }

    public static void RaiseDayResolved(int day)
    {
        Debug.Log($"[Sự kiện] DayResolved N{day}");
        DayResolved?.Invoke(day);
    }

    public static void RaiseEndingReached(EndingId id)
    {
        Debug.Log($"[Sự kiện] EndingReached {id}");
        EndingReached?.Invoke(id);
    }

    public static void RaiseHandlerOrderResolved(string messageId, Handler handler, bool followed)
    {
        Debug.Log($"[Lệnh handler] {messageId} ({handler}): {(followed ? "đã làm theo" : "không làm theo")}");
        HandlerOrderResolved?.Invoke(messageId, handler, followed);
    }
}
