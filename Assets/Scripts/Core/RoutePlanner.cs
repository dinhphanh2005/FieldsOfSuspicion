using System;
using System.Collections.Generic;

public static class RoutePlanner
{
    public static string Validate(IReadOnlyList<string> route, int maxSteps, Func<string, string, bool> edgeIsOpen)
    {
        if (route == null || route.Count < 2) return "Lộ trình chưa được chọn.";
        if (route[0] != "home") return "Lộ trình phải bắt đầu ở nhà.";
        if (route[route.Count - 1] != "oak") return "Lộ trình phải kết thúc ở hốc cây sồi.";
        if (route.Count - 1 > maxSteps) return "Lộ trình vượt quá số bước cho phép.";
        var visited = new HashSet<string>();
        for (int i = 0; i < route.Count; i++)
        {
            if (!visited.Add(route[i])) return "Không thể đi lại qua một địa điểm.";
            if (i > 0 && !edgeIsOpen(route[i - 1], route[i])) return "Có đoạn đường không nối hoặc đã bị đóng.";
        }
        return null;
    }
}
