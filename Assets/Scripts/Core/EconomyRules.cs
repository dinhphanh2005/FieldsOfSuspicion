// Tiền, rau, năng lượng, tiền nhà. Số tạm — chỉnh khi playtest.
public static class EconomyRules
{
    public const int MaxEnergy = 10;        // hồi đầy mỗi sáng
    public const int MaxActionPoints = 4;   // điểm thao tác ruộng mỗi sáng
    public const int HarvestEnergy = 3;     // giữ tương thích bản lưu cũ; ruộng dùng AP
    public const int HarvestYield = 4;      // giữ tương thích bản lưu cũ
    public const int OpenShopEnergy = 4;    // mở tiệm buổi chiều
    public const int StepEnergy = 1;        // mỗi bước đi ban đêm

    public const int StartMoney = 20;
    public const int StartStock = 4;        // bó cà rốt có sẵn ngày đầu
    public const int Rent = 12;             // trừ ở Tổng kết mỗi đêm
    public const int MaxWarnings = 3;       // 3 thư nhắc; thiếu lần thứ 4 → Bị đuổi nhà
    public const int StartingPlotCount = 6;
    public const int CarrotGrowDays = 3;
    public const int CarrotYield = HarvestYield;
    public const int PlantActionCost = 1;
    public const int HarvestActionCost = 1;

    static readonly string[] Letters =
    {
        "Cậu Tomas, đêm qua cậu chưa trả tiền nhà. Tôi biết mùa đầu ai cũng khó, nhưng tôi cũng phải sống. Bà Mabry.",
        "Cậu Tomas, đây là lần thứ hai. Tôi không muốn phải nói chuyện này trước mặt hàng xóm. Bà Mabry.",
        "Cậu Tomas, lần cuối. Nếu cậu còn thiếu thêm một đêm nữa, tôi sẽ cho người khác thuê. Bà Mabry.",
    };

    // n = số lần đã thiếu (1..3)
    public static string RentLetter(int n) => Letters[System.Math.Clamp(n, 1, Letters.Length) - 1];
}
