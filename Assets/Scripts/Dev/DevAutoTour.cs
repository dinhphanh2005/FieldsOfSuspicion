#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevAutoTour : MonoBehaviour
{
    public const string PrefKey = "FoS.DevAutoTour";
    public static string OutDir => Path.Combine(Path.GetDirectoryName(Application.dataPath), "DevShots");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        string plan = UnityEditor.EditorPrefs.GetString(PrefKey, "");
        string stamp = UnityEditor.EditorPrefs.GetString(PrefKey + ".at", "");
        UnityEditor.EditorPrefs.DeleteKey(PrefKey);
        UnityEditor.EditorPrefs.DeleteKey(PrefKey + ".at");
        if (string.IsNullOrEmpty(plan)) return;
        if (!long.TryParse(stamp, out long ticks) || (System.DateTime.UtcNow - new System.DateTime(ticks)).TotalSeconds > 60)
        {
            Debug.Log("[Tour] Bỏ qua cờ tự chơi cũ.");
            return;
        }
        var go = new GameObject("DevAutoTour");
        go.AddComponent<DevAutoTour>().plan = plan;
    }

    string plan;
    int shot;
    string Dir => Path.Combine(OutDir, plan == "rook-correct" ? "" : plan);
    readonly StringBuilder log = new StringBuilder();
    DayManager dm;
    Canvas canvas;

    void Log(string s) { Debug.Log("[Tour] " + s); log.AppendLine(s); }

    IEnumerator Start()
    {
        Application.runInBackground = true;
        if (Directory.Exists(Dir)) foreach (var f in Directory.GetFiles(Dir)) File.Delete(f);
        Directory.CreateDirectory(Dir);
        Application.logMessageReceived += (msg, st, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Warning)
                log.AppendLine($"!! {type}: {msg}\n{st}");
        };
        yield return new WaitForSecondsRealtime(0.5f);
        dm = FindFirstObjectByType<DayManager>();
        canvas = FindFirstObjectByType<Canvas>();
        Log("Kế hoạch: " + plan);

        yield return Wait(3.2f);
        yield return Shot("title");
        Click("Cài đặt");
        yield return Wait(0.8f);
        yield return Shot("title_settings");
        Click("Quay lại");
        yield return Wait(0.8f);
        Click("Ván mới");
        yield return Wait(1.6f);
        yield return Shot("intro_0");
        for (int i = 1; i < 4; i++)
        {
            ClickNamed("Hit");
            yield return Wait(0.9f);
            yield return Shot($"intro_{i}");
        }
        var input = FindFirstObjectByType<TMP_InputField>();
        if (input) input.text = "Minh";
        yield return Shot("intro_name");
        Click("Ký tên");
        yield return Wait(1.2f);
        yield return Shot("intro_signed");
        ClickNamed("Hit");
        yield return Wait(2.5f);

        for (int day = 1; day <= 7; day++)
        {
            if (dm.CurrentPhase == Phase.Ended) break;
            Log($"===== N{day} =====");
            yield return News(day);
            yield return Morning(day);
            yield return Afternoon(day);
            yield return Night(day);
            yield return Wait();
        }
        yield return Shot("ending");
        Log($"Kết thúc: {dm.State.ending} · nghi ngờ cửa sổ {dm.State.SuspicionWindow} · tiền {dm.State.money}");
        File.WriteAllText(Path.Combine(Dir, "tour.log"), log.ToString());
        UnityEditor.EditorApplication.isPlaying = false;
    }

    IEnumerator News(int day)
    {
        for (int page = 0; page < 6 && dm.CurrentPhase == Phase.News; page++)
        {
            CaptureAllLinks();
            yield return Wait(0.2f);
            yield return Shot($"N{day}_news_{page}");
            if (!ClickPrimary()) break;
            yield return Wait();
        }
    }

    IEnumerator Morning(int day)
    {
        if (dm.CurrentPhase != Phase.Morning) yield break;
        yield return Shot($"N{day}_board");
        if (day == 1)
        {
            var hud = FindAnyObjectByType<HudView>();
            if (hud) { hud.SetPaused(true); yield return Wait(0.3f); yield return Shot("pause_menu"); hud.SetPaused(false); yield return Wait(0.2f); }
        }
        if (day == 1)
        {
            Click("Giải sẵn lệnh"); yield return Wait(); yield return Shot($"N{day}_board_decoded");
        }
        if (plan == "nofarm") { ClickPrimary(); yield return Wait(); yield break; }
        Click("Ruộng");
        yield return Wait();
        for (int i = 0; i < 6; i++) ClickNamed("Plot_" + i);
        yield return Wait();
        yield return Shot($"N{day}_farm");
        Click("Đóng ruộng");
        yield return Wait();
        ClickPrimary();
        yield return Wait();
    }

    static readonly Dictionary<string, string[]> Script = new Dictionary<string, string[]>
    {
        ["1_0"] = new[] { "sell", "ask" },
        ["2_0"] = new[] { "sell", "ask" },
        ["2_1"] = new[] { "discount", "ask" },
        ["2_2"] = new[] { "sell" },
        ["3_0"] = new[] { "sell", "ask" },
        ["3_1"] = new[] { "sell", "ask" },
        ["4_0"] = new[] { "discount", "ask" },
        ["4_1"] = new[] { "sell", "ask" },
        ["4_2"] = new[] { "discount" },
        ["5_0"] = new[] { "sell", "ask" },
        ["5_1"] = new[] { "sell", "ask" },
        ["6_0"] = new[] { "sell", "ask" },
        ["6_1"] = new[] { "ask", "sell" },
        ["7_0"] = new[] { "sell", "ask" },
        ["7_1"] = new[] { "sell", "ask", "press" },
    };

    IEnumerator Afternoon(int day)
    {
        if (dm.CurrentPhase != Phase.Afternoon) yield break;
        yield return Shot($"N{day}_shop_prompt");
        if (plan == "broke")
        {
            Click("Nghỉ chiều nay");
            yield return Wait();
            yield return Shot($"N{day}_shop_closed");
            Click("Sang buổi tối");
            yield return Wait();
            yield break;
        }
        Click("Mở tiệm");
        yield return Wait();
        for (int k = 0; k < 4; k++)
        {
            yield return Shot($"N{day}_shop_v{k}_greet");
            if (Script.TryGetValue($"{day}_{k}", out var acts))
            {
                if (plan == "shrike" && day == 2 && k == 2) acts = new[] { "ask" };
                if (plan == "pressall") acts = new[] { "sell", "ask", "press" };
                if (plan == "pressdisc") acts = new[] { "discount", "ask", "press" };
                foreach (var a in acts)
                {
                    bool ok = a == "ask" ? Click("Hỏi chuyện") : a == "press" ? Click("Hỏi dồn")
                            : a == "discount" ? (Click("Bớt giá") || Click("Đổi") || Click("Đề nghị")) : Click("Bán");
                    if (!ok) Log($"  (không bấm được {a})");
                    yield return Wait(0.3f);
                    CaptureAllLinks();
                    yield return Wait(0.3f);
                    yield return Shot($"N{day}_shop_v{k}_{a}");
                }
            }
            if (!Click("Tiễn khách")) Log("  (khách đã tự về)");
            yield return Wait(0.3f);
            yield return Shot($"N{day}_shop_v{k}_leave");
            if (Click("Mời khách sau")) { yield return Wait(); continue; }
            Click("Đóng tiệm");
            yield return Wait();
            break;
        }
        yield return Shot($"N{day}_shop_end");
        if (!Click("Sang buổi tối")) ClickPrimary();
        yield return Wait();
    }

    static readonly Dictionary<int, (string caseId, string[] answers, string[] route)> NightPlan = new()
    {
        [1] = ("C0", new[] { "K01" }, new[] { "river", "oak" }),
        [2] = ("C1", new[] { "K02", "K03" }, new[] { "well", "market", "oak" }),
        [3] = ("C2", new[] { "K07" }, new[] { "river", "oak" }),
        [4] = ("C3", new[] { "K10", "K11" }, new[] { "well", "market", "oak" }),
        [5] = (null, null, null),
        [6] = (null, null, null),
        [7] = ("C4", new[] { "K_SHRIKE", "K17" }, new[] { "river", "oak" }),
    };

    static readonly Dictionary<int, (string caseId, string[] answers, string[] route)> ShrikePlan = new()
    {
        [1] = ("C0", new[] { "K01" }, new[] { "river", "oak" }),
        [2] = ("C2", new[] { "K05" }, new[] { "well", "market", "oak" }),
        [3] = ("C1", new string[0], new[] { "well", "market", "oak" }),
        [4] = ("C1", new string[0], new[] { "well", "market", "oak" }),
        [5] = ("C1", new string[0], new[] { "well", "church", "bridge", "oak" }),
        [6] = ("C1", new string[0], new[] { "well", "church", "bridge", "oak" }),
        [7] = ("C4", new[] { "K_ROOK", "K18" }, new[] { "river", "oak" }),
    };

    IEnumerator Night(int day)
    {
        if (dm.CurrentPhase != Phase.Night) yield break;
        yield return Shot($"N{day}_night");
        var p = plan == "shrike" ? ShrikePlan[day] : plan == "broke" ? (null, null, null) : NightPlan[day];
        if (day == 7 && plan == "defection") p = ("C4", new[] { "K_ROOK", "K18" }, new[] { "river", "oak" });
        if (day == 7 && plan == "weak")      p = ("C4", new[] { "K_SHRIKE", "K19" }, new[] { "river", "oak" });
        if (day == 7 && plan == "cold")      p = (null, null, null);
        if (p.caseId == null) { Click("Ở nhà tối nay"); yield return Wait(); yield break; }

        if (p.answers.Length == 0) goto walk;
        Click("Sổ tay");
        yield return Wait();
        yield return Shot($"N{day}_notebook_cases");
        Click("Manh mối");
        yield return Wait();
        yield return Shot($"N{day}_notebook_clues");
        Click("Hồ sơ");
        yield return Wait();
        ClickNamed("Case_" + p.caseId);
        yield return Wait(0.3f);
        for (int i = 0; i < p.answers.Length; i++)
        {
            ClickNamedIndex("Pick", i);
            yield return Wait(0.2f);
            if (!ClickNamed("Chip_" + p.answers[i])) Log($"  (không có manh mối {p.answers[i]} trong sổ)");
            yield return Wait(0.2f);
        }
        yield return Shot($"N{day}_notebook_{p.caseId}");
        Click("Gấp sổ lại");
        yield return Wait();

        walk:
        foreach (var node in p.route) ClickNamedParent("Node_" + node);
        yield return Wait(0.3f);
        if (p.answers.Length > 0) ClickNamed("Report_" + p.caseId);
        yield return Wait(0.3f);
        yield return Shot($"N{day}_night_ready");
        if (ActiveButtons().Any(b => Plain(b.GetComponentInChildren<TMP_Text>()?.text).Contains("Gấp sổ lại")))
        {
            Log("  (báo cáo còn thiếu ô nên sổ tay mở lại: ở nhà tối nay)");
            Click("Gấp sổ lại");
            yield return Wait();
            Click("Ở nhà tối nay");
            yield return Wait();
            yield break;
        }
        ClickPrimary();
        yield return Wait();
    }

    static string Plain(string s) => Regex.Replace(s ?? "", "<.*?>", "");

    IEnumerable<Button> ActiveButtons() =>
        FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b => b.isActiveAndEnabled);

    bool Invoke(Button b, string what)
    {
        if (!b) { Log($"  ✗ không thấy nút: {what}"); return false; }
        if (!b.interactable) { Log($"  ✗ nút đang tắt: {what}"); return false; }
        Log($"  bấm: {what}");
        b.onClick.Invoke();
        return true;
    }

    bool Click(string label)
    {
        var b = ActiveButtons().FirstOrDefault(x =>
        {
            var t = x.GetComponentInChildren<TMP_Text>();
            return t && Plain(t.text).Trim().StartsWith(label) && x.interactable;
        }) ?? ActiveButtons().FirstOrDefault(x => { var t = x.GetComponentInChildren<TMP_Text>(); return t && Plain(t.text).Contains(label); });
        if (!b) { Log($"  - không có nút \"{label}\""); return false; }
        return Invoke(b, label);
    }

    bool ClickPrimary()
    {
        var b = ActiveButtons().FirstOrDefault(x => x.name == "Btn_Primary");
        return Invoke(b, "nút chính " + (b ? Plain(b.GetComponentInChildren<TMP_Text>().text) : ""));
    }

    bool ClickNamed(string name) => Invoke(ActiveButtons().FirstOrDefault(x => x.name == name), name);

    bool ClickNamedIndex(string name, int i)
    {
        var list = ActiveButtons().Where(x => x.name == name).OrderByDescending(x => x.transform.position.y).ToList();
        return Invoke(i < list.Count ? list[i] : null, $"{name}[{i}]");
    }

    bool ClickNamedParent(string parent) =>
        Invoke(ActiveButtons().FirstOrDefault(x => x.transform.parent && x.transform.parent.name == parent), parent);

    void CaptureAllLinks()
    {
        foreach (var ct in FindObjectsByType<ClickableText>(FindObjectsSortMode.None))
        {
            var t = ct.GetComponent<TMP_Text>();
            foreach (Match m in Regex.Matches(t.text, "<link=\"(\\w+)\">"))
            {
                Log("  ghi manh mối " + m.Groups[1].Value);
                ct.SimulateClick(m.Groups[1].Value);
            }
        }
    }

    IEnumerator Wait(float s = 0.6f) { yield return new WaitForSecondsRealtime(s); }

    IEnumerator Shot(string name)
    {
        yield return new WaitForEndOfFrame();
        var cam = new GameObject("ShotCam").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.cullingMask = ~0;
        int w = Screen.width, h = Screen.height;
        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        var oldMode = canvas.renderMode;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 10;
        Canvas.ForceUpdateCanvases();
        cam.Render();
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        canvas.renderMode = oldMode;
        canvas.worldCamera = null;
        File.WriteAllBytes(Path.Combine(Dir, $"{++shot:000}_{name}.png"), tex.EncodeToPNG());
        Destroy(tex); cam.targetTexture = null; rt.Release(); Destroy(rt); Destroy(cam.gameObject);
        Log($"[chụp] {shot:000}_{name}");
    }
}
#endif
