using UnityEngine;

[CreateAssetMenu(menuName = "FoS/Ending", fileName = "Ending")]
public class EndingSO : ScriptableObject
{
    public EndingId id;
    public string title;
    [TextArea(5, 12)] public string text;

    [Header("Chỉ dùng cho kết thúc có báo cáo cuối")]
    [TextArea(2, 6)] public string strongEvidence;
    [TextArea(2, 6)] public string weakEvidence;
}
