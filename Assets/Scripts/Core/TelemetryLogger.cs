using System;
using System.IO;
using System.Text;
using UnityEngine;

// Ghi một dòng snapshot mỗi ngày và các sự kiện phát sinh trong ngày ra CSV cục bộ.
public sealed class TelemetryLogger : MonoBehaviour
{
    StringBuilder events = new StringBuilder();
    // v2: thêm cột run, report, suspicion_cumulative. File mới để không lẫn cột với bản cũ.
    public string CsvPath => Path.Combine(Application.persistentDataPath, "telemetry_v2.csv");
    // Mã ván chơi (đặt lúc bắt đầu ván mới) để tách nhiều ván trong cùng một file CSV
    public string RunId = DateTime.Now.ToString("yyyyMMdd-HHmmss");

    void OnEnable()
    {
        GameEvents.IntelCaptured += OnIntelCaptured;
        GameEvents.DropDelivered += OnDropDelivered;
        GameEvents.SuspicionAdded += OnSuspicionAdded;
        GameEvents.EndingReached += OnEndingReached;
        GameEvents.HandlerOrderResolved += OnHandlerOrderResolved;
    }

    void OnDisable()
    {
        GameEvents.IntelCaptured -= OnIntelCaptured;
        GameEvents.DropDelivered -= OnDropDelivered;
        GameEvents.SuspicionAdded -= OnSuspicionAdded;
        GameEvents.EndingReached -= OnEndingReached;
        GameEvents.HandlerOrderResolved -= OnHandlerOrderResolved;
    }

    void OnIntelCaptured(string id) => AddEvent("IntelCaptured", id);
    void OnDropDelivered(DropRecord drop) => AddEvent("ReportDelivered", drop.HasReport ? $"{drop.caseId}:{(drop.correct ? "correct" : "wrong")}" : "none");
    void OnSuspicionAdded(int points, string reason) => AddEvent("SuspicionAdded", $"{points}:{reason}");
    void OnEndingReached(EndingId id) => AddEvent("EndingReached", id.ToString());
    void OnHandlerOrderResolved(string id, Handler handler, bool followed) => AddEvent("HandlerOrder", $"{handler}:{id}:{followed}");
    void AddEvent(string type, string value) => events.Append(type).Append('=').Append(value).Append('|');

    public void WriteDayRow(GameState state)
    {
        try
        {
            bool header = !File.Exists(CsvPath);
            using (var writer = new StreamWriter(CsvPath, true, new UTF8Encoding(true)))
            {
                if (header) writer.WriteLine("run,day,money,stock,energy,action_points,price_level,customer_momentum,sales,clues,report,suspicion_night,suspicion_window,suspicion_cumulative,rent_warnings,p_burned,burn_model_eligible,ending,events");
                var tonight = state.TonightDrop;
                string delivered = tonight == null || !tonight.HasReport ? "" : $"{tonight.caseId}:{(tonight.correct ? 1 : 0)}";
                string row = string.Join(",", new[] {
                    RunId, state.day.ToString(), state.money.ToString(), state.stock.ToString(), state.energy.ToString(), state.actionPoints.ToString(),
                    state.shopPriceLevel.ToString(), state.customerBaseMomentum.ToString(), state.salesToday.ToString(),
                    Csv(string.Join("|", state.fragments)), Csv(delivered),
                    state.nightlySuspicion.Count == 0 ? "0" : state.nightlySuspicion[state.nightlySuspicion.Count - 1].ToString(),
                    state.SuspicionWindow.ToString(), state.SuspicionTotal.ToString(), state.rentWarnings.ToString(),
                    state.burnProbability < 0 ? "" : state.burnProbability.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                    state.ending == EndingId.Evicted.ToString() ? "0" : "1",
                    Csv(state.ending ?? ""), Csv(events.ToString())
                });
                writer.WriteLine(row);
            }
        }
        catch (Exception ex) { Debug.LogWarning("Telemetry không ghi được; ván chơi vẫn tiếp tục: " + ex.Message); }
        finally { events.Length = 0; }
    }

    static string Csv(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
}
