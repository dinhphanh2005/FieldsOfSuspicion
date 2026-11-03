// Các enum dùng chung cho dữ liệu nội dung (ScriptableObject).

// Mạch tin trong sổ tay.
// Convoy = Đoàn xe · Granary = Kho thóc · Vaule = Nhà Vaule · Route = Lộ trình
// Traitor = Ai là kẻ phản bội? · Report = B1/B2 (mảnh báo cáo đêm N7)
public enum IntelThread { Convoy, Granary, Vaule, Route, Traitor, Report }

// Người đăng thẻ trên bảng tin. None = thông báo thường của làng.
public enum Handler { None, Rook, Shrike }

// Điều kiện hiện nội dung: tin tức, thẻ bảng tin, biến thể câu thoại.
// "param" đi kèm là mã mảnh tin (F02), mã địa điểm (market) hoặc "F07:4".
// Mã địa điểm bản đồ đêm: home, well, market, church, bridge, river, oak
// CHỈ THÊM VÀO CUỐI — asset lưu enum dưới dạng số thứ tự.
public enum ShowCondition
{
    Always,                 // luôn hiện
    DeliveredLastNight,     // đêm qua đã giao mảnh tin param
    NotDeliveredLastNight,  // đêm qua KHÔNG giao mảnh tin param
    RouteVisitedLastNight,  // lộ trình đêm qua đi qua địa điểm param
    HasFragment,            // sổ tay đã có mảnh tin param
    DeliveredBy,            // param "F07:4" = đã giao F07 vào đêm N4 hoặc sớm hơn
    SuspicionLow,           // mức nghi ngờ (cửa sổ trượt) Thấp
    SuspicionMid,           // Vừa
    SuspicionHigh,          // Cao
}

// Mức rủi ro của một đoạn đường ban đêm. Điểm nghi ngờ: xem SuspicionRules.
public enum RouteRisk { Safe, Low, Medium, High }

// Kênh nghi ngờ: ai đang để ý tới Tomas. Tổng nghi ngờ = tổng có trọng số của các kênh
// (nguyên tắc "nhiều đĩa cùng quay": không có một thanh đo duy nhất).
// Patrol = lính tuần / dân làng (nền). CHỈ THÊM VÀO CUỐI.
public enum SuspicionChannel { Emel, Perrin, Sable, Patrol }

// 5 kết thúc, theo thang ưu tiên: Burned > Evicted > Defection > Extraction > LeftInCold
public enum EndingId { Burned, Evicted, Defection, Extraction, LeftInCold }
