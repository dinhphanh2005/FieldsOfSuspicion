using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CaseSlot
{
    public string label;
    public ClueKind kind;
    public List<string> correct = new();
}

[CreateAssetMenu(menuName = "FoS/Case", fileName = "C0")]
public class CaseSO : ScriptableObject
{
    public string id;
    public string title;
    [TextArea(2, 4)] public string brief;
    [TextArea(1, 3)] public string template;
    public List<CaseSlot> slots = new();

    [Tooltip("Hồ sơ mở khi sổ tay có MỘT trong các manh mối này (cách nhau dấu phẩy). Trống = mở từ đầu.")]
    public string opensWith;
    [Tooltip("Gửi được từ đêm này...")]
    public int firstNight = 1;
    [Tooltip("...tới hết đêm này (sau đó tin đã cũ)")]
    public int lastNight = 7;

    [Header("Hậu quả lên nghi ngờ khi gửi")]
    public int correctSuspicion;
    public int wrongSuspicion;
    public SuspicionChannel channel;

    [Tooltip("Hồ sơ cuối (kẻ phản bội) — quyết định kết thúc")]
    public bool isFinal;

    public string SlotLabel(int i) => i >= 0 && i < slots.Count ? slots[i].label : "?";
}
