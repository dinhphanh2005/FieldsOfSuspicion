using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Màn kết thúc: tấm thẻ tối giữa màn hình — tên kết thúc, đoạn kết, vài con số. Nút "Chơi lại" ở thanh dưới.
public class EndingView : MonoBehaviour
{
    TextMeshProUGUI title, body, stats;
    RectTransform card;

    public void Init()
    {
        var stage = UIFactory.MakeStage(transform);

        card = UIFactory.MakeRect(stage, "Ending_Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        card.sizeDelta = new Vector2(860, 0);
        UIFactory.MakeSurface(card, new Color(0.11f, 0.09f, 0.08f, 0.95f), 10);
        UIFactory.Skin(card.GetComponent<UnityEngine.UI.Image>(), "panel_ending");
        UIFactory.AddShadow(card, 10);
        UIFactory.AutoHeight(card, 56, 44, 40, 0);

        title = UIFactory.AddLine(card, "", 38, UIFactory.Gold, TextAlignmentOptions.Center, title: true);
        title.characterSpacing = 4;
        UIFactory.Size(UIFactory.MakeRect(card, "Gap", Vector2.zero, Vector2.one), 18);
        UIFactory.AddRule(card, UIFactory.FrameLine, 1);
        UIFactory.Size(UIFactory.MakeRect(card, "Gap", Vector2.zero, Vector2.one), 22);
        body = UIFactory.AddLine(card, "", 19, UIFactory.Light);
        body.lineSpacing = 10;
        UIFactory.Size(UIFactory.MakeRect(card, "Gap", Vector2.zero, Vector2.one), 26);
        stats = UIFactory.AddLine(card, "", 15, UIFactory.Hex("9C8C7B"));
        stats.lineSpacing = 6;

        var bar = UIFactory.MakeActionBar(transform);
        UIFactory.BarSpacer(bar);
        UIFactory.BarButton(bar, "Chơi lại", Replay, primary: true);
    }

    public void Show(EndingSO ending, GameState s)
    {
        title.text = ending ? ending.title.ToUpper() : "KẾT THÚC";
        body.text = ending ? ending.text : "(Thiếu EndingSO cho kết thúc này)";
        stats.text = $"Hồ sơ thật: {s.playerName} · kết thúc ở N{s.day} · Nghi ngờ 3 đêm cuối: {s.SuspicionWindow} (cộng dồn {s.SuspicionTotal})\n" +
                     $"Mảnh tin đã ghi: {s.fragments.Count} · Đã giao: {s.deliveredDrops.Count} · Tiền còn: {s.money} xu";
        UIFactory.FitHeight(card, 580);
    }

    void Replay() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
