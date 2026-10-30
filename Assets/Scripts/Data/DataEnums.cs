// Các enum dùng chung cho dữ liệu nội dung (ScriptableObject).

// Mạch tin trong sổ tay.
// Convoy = Đoàn xe · Granary = Kho thóc · Vaule = Nhà Vaule · Route = Lộ trình
// Traitor = Ai là kẻ phản bội? · Report = B1/B2 (mảnh báo cáo đêm N7)
public enum IntelThread { Convoy, Granary, Vaule, Route, Traitor, Report }

// Người đăng thẻ trên bảng tin. None = thông báo thường của làng.
public enum Handler { None, Rook, Shrike }

// Điều kiện hiện nội dung: tin tức, thẻ bảng tin, biến thể câu thoại.
// "param" đi kèm là mã mảnh tin (F02) hoặc mã địa điểm (market).
// Mã địa điểm bản đồ đêm: home, well, market, church, bridge, river, oak
public enum ShowCondition
{
    Always,                 // luôn hiện
    DeliveredLastNight,     // đêm qua đã giao mảnh tin param
    NotDeliveredLastNight,  // đêm qua KHÔNG giao mảnh tin param
    RouteVisitedLastNight,  // lộ trình đêm qua đi qua địa điểm param
}
