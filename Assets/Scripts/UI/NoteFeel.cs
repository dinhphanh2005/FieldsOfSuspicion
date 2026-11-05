using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NoteFeel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    float baseTilt, k;
    bool hover;
    Shadow shadow;
    Button button;

    void Awake()
    {
        baseTilt = transform.localEulerAngles.z;
        if (baseTilt > 180) baseTilt -= 360;
        shadow = GetComponent<Shadow>();
        button = GetComponent<Button>();
    }

    void OnDisable() { hover = false; k = 0; Apply(); }

    public void OnPointerEnter(PointerEventData e) => hover = true;
    public void OnPointerExit(PointerEventData e) => hover = false;

    void Update()
    {
        float target = hover && (!button || button.interactable) ? 1 : 0;
        if (Mathf.Approximately(k, target)) return;
        k = Mathf.MoveTowards(k, target, Time.unscaledDeltaTime / 0.12f);
        Apply();
    }

    void Apply()
    {
        float e = k * k * (3 - 2 * k);
        transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(baseTilt, 0, e));
        transform.localScale = Vector3.one * (1 + 0.04f * e);
        if (shadow) shadow.effectDistance = new Vector2(2.4f + 3 * e, -6 - 6 * e);
    }
}
