using UnityEngine;

[CreateAssetMenu(menuName = "FoS/Board Message", fileName = "N0_Message")]
public class HandlerMessageSO : ScriptableObject
{
    public Handler handler;
    public string title;
    [TextArea(3, 8)] public string body;
    public string signature;

    [Header("Chỉ để tác giả kiểm tra, game không dùng")]
    public string answer;

    [Header("Điều kiện hiện (mặc định: luôn hiện)")]
    public ShowCondition condition;
    public string param;
}
