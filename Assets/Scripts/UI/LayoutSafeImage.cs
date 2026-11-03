using UnityEngine.UI;

// Image dùng cho mọi khung/nút dựng bằng code.
// Image gốc báo "kích thước ưa thích" = kích thước ảnh gốc (vd. tờ báo 1400px, nút 400px).
// Khi khung có VerticalLayoutGroup + ContentSizeFitter, Unity lấy số lớn nhất → khung cao vọt,
// rồi FitHeight thu nhỏ cả khung → chữ tí hon; nút trên thanh dưới thì rộng theo ảnh, to nhỏ thất thường.
// Hạ ưu tiên layout xuống -1: LayoutGroup / LayoutElement trên cùng object luôn được tính trước.
public class LayoutSafeImage : Image
{
    public override int layoutPriority => -1;
}
