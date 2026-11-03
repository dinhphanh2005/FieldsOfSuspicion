using UnityEngine;

// Một mẩu Tin tức buổi sáng.
// Tin chính: chọn MỘT tin có priority cao nhất trong số tin thỏa điều kiện.
// Tin phụ: mọi tin phụ thỏa điều kiện đều hiện, dưới tin chính.
[CreateAssetMenu(menuName = "FoS/News", fileName = "N0_News")]
public class NewsSO : ScriptableObject
{
    public string headline;
    [TextArea(2, 12)] public string sub;
    public bool isSide;
    public int priority;
    public ShowCondition condition;
    public string param;

    [Tooltip("Khi tin này hiện, tự thêm các manh mối này vào sổ tay (cách nhau dấu phẩy)")]
    public string grantsIntel;

    [Tooltip("Lá thư (đọc riêng trên tờ giấy thư, trước tờ báo) thay vì một mục trên báo")]
    public bool isLetter;
    [Tooltip("Dòng nhỏ dưới tiêu đề thư, ví dụ: Không đề tên người gửi")]
    public string letterNote;
}
