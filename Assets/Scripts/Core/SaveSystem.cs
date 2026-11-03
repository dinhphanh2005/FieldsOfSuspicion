using System;
using System.IO;
using UnityEngine;

[Serializable]
class SaveEnvelope
{
    public int version = SaveSystem.CurrentVersion;
    public string gameStateJson;
}

// Tất cả trạng thái runtime chỉ chứa kiểu dữ liệu JsonUtility hỗ trợ và id, không chứa tham chiếu asset.
public static class SaveSystem
{
    public const int CurrentVersion = 1;
    public static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    public static bool HasSave => File.Exists(SavePath);

    public static bool AutoSave(GameState state, out string error)
    {
        error = null;
        try
        {
            string json = JsonUtility.ToJson(state, true);
            string envelope = JsonUtility.ToJson(new SaveEnvelope { version = CurrentVersion, gameStateJson = json }, true);
            string temp = SavePath + ".tmp";
            File.WriteAllText(temp, envelope);
            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(temp, SavePath);
            return true;
        }
        catch (Exception ex) { error = ex.Message; return false; }
    }

    public static bool TryLoad(out GameState state, out string error)
    {
        state = null;
        error = null;
        try
        {
            if (!File.Exists(SavePath)) { error = "Chưa có bản lưu."; return false; }
            var envelope = JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(SavePath));
            if (envelope == null || envelope.version != CurrentVersion)
            { error = "Phiên bản bản lưu không tương thích."; return false; }
            state = JsonUtility.FromJson<GameState>(envelope.gameStateJson);
            if (state == null || state.day < 1 || state.day > GameState.LastDay)
            { state = null; error = "Dữ liệu bản lưu không hợp lệ."; return false; }
            FarmSystem.EnsurePlots(state);
            if (state.pendingByChannel == null || state.pendingByChannel.Length != 4) state.pendingByChannel = new int[4];
            if (state.fragments == null) state.fragments = new System.Collections.Generic.List<string>();
            if (state.deliveredDrops == null) state.deliveredDrops = new System.Collections.Generic.List<DropRecord>();
            if (state.handlerOrderResults == null) state.handlerOrderResults = new System.Collections.Generic.List<HandlerOrderResult>();
            if (state.intelEvidence == null) state.intelEvidence = new System.Collections.Generic.List<IntelEvidenceState>();
            if (state.shownMessageIds == null) state.shownMessageIds = new System.Collections.Generic.List<string>();
            if (state.capturedToday == null) state.capturedToday = new System.Collections.Generic.List<string>();
            if (state.nightlySuspicion == null) state.nightlySuspicion = new System.Collections.Generic.List<int>();
            if (state.nightlyByChannel == null) state.nightlyByChannel = new System.Collections.Generic.List<NightSuspicion>();
            state.ReconcileEvidence();
            return true;
        }
        catch (Exception ex) { error = ex.Message; state = null; return false; }
    }

    public static void DeleteSave()
    {
        try { if (File.Exists(SavePath)) File.Delete(SavePath); }
        catch (Exception ex) { Debug.LogWarning("Không xóa được bản lưu: " + ex.Message); }
    }
}
