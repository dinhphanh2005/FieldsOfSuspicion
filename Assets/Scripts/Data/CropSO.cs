using UnityEngine;

[CreateAssetMenu(menuName = "FoS/Crop", fileName = "Crop_Carrot")]
public class CropSO : ScriptableObject
{
    public string id = "carrot";
    public string displayName = "Cà rốt";
    [Min(1)] public int growDays = EconomyRules.CarrotGrowDays;
    [Min(1)] public int harvestYield = EconomyRules.CarrotYield;
}
