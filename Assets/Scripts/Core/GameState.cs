using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameState
{
    public const int LastDay = 7;
    public int day = 1;   // N1 = Sunday.
    public bool IsLastDay => day >= LastDay;

    // ---- Kinh tế ----
    public int money = EconomyRules.StartMoney;
    public int stock = EconomyRules.StartStock;     // bó cà rốt trong kho
    public int energy = EconomyRules.MaxEnergy;
    public int actionPoints = EconomyRules.MaxActionPoints;
    public ShopPriceLevel shopPriceLevel = ShopPriceLevel.Fair;
    public int customerBaseMomentum;
    public int salesToday;
    public bool harvestedToday;
    public bool shopOpenedToday;
    public int growthProcessedDay;
    public string playerName = "Wren";
    public List<string> capturedToday = new();
    public int rentWarnings;                         // số lần đã thiếu tiền nhà (không reset)
    public int lastRentMissDay = -1;                 // đêm gần nhất thiếu tiền nhà

    // Mã các mảnh tin đã ghi vào sổ tay ("F02"...).
    // Lưu mã (string) chứ không lưu IntelSO, để JsonUtility lưu game được.
    public List<string> fragments = new();
    public List<string> shownMessageIds = new();
    public List<FarmPlotState> farmPlots = FarmSystem.CreateStartingPlots();

    // Các đêm đã đi giao tin (đêm ở nhà thì không có bản ghi).
    public List<DropRecord> deliveredDrops = new();
    public List<HandlerOrderResult> handlerOrderResults = new();
    public List<IntelEvidenceState> intelEvidence = new();

    // ---- Nghi ngờ (cửa sổ trượt, nhiều kênh) ----
    // Tổng có trọng số của từng đêm đã tổng kết: phần tử i = đêm N(i+1).
    public List<int> nightlySuspicion = new();
    // Điểm từng kênh của từng đêm (để xem chi tiết và ghi CSV).
    public List<NightSuspicion> nightlyByChannel = new();
    // Điểm cộng trong ngày đang chơi, theo kênh; chốt ở Tổng kết.
    public int[] pendingByChannel = new int[4];

    public string ending;   // tên EndingId khi ván kết thúc
    public float burnProbability = -1f; // -1 khi chưa được cấu hình trọng số mô hình offline

    static readonly string[] DayNames =
        { "Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };

    public string Weekday => DayNames[Math.Clamp(day, 1, LastDay) - 1];
    public string DayLabel => $"N{day} — {Weekday}";

    public void NextDay()
    {
        day++;
        energy = EconomyRules.MaxEnergy;
        actionPoints = EconomyRules.MaxActionPoints;
        harvestedToday = false;
        shopOpenedToday = false;
        salesToday = 0;
        capturedToday.Clear();
    }

    public bool HasFragment(string id) => fragments.Contains(id);

    // Trả về true nếu là mảnh tin MỚI (để UI biết vừa ghi được tin).
    public bool AddFragment(string id)
    {
        if (HasFragment(id)) return false;
        fragments.Add(id);
        if (intelEvidence.Find(e => e.intelId == id) == null)
            intelEvidence.Add(new IntelEvidenceState { intelId = id, truthStatus = IntelTruthStatus.Unverified });
        return true;
    }

    public bool WasMessageShown(string id) => shownMessageIds.Contains(id);

    public void MarkMessageShown(string id)
    {
        if (!string.IsNullOrEmpty(id) && !shownMessageIds.Contains(id)) shownMessageIds.Add(id);
    }

    public bool WasDelivered(string id) => deliveredDrops.Exists(d => d.intelId == id);

    public IntelTruthStatus TruthStatus(string id)
    {
        var evidence = intelEvidence.Find(e => e.intelId == id);
        return evidence == null ? IntelTruthStatus.Unverified : evidence.truthStatus;
    }

    public void ReconcileEvidence()
    {
        bool hasFalseGranary = HasFragment("F03");
        bool hasTrueGranary = HasFragment("F04");
        if (hasFalseGranary && hasTrueGranary)
        {
            SetTruthStatus("F03", IntelTruthStatus.Contradicted);
            SetTruthStatus("F04", IntelTruthStatus.Confirmed);
        }

        int vauleClues = (HasFragment("F05") ? 1 : 0) + (HasFragment("F06") ? 1 : 0) + (HasFragment("F07") ? 1 : 0);
        if (vauleClues >= 2)
            foreach (string id in new[] { "F05", "F06", "F07" })
                if (HasFragment(id)) SetTruthStatus(id, IntelTruthStatus.Confirmed);
    }

    void SetTruthStatus(string id, IntelTruthStatus status)
    {
        var evidence = intelEvidence.Find(e => e.intelId == id);
        if (evidence == null) intelEvidence.Add(evidence = new IntelEvidenceState { intelId = id });
        evidence.truthStatus = status;
    }

    // Bản ghi giao tin của đêm hôm qua; null nếu đêm qua ở nhà.
    public DropRecord LastNightDrop => deliveredDrops.Find(d => d.day == day - 1);

    // Bản ghi giao tin của đêm hôm nay (dùng ở Tổng kết).
    public DropRecord TonightDrop => deliveredDrops.Find(d => d.day == day);

    public void AddSuspicion(SuspicionChannel channel, int points, string reason)
    {
        if (points <= 0) return;
        pendingByChannel[(int)channel] += points;
        GameEvents.RaiseSuspicionAdded(points, $"{reason} → {SuspicionRules.ChannelName(channel)}");
    }

    // Gọi ở Tổng kết: chốt điểm của đêm nay.
    public void CloseNight()
    {
        float total = 0;
        for (int c = 0; c < pendingByChannel.Length; c++)
            total += pendingByChannel[c] * SuspicionRules.Weight((SuspicionChannel)c);

        nightlySuspicion.Add(Mathf.RoundToInt(total));
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
