using UnityEngine;

public enum ShopPriceLevel { Low, Fair, High }

public static class PricingSystem
{
    public static int Revenue(int basePrice, ShopPriceLevel level, int customerMomentum)
    {
        if (basePrice <= 0) return 0;
        float factor = level == ShopPriceLevel.Low ? 0.8f : level == ShopPriceLevel.High ? 1.2f : 1f;
        int regularBonus = Mathf.Clamp(customerMomentum, 0, 3);
        return Mathf.Max(0, Mathf.RoundToInt(basePrice * factor) + regularBonus);
    }

    public static int MomentumAfterDay(int currentMomentum, ShopPriceLevel level, int sales)
    {
        int change = level switch
        {
            ShopPriceLevel.Low => 2,
            ShopPriceLevel.Fair => sales > 0 ? 1 : 0,
            _ => -2
        };
        return Mathf.Clamp(currentMomentum + change, 0, 5);
    }

    public static string Name(ShopPriceLevel level) => level switch
    {
        ShopPriceLevel.Low => "Giá mềm",
        ShopPriceLevel.High => "Giá cao",
        _ => "Giá vừa"
    };
}
