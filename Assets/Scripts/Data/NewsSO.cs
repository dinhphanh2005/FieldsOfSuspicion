using UnityEngine;

// Một mẩu Tin tức buổi sáng.
// Tin chính: chọn MỘT tin có priority cao nhất trong số tin thỏa điều kiện.
// Tin phụ: mọi tin phụ thỏa điều kiện đều hiện, dưới tin chính.
[CreateAssetMenu(menuName = "FoS/News", fileName = "N0_News")]
public class NewsSO : ScriptableObject
{
    public string headline;
    [TextArea(2, 4)] public string sub;
    public bool isSide;
    public int priority;
    public ShowCondition condition;
    public string param;
}
