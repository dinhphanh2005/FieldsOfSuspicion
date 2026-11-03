using System;
using UnityEngine;

// Luật bản đồ đêm của một ngày, ghi đè lên bảng đường gốc (mục 9 PDF).
// Ví dụ: ngập ven sông (closed), lính canh cầu (risk = High),
// rủi ro ẩn chỉ lộ khi sổ tay có mảnh tin hiddenUntil (Cầu Bắc + F08).
[Serializable]
public class RouteRule
{
    public string a;                    // mã địa điểm: home, well, market, church, bridge, river, oak
    public string b;
    public RouteRisk risk;
    public bool closed;                 // không đi được đêm nay

    [Tooltip("Để trống = người chơi thấy ngay. Có mã (F08) = chỉ thấy rủi ro thật khi sổ tay có mảnh tin này")]
    public string hiddenUntil;

    [TextArea(1, 2)] public string note; // hiện ở khung bên phải bản đồ (khi người chơi biết)
}
