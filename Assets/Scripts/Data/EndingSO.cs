using UnityEngine;

// Text của một kết thúc. Điều kiện nằm trong EndingResolver.
[CreateAssetMenu(menuName = "FoS/Ending", fileName = "Ending")]
public class EndingSO : ScriptableObject
{
    public EndingId id;
    public string title;
    [TextArea(5, 12)] public string text;
}
