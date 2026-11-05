using System;
using UnityEngine;

[Serializable]
public class RouteRule
{
    public string a;
    public string b;
    public RouteRisk risk;
    public bool closed;

    [Tooltip("Để trống = người chơi thấy ngay. Có mã (F08) = chỉ thấy rủi ro thật khi sổ tay có mảnh tin này")]
    public string hiddenUntil;

    [TextArea(1, 2)] public string note;
}
