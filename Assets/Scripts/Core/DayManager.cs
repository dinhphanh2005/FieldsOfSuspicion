using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayManager : MonoBehaviour
{
    [Header("Phase panels")]
    [SerializeField] GameObject newsPanel;
    [SerializeField] GameObject morningPanel;
    [SerializeField] GameObject afternoonPanel;
    [SerializeField] GameObject nightPanel;
    [SerializeField] GameObject endingPanel;

    [Header("Overlay")]
    [SerializeField] GameObject notebookPanel;

    [Header("Labels")]
    [SerializeField] TMP_Text newsTitle;

    [Header("Content")]
    [SerializeField] DaySO[] days;
    [SerializeField] IntelSO[] intel;
    [SerializeField] CaseSO[] cases;
    [SerializeField] EndingSO[] endings;
    [SerializeField] CropSO[] crops;
    [SerializeField] float[] burnModelWeights = new float[0];
    [SerializeField] float burnModelBias;

    [Header("Backgrounds — Assets/Sprites/Backgrounds/bg_*.png (thiếu thì dùng màu nền)")]
    [SerializeField] Sprite bgNews;
    [SerializeField] Sprite bgBoard;
    [SerializeField] Sprite bgShop;
    [SerializeField] Sprite bgNight;
    [SerializeField] Sprite bgNotebook;
    [SerializeField] Sprite bgEnding;

    [Header("Font tiêu đề — Assets/Fonts/Lora-Bold.ttf (thiếu thì dùng font mặc định)")]
    [SerializeField] Font titleFont;

    public GameState State { get; private set; }
    public Phase CurrentPhase { get; private set; }

    public DaySO Today => days == null ? null : System.Array.Find(days, d => d && d.day == State.day);

    public IntelSO[] AllIntel => intel ?? new IntelSO[0];
    public CaseSO[] AllCases => cases == null ? new CaseSO[0] : cases.Where(c => c).OrderBy(c => c.id).ToArray();
    public CaseSO FinalCase => AllCases.FirstOrDefault(c => c.isFinal);
    public CropSO[] AllCrops => crops ?? new CropSO[0];

    NewsView newsView;
    BoardView boardView;
    ShopView shopView;
    NightView nightView;
    NotebookView notebookView;
    EndingView endingView;
    HudView hud;
    TelemetryLogger telemetry;
    TitleView titleView;
    Image fader;
    float fade;

    void Awake()
    {
        foreach (var scaler in FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None))
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        days = Clean(days); intel = Clean(intel); cases = Clean(cases); endings = Clean(endings); crops = Clean(crops);

        var catalog = Resources.Load<ContentCatalog>("FoSContentCatalog");
        if (catalog)
        {
            UIFactory.SetSpriteCatalog(catalog.interfaceSprites);
            if (days == null || days.Length == 0) days = catalog.days;
            if (intel == null || intel.Length == 0) intel = catalog.intel;
            if (cases == null || cases.Length == 0) cases = catalog.cases;
            if (endings == null || endings.Length == 0) endings = catalog.endings;
            if (crops == null || crops.Length == 0) crops = catalog.crops;
            if (!bgNews) bgNews = catalog.bgNews;
            if (!bgBoard) bgBoard = catalog.bgBoard;
            if (!bgShop) bgShop = catalog.bgShop;
            if (!bgNight) bgNight = catalog.bgNight;
            if (!bgNotebook) bgNotebook = catalog.bgNotebook;
            if (!bgEnding) bgEnding = catalog.bgEnding;
            if (!titleFont) titleFont = catalog.titleFont;
        }
#if UNITY_EDITOR
        if (!catalog || catalog.interfaceSprites == null || catalog.interfaceSprites.Length == 0 || !UIFactory.SpriteAsset("plot_empty"))
            UIFactory.SetSpriteCatalog(UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites" })
            .Select(guid => UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)))
            .Where(sprite => sprite && !UnityEditor.AssetDatabase.GetAssetPath(sprite).StartsWith("Assets/Sprites/Characters/"))
            .ToArray());
        if (days == null || days.Length == 0) days = LoadAll<DaySO>();
        if (intel == null || intel.Length == 0) intel = LoadAll<IntelSO>();
        if (cases == null || cases.Length == 0) cases = LoadAll<CaseSO>();
        if (endings == null || endings.Length == 0) endings = LoadAll<EndingSO>();
        if (crops == null || crops.Length == 0) crops = LoadAll<CropSO>();
        if (!bgNews)     bgNews     = LoadBg("bg_news");
        if (!bgBoard)    bgBoard    = LoadBg("bg_board");
        if (!bgShop)     bgShop     = LoadBg("bg_shop");
        if (!bgNight)    bgNight    = LoadBg("bg_night");
        if (!bgNotebook) bgNotebook = LoadBg("bg_notebook");
        if (!bgEnding)   bgEnding   = LoadBg("bg_ending");
        if (!titleFont)  titleFont  = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Lora-Bold.ttf");
