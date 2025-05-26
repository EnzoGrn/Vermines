using UnityEngine;
using UnityEngine.Localization;
using Vermines.ShopSystem.Enumerations;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "UI/ShopConfig", order = 0)]
public class ShopUIConfig : ScriptableObject
{
    public ShopType shopType;

    public string shopName;
    public string shopDescription;

    public string leftDialogue;
    public string rightDialogue;

    // TODO: Use LocalizedString for localization

    //public LocalizedString shopName;
    //public LocalizedString shopDescription;
    //public LocalizedString leftDialogue;
    //public LocalizedString rightDialogue;

    public Sprite portraitLeft;
    public bool flipLeft;
    public Sprite portraitRight;
    public bool flipRight;
}
