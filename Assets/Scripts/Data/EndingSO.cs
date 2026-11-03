using UnityEngine;

// Text của một kết thúc. Điều kiện nằm trong EndingResolver.
[CreateAssetMenu(menuName = "FoS/Ending", fileName = "Ending")]
public class EndingSO : ScriptableObject
{
    public EndingId id;
    public string title;
    [TextArea(5, 12)] public string text;

    [Header("Chỉ dùng cho kết thúc có báo cáo cuối")]
    [TextArea(2, 6)] public string strongEvidence;  // nối thêm khi bằng chứng gửi kèm là đúng
    [TextArea(2, 6)] public string weakEvidence;    // nối thêm khi bằng chứng không chứng minh được
}
