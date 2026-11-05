using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FoS/Day", fileName = "N0")]
public class DaySO : ScriptableObject
{
    [Tooltip("1..7, N1 = Chủ Nhật")]
    public int day;

    [Tooltip("Tin tức sáng nay (tin chính + tin phụ)")]
    public List<NewsSO> news = new();

    [Tooltip("Thẻ trên bảng tin buổi sáng, theo thứ tự hiện")]
    public List<HandlerMessageSO> board = new();

    [Tooltip("Khách ghé tiệm buổi chiều, theo thứ tự")]
    public List<CustomerVisit> visits = new();

    [Tooltip("Luật bản đồ đêm của ngày này (ghi đè bảng đường gốc)")]
    public List<RouteRule> night = new();
}
