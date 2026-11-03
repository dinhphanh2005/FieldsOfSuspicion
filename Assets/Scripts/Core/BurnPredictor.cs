using System;

// Bộ suy luận logistic dùng trọng số fit offline. Không điều khiển luật kết thúc trong game.
public static class BurnPredictor
{
    public const int FeatureCount = 4;

    // Thứ tự feature: nghi ngờ cửa sổ, kênh Emel, kênh tuần tra, số lần trễ tiền nhà.
    public static float[] Features(GameState state) => new float[]
    {
        state.SuspicionWindow,
        state.ChannelWindow(SuspicionChannel.Emel),
        state.ChannelWindow(SuspicionChannel.Patrol),
        state.rentWarnings
    };

    public static bool TryPredict(GameState state, float[] weights, float bias, out float probability)
    {
        probability = -1f;
        if (weights == null || weights.Length != FeatureCount) return false;
        var features = Features(state);
        double z = bias;
        for (int i = 0; i < FeatureCount; i++) z += weights[i] * features[i];
        probability = (float)(z >= 0 ? 1.0 / (1.0 + Math.Exp(-z)) : Math.Exp(z) / (1.0 + Math.Exp(z)));
        return true;
    }
}
