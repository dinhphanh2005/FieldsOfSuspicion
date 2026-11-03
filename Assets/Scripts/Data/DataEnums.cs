// Các enum dùng chung cho dữ liệu nội dung (ScriptableObject).
// Asset lưu enum dưới dạng số thứ tự: nếu thêm giá trị, CHỈ THÊM VÀO CUỐI
// (toàn bộ asset nội dung được sinh lại bằng Tools/StoryGen/gen_story.py).

// Chủ đề của manh mối — dùng để xếp nhóm trong sổ tay.
// Convoy = Đoàn xe · Granary = Kho thóc · Vaule = Nhà Vaule · Route = Đường đi đêm
// Traitor = Kẻ phản bội · Report = (không dùng nữa) · Village = Chuyện trong làng
public enum IntelThread { Convoy, Granary, Vaule, Route, Traitor, Report, Village }

// Loại manh mối — quyết định manh mối được ghép vào ô trống nào của hồ sơ.
// Detail = chi tiết bối cảnh, chỉ để đọc và suy luận, không ghép vào ô trống.
public enum ClueKind { Who, What, Where, When, State, Evidence, Detail }

// Người đăng thẻ trên bảng tin. None = thông báo thường của làng.
public enum Handler { None, Rook, Shrike }

// Điều kiện hiện nội dung: tin tức, thẻ bảng tin, biến thể câu thoại.
// "param" là mã hồ sơ (C1), mã manh mối (K02), mã địa điểm (market) hoặc "C3:4".
// Mã địa điểm bản đồ đêm: home, well, market, church, bridge, river, oak
public enum ShowCondition
{
    Always,                 // luôn hiện
    DeliveredLastNight,     // đêm qua đã gửi hồ sơ param (đúng hay sai đều tính)
    NotDeliveredLastNight,  // đêm qua KHÔNG gửi hồ sơ param
    RouteVisitedLastNight,  // lộ trình đêm qua đi qua địa điểm param
    HasFragment,            // sổ tay đã có manh mối param
    DeliveredBy,            // "C3:4" = đã gửi hồ sơ C3 ĐÚNG vào đêm N4 hoặc sớm hơn
    SuspicionLow,           // mức nghi ngờ (cửa sổ trượt) Thấp
    SuspicionMid,           // Vừa
    SuspicionHigh,          // Cao
    CorrectLastNight,       // đêm qua gửi hồ sơ param và ghép đúng
    WrongLastNight,         // đêm qua gửi hồ sơ param nhưng ghép sai
    CaseCorrect,            // đã từng gửi hồ sơ param và ghép đúng
    CaseNeverDelivered,     // chưa từng gửi hồ sơ param
}

// Mức rủi ro của một đoạn đường ban đêm. Điểm nghi ngờ: xem SuspicionRules.
public enum RouteRisk { Safe, Low, Medium, High }

// Kênh nghi ngờ: ai đang để ý tới Tomas. Tổng nghi ngờ = tổng có trọng số của các kênh
// (nguyên tắc "nhiều đĩa cùng quay": không có một thanh đo duy nhất).
public enum SuspicionChannel { Emel, Perrin, Sable, Patrol }

// 5 kết thúc, theo thang ưu tiên: Burned > Evicted > Defection > Extraction > LeftInCold
public enum EndingId { Burned, Evicted, Defection, Extraction, LeftInCold }
