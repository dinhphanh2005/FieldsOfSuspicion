using System;
using System.Collections.Generic;

// Một đêm đã đi giao tin. Đêm ở nhà thì KHÔNG có bản ghi.
[Serializable]
public class DropRecord
{
    public int day;                         // đêm của ngày nào
    public string intelId;                  // "F02"
    public List<string> route = new();      // ["home", "river", "oak"]
}
