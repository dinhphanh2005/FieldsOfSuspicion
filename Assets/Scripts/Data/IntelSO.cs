using UnityEngine;

// Một MANH MỐI (mẩu tin nhỏ): K01, K02... Nội dung viết sẵn, KHÔNG sửa lúc chạy.
// Người chơi bấm cụm từ tô màu trong lời thoại để ghi manh mối vào sổ tay,
// rồi tự ghép các manh mối vào ô trống của một hồ sơ (CaseSO) để thành bản báo cáo.
// Đã ghi manh mối nào thì GameState.fragments lưu mã của nó.
[CreateAssetMenu(menuName = "FoS/Clue", fileName = "K00")]
public class IntelSO : ScriptableObject
{
    public string id;                       // "K02" — khớp với {{cụm từ|K02}} trong thoại
    public string keyword;                  // nhãn ngắn hiện trên chip và trong câu báo cáo: "cầu Bắc"
    [TextArea(2, 4)] public string text;    // ghi chú đầy đủ trong sổ tay: ai nói, nói gì
    public ClueKind kind;                   // ghép được vào ô trống loại nào
    public IntelThread thread;              // nhóm trong sổ tay
    public string source;                   // "Emel · thứ Hai"

    [Header("Chỉ để tác giả kiểm tra, người chơi không thấy")]
    public bool isTrue = true;
}
