using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonFeel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform content;
    public Graphic marker;
    public float nudge = 3f;

    Button button;
    float applied;
    bool hover, down;
    float k, press;

    void Awake() => button = GetComponent<Button>();

    void OnEnable() { hover = down = false; k = press = 0; Apply(); }
    void OnDisable() { k = press = 0; Apply(); }

    bool Live => button == null || button.interactable;

    public void OnPointerEnter(PointerEventData e) { hover = true; }
    public void OnPointerExit(PointerEventData e) { hover = false; down = false; }
    public void OnPointerDown(PointerEventData e) { if (Live) down = true; }
    public void OnPointerUp(PointerEventData e) { down = false; }

    void Update()
    {
        float target = hover && Live ? 1f : 0f;
        float targetPress = down && Live ? 1f : 0f;
        float step = Time.unscaledDeltaTime / 0.1f;
        if (Mathf.Approximately(k, target) && Mathf.Approximately(press, targetPress)) return;
        k = Mathf.MoveTowards(k, target, step);
        press = Mathf.MoveTowards(press, targetPress, step * 1.5f);
        Apply();
    }

    void Apply()
    {
        if (content)
        {
            float ease = k * k * (3 - 2 * k);
            float want = nudge * ease;
            content.anchoredPosition += new Vector2(want - applied, 0);
            applied = want;
        }
        if (marker)
        {
            var c = marker.color; c.a = k; marker.color = c;
        }
        transform.localScale = Vector3.one * (1f - 0.03f * press);
    }
}
