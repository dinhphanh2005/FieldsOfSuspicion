using System.Collections.Generic;

public static class HandlerOrders
{
    static readonly Dictionary<string, string> Meanings = new Dictionary<string, string>
    {
        { "MO TIEM",     "chiều nay mở tiệm bán hàng." },
        { "LANG NGHE",   "chiều nay ghi ít nhất một manh mối vào sổ." },
        { "GIAO XE",     "đêm nay gửi hồ sơ \"Đoàn xe quân nhu\"." },
        { "GIAO KHO",    "đêm nay gửi hồ sơ \"Kho thóc phía Tây\"." },
        { "THEO VAULE",  "hôm nay ghi một manh mối về nhà Vaule." },
        { "QUA CHO",     "đêm nay đi đường vòng qua chợ." },
        { "GIAO VAULE",  "đêm nay gửi hồ sơ \"Vị khách nhà Vaule\"." },
        { "BO VAULE",    "đừng gửi hồ sơ nhà Vaule đêm nay." },
        { "GIU YEN",     "đêm nay ở nhà, không ra hốc cây." },
        { "QUA CAU",     "đêm nay đi đường qua cầu Bắc." },
        { "MAI BAO TEN", "tối mai gửi báo cáo cuối, nêu tên kẻ phản bội." },
        { "ROOK PHAN",   "báo cáo cuối hãy tố Rook là kẻ phản bội." },
        { "DEM CUOI",    "đêm nay gửi báo cáo cuối." },
        { "BAO ROOK",    "đêm nay tố Rook trong báo cáo cuối." },
    };

    public static string Key(string answer) => (answer ?? "").Trim().ToUpperInvariant();

    public static string Meaning(string answer) =>
        Meanings.TryGetValue(Key(answer), out var m) ? m : "";
}