#endif
        if (titleFont && !UIFactory.TitleFont) UIFactory.TitleFont = TMP_FontAsset.CreateFontAsset(titleFont);

        var bodyFont = TMP_Settings.defaultFontAsset;
        if (bodyFont && bodyFont.atlasPopulationMode == AtlasPopulationMode.Dynamic)
        {
            bodyFont.isMultiAtlasTexturesEnabled = true;
            bodyFont.TryAddCharacters("ÀÁẢÃẠĂẰẮẲẴẶÂẦẤẨẪẬÈÉẺẼẸÊỀẾỂỄỆÌÍỈĨỊÒÓỎÕỌÔỒỐỔỖỘƠỜỚỞỠỢÙÚỦŨỤƯỪỨỬỮỰỲÝỶỸỴĐàáảãạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđ");
        }

        UIFactory.SetBackground(newsPanel, bgNews);
        UIFactory.SetBackground(morningPanel, bgBoard);
        UIFactory.SetBackground(afternoonPanel, bgShop);
        UIFactory.SetBackground(nightPanel, bgNight, 0.15f);
        UIFactory.SetBackground(notebookPanel, bgNotebook, 0.35f);
        UIFactory.SetBackground(endingPanel, bgEnding, 0.2f);

        foreach (var panel in new[] { newsPanel, morningPanel, afternoonPanel, nightPanel, notebookPanel, endingPanel })
            UIFactory.StylePanel(panel);

        newsView     = Ensure<NewsView>(newsPanel);         newsView.Init(this);
        boardView    = Ensure<BoardView>(morningPanel);     boardView.Init(this);
        shopView     = Ensure<ShopView>(afternoonPanel);    shopView.Init(this);
        nightView    = Ensure<NightView>(nightPanel);       nightView.Init(this);
        notebookView = Ensure<NotebookView>(notebookPanel); notebookView.Init(this);
        endingView   = Ensure<EndingView>(endingPanel);     endingView.Init();

        var hudRect = UIFactory.MakeRect(newsPanel.transform.parent, "HUD", new Vector2(0, 1), Vector2.one);
        hudRect.pivot = new Vector2(0.5f, 1);
        hudRect.sizeDelta = new Vector2(0, UIFactory.HudHeight);
        hudRect.SetAsLastSibling();
        hud = hudRect.gameObject.AddComponent<HudView>();
        hud.Init(this);
        telemetry = gameObject.AddComponent<TelemetryLogger>();
        GameAudio.Ensure(gameObject);
        var titleRect = UIFactory.MakeRect(newsPanel.transform.parent, "Title_Screen", Vector2.zero, Vector2.one);
        titleView = titleRect.gameObject.AddComponent<TitleView>();
        titleView.Init(this);

        var faderRect = UIFactory.MakeRect(newsPanel.transform.parent, "Fader", Vector2.zero, Vector2.one);
        fader = UIFactory.MakeBox(faderRect, new Color(0.08f, 0.06f, 0.05f, 0f), false);
        faderRect.SetAsLastSibling();
    }

