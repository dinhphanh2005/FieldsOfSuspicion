using System;
using System.Collections.Generic;

// Một đêm đã ra hốc cây sồi. Đêm ở nhà thì KHÔNG có bản ghi.
// caseId rỗng = ra hốc cây nhưng không gửi báo cáo nào.
[Serializable]
public class DropRecord
{
    public int day;                         // đêm của ngày nào
    public string caseId;                   // hồ sơ đã gửi: "C1"
    public List<string> answers = new();    // manh mối người chơi ghép vào từng ô trống
    public bool correct;                    // mọi ô đều đúng (chỉ hiện ra qua hậu quả)
    public List<string> route = new();      // ["home", "river", "oak"]

    public bool HasReport => !string.IsNullOrEmpty(caseId);
}

// Bản nháp báo cáo trong sổ tay: người chơi đang ghép manh mối cho hồ sơ nào.
[Serializable]
public class ReportDraft
{
    public string caseId;
    public List<string> answers = new();    // "" = ô còn trống
}

[Serializable]
public class HandlerOrderResult
{
    public int day;
    public string messageId;
    public Handler handler;
    public string instruction;
    public bool followed;
    public string outcome;
}
