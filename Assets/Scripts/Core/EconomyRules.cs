public static class EconomyRules
{
    // Năng lượng dùng chung cho hỏi chuyện buổi chiều và từng bước đi buổi tối: hỏi nhiều thì tối phải chọn đường ngắn hơn.
    public const int MaxEnergy = 10;
    public const int HarvestYield = 3;
    public const int OpenShopEnergy = 2;
    public const int TalkEnergy = 1;
    public const int StepEnergy = 1;
    public const int OverexertSuspicion = 2;

    public const int StartMoney = 20;
    public const int StartStock = 4;
    public const int Rent = 12;
    public const int MaxWarnings = 3;
    public const int StartingPlotCount = 4;
    public const int CarrotGrowDays = 3;
    public const int CarrotYield = HarvestYield;

    static readonly string[] Letters =
    {
        $"Cậu Tomas, đêm qua cậu chưa trả {Rent} xu tiền nhà. Tôi biết mùa đầu ai cũng khó, nhưng tôi cũng phải sống. Bà Mabry.",
        $"Cậu Tomas, đây là lần thứ hai cậu thiếu {Rent} xu tiền nhà. Tôi không muốn phải nói chuyện này trước mặt hàng xóm. Bà Mabry.",
        $"Cậu Tomas, lần cuối. Tiền nhà {Rent} xu mỗi đêm, nếu cậu còn thiếu thêm một đêm nữa, tôi sẽ cho người khác thuê. Bà Mabry.",
    };

    public static string RentLetter(int n) => Letters[System.Math.Clamp(n, 1, Letters.Length) - 1];
}
