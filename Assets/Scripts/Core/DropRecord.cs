using System;
using System.Collections.Generic;

// Một đêm đã đi giao tin. Đêm ở nhà thì KHÔNG có bản ghi.
[Serializable]
public class DropRecord
{
    public int day;                         // đêm của ngày nào
    public string intelId;                  // "F02"
    public List<string> route = new();      // ["home", "river", "oak"]
    public DeliveryMode mode = DeliveryMode.Truthful;
}

public enum DeliveryMode { Truthful, Scrambled, False }

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

public enum IntelTruthStatus { Unverified, Confirmed, Contradicted, Closed }

[Serializable]
public class IntelEvidenceState
{
    public string intelId;
    public IntelTruthStatus truthStatus;
    public bool consequenceSpreading;
}
