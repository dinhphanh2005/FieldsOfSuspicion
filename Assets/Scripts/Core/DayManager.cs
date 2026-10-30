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
    [SerializeField] GameObject notebookPanel;   // không phải pha, mở đè lên bất kỳ pha nào

    [Header("Labels")]
    [SerializeField] TMP_Text newsTitle;

    public GameState State { get; private set; }
    public Phase CurrentPhase { get; private set; }

    void Start()
    {
        State = new GameState();
        notebookPanel.SetActive(false);
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

        if (phase == Phase.News) newsTitle.text = $"TIN TỨC — {State.DayLabel}";
        if (phase == Phase.Resolve) Resolve();
    }

    // Gắn vào OnClick của mọi nút "Tiếp tục"
    public void EndPhase()
    {
        switch (CurrentPhase)
        {
            case Phase.News:      EnterPhase(Phase.Morning);   break;
            case Phase.Morning:   EnterPhase(Phase.Afternoon); break;
            case Phase.Afternoon: EnterPhase(Phase.Night);     break;
            case Phase.Night:     EnterPhase(Phase.Resolve);   break;
        }
    }
    
    void Resolve()
    {
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

    public void OpenNotebook()  => notebookPanel.SetActive(true);
    public void CloseNotebook() => notebookPanel.SetActive(false);
}