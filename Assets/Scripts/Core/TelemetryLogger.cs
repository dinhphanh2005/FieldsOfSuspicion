using System;
using System.IO;
using System.Text;
using UnityEngine;

// Ghi một dòng snapshot mỗi ngày và các sự kiện phát sinh trong ngày ra CSV cục bộ.
public sealed class TelemetryLogger : MonoBehaviour
{
    StringBuilder events = new StringBuilder();
    public string CsvPath => Path.Combine(Application.persistentDataPath, "telemetry.csv");

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
    void OnDropDelivered(DropRecord drop) => AddEvent("DropDelivered", $"{drop.intelId}:{drop.mode}");
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
                if (header) writer.WriteLine("day,money,stock,energy,action_points,price_level,customer_momentum,sales,fragments,delivered,suspicion_night,suspicion_window,rent_warnings,p_burned,burn_model_eligible,ending,events");
                string delivered = state.TonightDrop == null ? "" : state.TonightDrop.intelId;
                string row = string.Join(",", new[] {
                    state.day.ToString(), state.money.ToString(), state.stock.ToString(), state.energy.ToString(), state.actionPoints.ToString(),
                    state.shopPriceLevel.ToString(), state.customerBaseMomentum.ToString(), state.salesToday.ToString(),
                    Csv(string.Join("|", state.fragments)), Csv(delivered),
                    state.nightlySuspicion.Count == 0 ? "0" : state.nightlySuspicion[state.nightlySuspicion.Count - 1].ToString(),
                    state.SuspicionWindow.ToString(), state.rentWarnings.ToString(),
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
