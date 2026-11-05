#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class FoSContentCatalogBuilder : IPreprocessBuildWithReport
{
    const string CatalogPath = "Assets/Resources/FoSContentCatalog.asset";
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        var folder = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "Resources");
        var catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(CatalogPath);
        if (!catalog)
        {
            catalog = ScriptableObject.CreateInstance<ContentCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        catalog.days = LoadAll<DaySO>();
        catalog.intel = LoadAll<IntelSO>();
        catalog.cases = LoadAll<CaseSO>();
        catalog.endings = LoadAll<EndingSO>();
        catalog.crops = LoadAll<CropSO>();
        catalog.bgNews = LoadSprite("bg_news");
        catalog.bgBoard = LoadSprite("bg_board");
        catalog.bgShop = LoadSprite("bg_shop");
        catalog.bgNight = LoadSprite("bg_night");
        catalog.bgNotebook = LoadSprite("bg_notebook");
        catalog.bgEnding = LoadSprite("bg_ending");
        catalog.interfaceSprites = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites" })
            .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(sprite => sprite && !AssetDatabase.GetAssetPath(sprite).StartsWith("Assets/Sprites/Characters/"))
            .ToArray();
        catalog.titleFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Lora-Bold.ttf");
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
    }

    static T[] LoadAll<T>() where T : Object => AssetDatabase.FindAssets("t:" + typeof(T).Name)
        .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
        .Where(asset => asset)
        .ToArray();

    static Sprite LoadSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Backgrounds/{name}.png");
}
#endif
