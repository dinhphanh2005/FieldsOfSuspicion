using System;
using System.Collections.Generic;
using UnityEngine;

// Một ô trống trong câu báo cáo. Người chơi chọn một manh mối cùng loại để điền.
[Serializable]
public class CaseSlot
{
    public string label;                        // "Ở đâu", "Khi nào"...
    public ClueKind kind;                       // chỉ manh mối loại này mới ghép được
    public List<string> correct = new();        // mã manh mối được coi là đúng (người chơi không thấy)
}

// Một HỒ SƠ (mẩu tin lớn): câu hỏi Văn phòng cần trả lời + câu báo cáo có ô trống.
// Ví dụ: "Đoàn xe quân nhu sẽ đi qua {0} vào {1}."
// Người chơi tự ghép manh mối → bản báo cáo; game KHÔNG báo đúng/sai.
// Hậu quả hiện ra qua Tin tức sáng hôm sau (DNFTM-style: tự suy luận, tự chịu).
[CreateAssetMenu(menuName = "FoS/Case", fileName = "C0")]
public class CaseSO : ScriptableObject
{
    public string id;                           // "C1"
    public string title;                        // "Đoàn xe quân nhu"
    [TextArea(2, 4)] public string brief;       // Văn phòng muốn biết gì
    [TextArea(1, 3)] public string template;    // câu báo cáo, {0} {1}... là ô trống
    public List<CaseSlot> slots = new();

    [Tooltip("Hồ sơ mở khi sổ tay có MỘT trong các manh mối này (cách nhau dấu phẩy). Trống = mở từ đầu.")]
    public string opensWith;
    [Tooltip("Gửi được từ đêm này...")]
    public int firstNight = 1;
    [Tooltip("...tới hết đêm này (sau đó tin đã cũ)")]
    public int lastNight = 7;

    [Header("Hậu quả lên nghi ngờ khi gửi")]
    public int correctSuspicion;                // gửi đúng: Lenmar bị thiệt, bắt đầu lần theo kẻ báo tin
    public int wrongSuspicion;                  // gửi sai: người Straka sa bẫy, khai ra có kẻ báo tin trong làng
    public SuspicionChannel channel;

    [Tooltip("Hồ sơ cuối (kẻ phản bội) — quyết định kết thúc")]
    public bool isFinal;

    public string SlotLabel(int i) => i >= 0 && i < slots.Count ? slots[i].label : "?";
}
