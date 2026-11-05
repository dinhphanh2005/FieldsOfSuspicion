using UnityEngine;

[CreateAssetMenu(menuName = "FoS/Customer", fileName = "Customer")]
public class CustomerSO : ScriptableObject
{
    public string displayName;
    public Sprite portrait;

    [Tooltip("Hỏi dồn khách này thì nghi ngờ cộng vào kênh nào")]
    public SuspicionChannel channel;
}
