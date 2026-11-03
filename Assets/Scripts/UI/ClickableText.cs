using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Gắn vào một TextMeshPro: bấm vào đoạn <link="F02">...</link> thì báo ra mã "F02".
[RequireComponent(typeof(TMP_Text))]
public class ClickableText : MonoBehaviour, IPointerClickHandler
{
    public event Action<string> LinkClicked;
    TMP_Text text;

    void Awake() => text = GetComponent<TMP_Text>();

    // Dùng cho bản chơi tự động (DevAutoTour): bấm một cụm từ theo mã.
    public void SimulateClick(string id) => LinkClicked?.Invoke(id);

    public void OnPointerClick(PointerEventData e)
    {
        int index = TMP_TextUtilities.FindIntersectingLink(text, e.position, e.pressEventCamera);
        if (index < 0) return;
        LinkClicked?.Invoke(text.textInfo.linkInfo[index].GetLinkID());
    }
}
