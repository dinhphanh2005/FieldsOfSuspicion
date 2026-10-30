using UnityEngine;

// Một khách của tiệm. Lời thoại theo từng ngày nằm trong DaySO.visits.
[CreateAssetMenu(menuName = "FoS/Customer", fileName = "Customer")]
public class CustomerSO : ScriptableObject
{
    public string displayName;
    public Sprite portrait;
}
