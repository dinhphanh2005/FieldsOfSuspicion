using System.Linq;
using TMPro;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [Header("Phase panels")]
    [SerializeField] GameObject newsPanel;
    [SerializeField] GameObject morningPanel;
    [SerializeField] GameObject afternoonPanel;
    [SerializeField] GameObject nightPanel;
    [SerializeField] GameObject endingPanel;

    [Header("Overlay")]
    [SerializeField] GameObject notebookPanel;   // không phải pha; chỉ mở được buổi chiều

    [Header("Labels")]
    [SerializeField] TMP_Text newsTitle;

    [Header("Content")]
    [SerializeField] DaySO[] days;               // kéo N2, N3 vào
    [SerializeField] IntelSO[] intel;            // kéo F01–F06 vào
    [SerializeField] int startDay = 2;           // tạm = 2 (chưa có nội dung N1); đổi về 1 ở Sprint 2

    public GameState State { get; private set; }
    public Phase CurrentPhase { get; private set; }

    // Nội dung của ngày đang chơi; null nếu chưa có DaySO cho ngày đó
    public DaySO Today => days == null ? null : System.Array.Find(days, d => d && d.day == State.day);

    NewsView newsView;
    BoardView boardView;
    ShopView shopView;
    NightView nightView;
    NotebookView notebookView;

    void Awake()
    {
#if UNITY_EDITOR
        // Tiện lúc làm: chưa kéo dữ liệu vào Inspector thì tự tìm mọi DaySO / IntelSO trong project.
        // Trước khi build (Sprint 3) vẫn phải kéo vào Inspector rồi lưu scene.
        if (days == null || days.Length == 0) days = LoadAll<DaySO>();
        if (intel == null || intel.Length == 0) intel = LoadAll<IntelSO>();
#endif
        foreach (var panel in new[] { newsPanel, morningPanel, afternoonPanel, nightPanel, notebookPanel, endingPanel })
            UIFactory.StylePanel(panel);

        // Mỗi panel có một "view" lo phần hiển thị; DayManager chỉ điều phối.
        newsView     = Ensure<NewsView>(newsPanel);         newsView.Init();
        boardView    = Ensure<BoardView>(morningPanel);     boardView.Init();
        shopView     = Ensure<ShopView>(afternoonPanel);    shopView.Init(this);
        nightView    = Ensure<NightView>(nightPanel);       nightView.Init(this);
        notebookView = Ensure<NotebookView>(notebookPanel); notebookView.Init(this);
    }

#if UNITY_EDITOR
    static T[] LoadAll<T>() where T : Object =>
        UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name)
            .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)))
            .ToArray();
#endif

    static T Ensure<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c ? c : go.AddComponent<T>();
    }

    void Start()
    {
        State = new GameState { day = startDay };
        EnterPhase(Phase.News);
    }

    public void EnterPhase(Phase phase)
    {
        CurrentPhase = phase;

        newsPanel.SetActive(phase == Phase.News);
        morningPanel.SetActive(phase == Phase.Morning);
        afternoonPanel.SetActive(phase == Phase.Afternoon);
        nightPanel.SetActive(phase == Phase.Night);
        endingPanel.SetActive(phase == Phase.Ended);
        notebookPanel.SetActive(false);

        switch (phase)
        {
            case Phase.News:
                newsTitle.text = $"TIN TỨC — {State.DayLabel}";
                if (!Today) Debug.LogWarning($"Chưa có DaySO cho N{State.day}");
                newsView.Show(Today, State);
                break;
            case Phase.Morning:   boardView.Show(Today, State); break;
            case Phase.Afternoon: shopView.Begin(Today);        break;
            case Phase.Night:     nightView.Begin();            break;
            case Phase.Resolve:   Resolve();                    break;
        }
    }

    // Gắn vào OnClick của các nút "Tiếp"
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

    // NightView gọi khi người chơi bấm "Đi" (drop != null) hoặc "Ở nhà tối nay" (drop == null)
    public void CompleteNight(DropRecord drop)
    {
        if (drop != null)
        {
            State.deliveredDrops.Add(drop);
            GameEvents.RaiseDropDelivered(drop);
        }
        EnterPhase(Phase.Resolve);
    }

    void Resolve()
    {
        GameEvents.RaiseDayResolved(State.day);

        if (State.IsLastDay)
        {
            EnterPhase(Phase.Ended);
        }
        else
        {
            State.NextDay();
            EnterPhase(Phase.News);
        }
    }

    public IntelSO FindIntel(string id) =>
        intel == null ? null : System.Array.Find(intel, i => i && i.id == id);

    public void OpenNotebook()
    {
        notebookView.Refresh();
        notebookPanel.SetActive(true);
    }

    public void CloseNotebook() => notebookPanel.SetActive(false);
}
