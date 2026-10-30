using UnityEngine;

// Một mảnh tin (F01–F12, B1–B2). Nội dung viết sẵn, KHÔNG sửa lúc chạy.
// Người chơi đã ghi mảnh nào thì lưu mã vào GameState.fragments.
[CreateAssetMenu(menuName = "FoS/Intel", fileName = "F00")]
public class IntelSO : ScriptableObject
{
    public string id;                       // "F02" — khớp với {{cụm từ|F02}} trong thoại
    [TextArea(2, 4)] public string text;    // nội dung hiện trong sổ tay
    public IntelThread thread;
    public string source;                   // "Emel · N2"
    public bool isTrue;                     // người chơi không thấy
    public bool onlyLastNight;              // chỉ B1, B2
}
