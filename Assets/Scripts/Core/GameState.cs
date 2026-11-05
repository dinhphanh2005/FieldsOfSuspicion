using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameState
{
    public const int LastDay = 7;
    public int day = 1;
    public bool IsLastDay => day >= LastDay;

    public int money = EconomyRules.StartMoney;
    public int stock = EconomyRules.StartStock;
    public int energy = EconomyRules.MaxEnergy;
    public int actionPoints = EconomyRules.MaxActionPoints;
    public ShopPriceLevel shopPriceLevel = ShopPriceLevel.Fair;
    public int customerBaseMomentum;
    public int salesToday;
    public bool shopOpenedToday;
    public int growthProcessedDay;
    public string playerName = "Wren";
    public List<string> capturedToday = new();
    public int rentWarnings;
    public int lastRentMissDay = -1;
    public int lastRentPaidDay = -1;

    public List<string> fragments = new();
    public List<ReportDraft> drafts = new();
    public List<string> shownMessageIds = new();
    public List<FarmPlotState> farmPlots = FarmSystem.CreateStartingPlots();

    public List<DropRecord> deliveredDrops = new();
    public List<HandlerOrderResult> handlerOrderResults = new();

    public List<int> nightlySuspicion = new();
    public List<NightSuspicion> nightlyByChannel = new();
    public int[] pendingByChannel = new int[4];

    public string ending;
    public float burnProbability = -1f;

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

    [NonSerialized] HashSet<string> saidThisVisit;
    public HashSet<string> SaidThisVisit => saidThisVisit ??= new HashSet<string>();

    public bool HasFragment(string id) => !string.IsNullOrEmpty(id) && fragments.Contains(id);

    public bool AddFragment(string id)
    {
        if (string.IsNullOrEmpty(id) || HasFragment(id)) return false;
        fragments.Add(id);
        return true;
    }

    public ReportDraft Draft(CaseSO c)
    {
        var d = drafts.Find(x => x.caseId == c.id);
        if (d == null) drafts.Add(d = new ReportDraft { caseId = c.id });
        while (d.answers.Count < c.slots.Count) d.answers.Add("");
        return d;
    }

    public void SetAnswer(CaseSO c, int slot, string clueId)
    {
        if (CaseDelivered(c.id)) return;
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

    public DropRecord LastNightDrop => deliveredDrops.Find(d => d.day == day - 1);

    public DropRecord TonightDrop => deliveredDrops.Find(d => d.day == day);

    // Nghi ngờ trong ngày được gom theo từng kênh, đến CloseNight mới chốt thành điểm của một đêm.
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

    public void CloseNight()
    {
        nightlySuspicion.Add(PendingTotal);
        nightlyByChannel.Add(new NightSuspicion { day = day, channels = (int[])pendingByChannel.Clone() });
        pendingByChannel = new int[pendingByChannel.Length];
    }

    // Cửa sổ trượt: tổng các đêm cuối, không phải tổng cả tuần (SuspicionTotal chỉ để so sánh).
    public int SuspicionWindow => SumLast(nightlySuspicion, SuspicionRules.WindowNights);

    public int ChannelWindow(SuspicionChannel c)
    {
        int sum = 0;
        for (int i = Mathf.Max(0, nightlyByChannel.Count - SuspicionRules.WindowNights); i < nightlyByChannel.Count; i++)
            sum += nightlyByChannel[i].channels[(int)c];
        return sum;
    }

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
    public int[] channels;
}
