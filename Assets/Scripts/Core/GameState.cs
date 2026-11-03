using System;
using System.Collections.Generic;
using UnityEngine;

// Toàn bộ trạng thái của một ván. Class thuần [Serializable] để JsonUtility lưu được:
// chỉ chứa số, chuỗi, list — tham chiếu tới nội dung (ScriptableObject) lưu bằng mã id.
[Serializable]
public class GameState
{
    public const int LastDay = 7;
    public int day = 1;   // N1 = Chủ Nhật.
    public bool IsLastDay => day >= LastDay;

    // ---- Kinh tế ----
    public int money = EconomyRules.StartMoney;
    public int stock = EconomyRules.StartStock;     // bó cà rốt trong kho
    public int energy = EconomyRules.MaxEnergy;
    public int actionPoints = EconomyRules.MaxActionPoints;
    public ShopPriceLevel shopPriceLevel = ShopPriceLevel.Fair;
    public int customerBaseMomentum;
    public int salesToday;
    public bool shopOpenedToday;
    public int growthProcessedDay;
    public string playerName = "Wren";
    public List<string> capturedToday = new();
    public int rentWarnings;                         // số lần đã thiếu tiền nhà (không reset)
    public int lastRentMissDay = -1;                 // đêm gần nhất thiếu tiền nhà
    public int lastRentPaidDay = -1;                 // đêm gần nhất trả đủ tiền nhà (để báo lại buổi sáng)

    // ---- Sổ tay ----
    // Mã các manh mối đã ghi ("K02"...). Lưu mã chứ không lưu IntelSO, để JsonUtility lưu game được.
    public List<string> fragments = new();
    // Bản nháp báo cáo người chơi đang ghép, mỗi hồ sơ một bản.
    public List<ReportDraft> drafts = new();
    public List<string> shownMessageIds = new();
    public List<FarmPlotState> farmPlots = FarmSystem.CreateStartingPlots();

    // Các đêm đã ra hốc cây sồi (đêm ở nhà thì không có bản ghi).
    public List<DropRecord> deliveredDrops = new();
    public List<HandlerOrderResult> handlerOrderResults = new();

    // ---- Nghi ngờ (cửa sổ trượt, nhiều kênh) ----
    // Tổng có trọng số của từng đêm đã tổng kết: phần tử i = đêm N(i+1).
    public List<int> nightlySuspicion = new();
    // Điểm từng kênh của từng đêm (để xem chi tiết và ghi CSV).
    public List<NightSuspicion> nightlyByChannel = new();
    // Điểm cộng trong ngày đang chơi, theo kênh; chốt ở Tổng kết.
    public int[] pendingByChannel = new int[4];

    public string ending;   // tên EndingId khi ván kết thúc
    public float burnProbability = -1f; // -1 khi chưa cấu hình trọng số mô hình offline

    static readonly string[] DayNames =
        { "Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };

    public string Weekday => DayNames[Math.Clamp(day, 1, LastDay) - 1];
    public static string WeekdayOf(int d) => DayNames[Math.Clamp(d, 1, LastDay) - 1];
    public string DayLabel => $"N{day} — {Weekday}";

    public void NextDay()
    {
        day++;
        energy = EconomyRules.MaxEnergy;
        actionPoints = EconomyRules.MaxActionPoints;
        shopOpenedToday = false;
        salesToday = 0;
        capturedToday.Clear();
    }

    // ---- Manh mối ----
    public bool HasFragment(string id) => !string.IsNullOrEmpty(id) && fragments.Contains(id);

    // Trả về true nếu là manh mối MỚI (để UI biết vừa ghi được).
    public bool AddFragment(string id)
    {
        if (string.IsNullOrEmpty(id) || HasFragment(id)) return false;
        fragments.Add(id);
        return true;
    }

    // ---- Bản nháp báo cáo ----
    public ReportDraft Draft(CaseSO c)
    {
        var d = drafts.Find(x => x.caseId == c.id);
        if (d == null) drafts.Add(d = new ReportDraft { caseId = c.id });
        while (d.answers.Count < c.slots.Count) d.answers.Add("");
        return d;
    }

    public void SetAnswer(CaseSO c, int slot, string clueId)
    {
        if (CaseDelivered(c.id)) return;   // đã gửi thì không sửa được
        var d = Draft(c);
        if (slot >= 0 && slot < d.answers.Count) d.answers[slot] = clueId ?? "";
    }

    public bool CaseDelivered(string caseId) => deliveredDrops.Exists(d => d.caseId == caseId);
    public DropRecord DropFor(string caseId) => deliveredDrops.Find(d => d.caseId == caseId);

    public bool WasMessageShown(string id) => shownMessageIds.Contains(id);

    public void MarkMessageShown(string id)
    {
        if (!string.IsNullOrEmpty(id) && !shownMessageIds.Contains(id)) shownMessageIds.Add(id);
    }

    // Bản ghi của đêm hôm qua; null nếu đêm qua ở nhà.
    public DropRecord LastNightDrop => deliveredDrops.Find(d => d.day == day - 1);

    // Bản ghi của đêm hôm nay (dùng ở Tổng kết).
    public DropRecord TonightDrop => deliveredDrops.Find(d => d.day == day);

    // ---- Nghi ngờ ----
    public void AddSuspicion(SuspicionChannel channel, int points, string reason)
    {
        if (points <= 0) return;
        pendingByChannel[(int)channel] += points;
        GameEvents.RaiseSuspicionAdded(points, $"{reason} > {SuspicionRules.ChannelName(channel)}");
    }

    public int PendingTotal
    {
        get
        {
            float total = 0;
            for (int c = 0; c < pendingByChannel.Length; c++)
                total += pendingByChannel[c] * SuspicionRules.Weight((SuspicionChannel)c);
            return Mathf.RoundToInt(total);
        }
    }

    // Gọi ở Tổng kết: chốt điểm của đêm nay.
    public void CloseNight()
    {
        nightlySuspicion.Add(PendingTotal);
        nightlyByChannel.Add(new NightSuspicion { day = day, channels = (int[])pendingByChannel.Clone() });
        pendingByChannel = new int[pendingByChannel.Length];
    }

    // Tổng 3 đêm gần nhất đã chốt — con số dùng để xét Bại lộ và đổi câu thoại.
    public int SuspicionWindow => SumLast(nightlySuspicion, SuspicionRules.WindowNights);

    // Cửa sổ 3 đêm của riêng một kênh (để xem chi tiết)
    public int ChannelWindow(SuspicionChannel c)
    {
        int sum = 0;
        for (int i = Mathf.Max(0, nightlyByChannel.Count - SuspicionRules.WindowNights); i < nightlyByChannel.Count; i++)
            sum += nightlyByChannel[i].channels[(int)c];
        return sum;
    }

    // Tổng mọi đêm (cộng dồn thường) — chỉ để so sánh trong phần NCKH, không dùng để xét Bại lộ.
    public int SuspicionTotal => SumLast(nightlySuspicion, nightlySuspicion.Count);

    static int SumLast(List<int> list, int n)
    {
        int sum = 0;
        for (int i = Mathf.Max(0, list.Count - n); i < list.Count; i++) sum += list[i];
        return sum;
    }
}

[Serializable]
public class NightSuspicion
{
    public int day;
    public int[] channels;   // theo thứ tự SuspicionChannel
}