#if UNITY_EDITOR
    static Sprite LoadBg(string name) =>
        UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Backgrounds/{name}.png");

    static T[] LoadAll<T>() where T : Object =>
        UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name)
            .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)))
            .ToArray();
#endif

    static T[] Clean<T>(T[] arr) where T : Object => arr == null ? null : arr.Where(x => x).ToArray();

    static T Ensure<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c ? c : go.AddComponent<T>();
    }

    void Start()
    {
        ApplySettings();
        ShowTitle();
    }

    void ShowTitle()
    {
        foreach (var panel in new[] { newsPanel, morningPanel, afternoonPanel, nightPanel, endingPanel, notebookPanel })
            if (panel) panel.SetActive(false);
        if (hud) hud.gameObject.SetActive(false);
        titleView.Show();
        if (fader) fader.transform.SetAsLastSibling();
    }

    public void ToggleFullscreen()
    {
        bool full = !Screen.fullScreen;
        if (full)
        {
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        }
        else
        {
            var size = DefaultWindowSize();
            Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
            PlayerPrefs.SetInt("FoS.Width", size.x);
            PlayerPrefs.SetInt("FoS.Height", size.y);
        }
        PlayerPrefs.SetInt("FoS.Fullscreen", full ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Cửa sổ mặc định: khung 16:9 chiếm khoảng 80% màn hình thật, không phụ thuộc máy to hay nhỏ.
    static Vector2Int DefaultWindowSize()
    {
        float scale = Mathf.Min(Display.main.systemWidth * 0.8f / UIFactory.RefWidth, Display.main.systemHeight * 0.8f / UIFactory.RefHeight);
        return new Vector2Int(Mathf.RoundToInt(UIFactory.RefWidth * scale), Mathf.RoundToInt(UIFactory.RefHeight * scale));
    }

    static Vector2Int[] ResolutionChoices()
    {
        var list = Screen.resolutions.Select(r => new Vector2Int(r.width, r.height))
                         .Where(r => r.x >= 800 && r.y >= 600)
                         .Distinct().OrderBy(r => r.x * r.y).ToList();
        var native = new Vector2Int(Display.main.systemWidth, Display.main.systemHeight);
        if (!list.Contains(native)) list.Add(native);
        return list.ToArray();
    }

    void OnApplicationQuit()
    {
        if (Screen.fullScreen) return;
        PlayerPrefs.SetInt("FoS.Width", Screen.width);
        PlayerPrefs.SetInt("FoS.Height", Screen.height);
        PlayerPrefs.Save();
    }

    public void CycleVolume()
    {
        float v = Mathf.Round(AudioListener.volume * 5f) / 5f - 0.2f;
        AudioListener.volume = v < -0.01f ? 1f : Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("FoS.Volume", AudioListener.volume);
        PlayerPrefs.Save();
    }

    public void CycleResolution()
    {
        var choices = ResolutionChoices();
        if (choices.Length == 0) return;
        int current = System.Array.FindIndex(choices, r => r.x == Screen.width && r.y == Screen.height);
        var next = choices[(current + 1) % choices.Length];
        var mode = Screen.fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(next.x, next.y, mode);
        PlayerPrefs.SetInt("FoS.Width", next.x);
        PlayerPrefs.SetInt("FoS.Height", next.y);
        PlayerPrefs.Save();
    }

    public void StartNewGame(string playerName)
    {
        SaveSystem.DeleteSave();
        State = new GameState { day = 1, playerName = string.IsNullOrWhiteSpace(playerName) ? "Wren" : playerName.Trim() };
        telemetry.RunId = System.DateTime.Now.ToString("yyyyMMdd-HHmmss");
        BeginFromState();
    }

    public string ContinueGame()
    {
        if (!SaveSystem.TryLoad(out var state, out var error)) return error;
        State = state;
        BeginFromState();
        return null;
    }

    void BeginFromState()
    {
        if (hud) hud.gameObject.SetActive(true);
        EnterPhase(Phase.News);
    }

    void ApplySettings()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("FoS.Volume", 1f);
#if !UNITY_EDITOR
        bool full = PlayerPrefs.GetInt("FoS.Fullscreen", 1) == 1;
        int width = PlayerPrefs.GetInt("FoS.Width", 0);
        int height = PlayerPrefs.GetInt("FoS.Height", 0);
        if (full) Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        else if (width > 0 && height > 0) Screen.SetResolution(width, height, FullScreenMode.Windowed);
        else { var size = DefaultWindowSize(); Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed); }
#endif
    }

    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        if (!fader || fade <= 0f) return;
        fade = Mathf.Max(0f, fade - Time.unscaledDeltaTime / 0.45f);
        var c = fader.color;
        c.a = fade * fade * 0.9f;
        fader.color = c;
    }

    public void EnterPhase(Phase phase)
    {
        CurrentPhase = phase;
        if (phase != Phase.Resolve) fade = 1f;
        if (fader) fader.transform.SetAsLastSibling();

        newsPanel.SetActive(phase == Phase.News);
        morningPanel.SetActive(phase == Phase.Morning);
        afternoonPanel.SetActive(phase == Phase.Afternoon);
        nightPanel.SetActive(phase == Phase.Night);
        endingPanel.SetActive(phase == Phase.Ended);
        notebookPanel.SetActive(false);

        GameAudio.Music(phase == Phase.Night || (phase == Phase.Ended && State.ending == EndingId.Burned.ToString()) ? "night" : "day");

        switch (phase)
        {
            case Phase.News:
                if (newsTitle) newsTitle.text = $"TIN TỨC — {State.DayLabel}";
                if (!Today) Debug.LogWarning($"Chưa có DaySO cho N{State.day}");
                string letter = State.lastRentMissDay == State.day - 1 ? EconomyRules.RentLetter(State.rentWarnings) : null;
                var shown = newsView.Show(Today, State, letter, NightRecap());
                foreach (var n in shown)
                    if (n && !string.IsNullOrEmpty(n.grantsIntel))
                        foreach (var id in n.grantsIntel.Split(',')) CaptureIntel(id.Trim());
                break;
            case Phase.Morning:
                if (State.growthProcessedDay != State.day)
                {
                    FarmSystem.GrowthTick(State, crops);
                    State.growthProcessedDay = State.day;
                }
                boardView.Show(Today, State); break;
            case Phase.Afternoon: shopView.Begin(Today);        break;
            case Phase.Night:     nightView.Begin();            break;
            case Phase.Ended:     if (hud) hud.gameObject.SetActive(true); break;
            case Phase.Resolve:   Resolve();                    break;
        }
    }

    public void EndPhase()
    {
        switch (CurrentPhase)
        {
            case Phase.News:      EnterPhase(Phase.Morning);   break;
            case Phase.Morning:   EnterPhase(Phase.Afternoon); break;
            case Phase.Afternoon: EnterPhase(Phase.Night);     break;
            case Phase.Night:     CompleteNight(null);         break;
        }
    }

    public void CompleteNight(DropRecord drop)
    {
        if (drop != null)
        {
            var c = FindCase(drop.caseId);
            drop.correct = c && CaseEvaluator.IsCorrect(c, drop.answers);
            State.deliveredDrops.Add(drop);
            GameEvents.RaiseDropDelivered(drop);

            if (c)
            {
                if (drop.correct) State.AddSuspicion(c.channel, c.correctSuspicion, $"Lenmar lần theo vụ {c.title.ToLower()}");
                else              State.AddSuspicion(c.channel, c.wrongSuspicion, $"báo cáo sai về {c.title.ToLower()}");
            }
        }
        EnterPhase(Phase.Resolve);
    }

    string NightRecap()
    {
        if (State.day <= 1 || State.nightlySuspicion.Count == 0) return "";
        int last = State.nightlySuspicion[State.nightlySuspicion.Count - 1];
        string rent = State.lastRentPaidDay == State.day - 1 ? $"Đã trả {EconomyRules.Rent} xu tiền nhà" :
                      State.lastRentMissDay == State.day - 1 ? "Không đủ tiền nhà" : "";
        string sus = last > 0 ? $"đêm qua có thêm người để ý tới cậu (+{last})" : "đêm qua không ai để ý tới cậu";
        string line = string.IsNullOrEmpty(rent) ? char.ToUpper(sus[0]) + sus.Substring(1) : $"{rent} · {sus}";
        var orders = State.handlerOrderResults.Where(r => r.day == State.day - 1)
                          .Select(r => $"{HandlerOrders.Key(r.instruction)} {(r.followed ? "đã làm" : "không làm")}").ToList();
        if (orders.Count > 0) line += "\nLệnh mật hôm qua: " + string.Join(" · ", orders);
        return line;
    }

    void PayRent()
    {
        if (RentSystem.Settle(State))
        {
            State.lastRentPaidDay = State.day;
            Debug.Log($"[Tiền nhà] -{EconomyRules.Rent} xu, còn {State.money}");
        }
        else
        {
            Debug.Log($"[Tiền nhà] Thiếu tiền nhà lần {State.rentWarnings}");
        }
    }

    // Cuối mỗi đêm: trả tiền nhà, chốt nghi ngờ, chấm lệnh mật rồi mới xét kết thúc.
    void Resolve()
    {
        PayRent();
        State.CloseNight();
        if (BurnPredictor.TryPredict(State, burnModelWeights, burnModelBias, out float probability))
            State.burnProbability = probability;
        Debug.Log($"[Nghi ngờ] Tổng kết N{State.day}: đêm nay {State.nightlySuspicion[State.nightlySuspicion.Count - 1]}, " +
                  $"cửa sổ {SuspicionRules.WindowNights} đêm = {State.SuspicionWindow} / {SuspicionRules.BurnThreshold}, " +
                  $"cộng dồn = {State.SuspicionTotal}");
        GameEvents.RaiseDayResolved(State.day);
        HandlerOrderEvaluator.ResolveDay(State, Today, days, FindIntel);

        var ending = EndingResolver.Resolve(State, FinalCase);
        if (ending.HasValue)
        {
            State.ending = ending.Value.ToString();
            GameEvents.RaiseEndingReached(ending.Value);
            telemetry.WriteDayRow(State);
            EnterPhase(Phase.Ended);
            endingView.Show(FindEnding(ending.Value), State, this);
            return;
        }

        telemetry.WriteDayRow(State);
        State.NextDay();
        if (!SaveSystem.AutoSave(State, out var saveError)) Debug.LogWarning("Tự lưu thất bại: " + saveError);
        EnterPhase(Phase.News);
    }

    public void CaptureIntel(string id)
    {
        if (!FindIntel(id)) { Debug.LogWarning($"Thiếu manh mối {id}"); return; }
        if (!State.AddFragment(id)) return;
        if (CurrentPhase == Phase.Afternoon) GameAudio.Sfx("pen");
        if (!State.capturedToday.Contains(id)) State.capturedToday.Add(id);
        GameEvents.RaiseIntelCaptured(id);
    }

    public CaseSO FindCase(string id) =>
        string.IsNullOrEmpty(id) ? null : AllCases.FirstOrDefault(c => c.id == id);

    public IntelSO FindIntel(string id) =>
        intel == null ? null : System.Array.Find(intel, i => i && i.id == id);

    public CropSO FindCrop(string id) => crops == null ? null : System.Array.Find(crops, c => c && c.id == id);

    EndingSO FindEnding(EndingId id) =>
        endings == null ? null : System.Array.Find(endings, e => e && e.id == id);

    public void OpenNotebook(bool casesTab = false, string caseId = null)
    {
        GameAudio.Sfx("page");
        notebookPanel.SetActive(true);
        notebookPanel.transform.SetAsLastSibling();
        if (hud) hud.transform.SetAsLastSibling();
        notebookView.Open(casesTab, caseId);
    }

    public bool NotebookOpen => notebookPanel && notebookPanel.activeSelf;

    public void CloseNotebook()
    {
        notebookPanel.SetActive(false);
        if (CurrentPhase == Phase.Night) nightView.Refresh();
        if (CurrentPhase == Phase.Afternoon) shopView.RefreshNotebookButton();
    }
}
