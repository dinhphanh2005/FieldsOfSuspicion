using UnityEngine;

// Một thẻ trên bảng tin: lệnh mật mã của Rook/Shrike hoặc thông báo thường của làng.
// Dùng chung một kiểu để bảng tin trộn lẫn dễ dàng.
[CreateAssetMenu(menuName = "FoS/Board Message", fileName = "N0_Message")]
public class HandlerMessageSO : ScriptableObject
{
    public Handler handler;                 // None = thông báo thường
    public string title;
    [TextArea(3, 8)] public string body;
    public string signature;                // "Liên hệ: bà Rosa, xóm Bắc"

    [Header("Chỉ để tác giả kiểm tra, game không dùng")]
    public string answer;                   // "GIAO XE"

    [Header("Điều kiện hiện (mặc định: luôn hiện)")]
    public ShowCondition condition;
    public string param;
}
