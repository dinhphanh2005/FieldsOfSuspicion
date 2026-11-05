using UnityEngine;

[CreateAssetMenu(menuName = "FoS/Clue", fileName = "K00")]
public class IntelSO : ScriptableObject
{
    public string id;
    public string keyword;
    [TextArea(2, 4)] public string text;
    public ClueKind kind;
    public IntelThread thread;
    public string source;

    [Header("Chỉ để tác giả kiểm tra, người chơi không thấy")]
    public bool isTrue = true;
}
