using UnityEngine;

[CreateAssetMenu(menuName = "FoS/Content Catalog", fileName = "FoSContentCatalog")]
public class ContentCatalog : ScriptableObject
{
    public DaySO[] days;
    public IntelSO[] intel;
    public CaseSO[] cases;
    public EndingSO[] endings;
    public CropSO[] crops;
    public Sprite bgNews, bgBoard, bgShop, bgNight, bgNotebook, bgEnding;
    public Sprite[] interfaceSprites;
    public Font titleFont;
}
