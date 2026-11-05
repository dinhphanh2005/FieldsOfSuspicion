using System;
using System.Collections.Generic;

[Serializable]
public class DropRecord
{
    public int day;
    public string caseId;
    public List<string> answers = new();
    public bool correct;
    public List<string> route = new();

    public bool HasReport => !string.IsNullOrEmpty(caseId);
}

[Serializable]
public class ReportDraft
{
    public string caseId;
    public List<string> answers = new();
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
