using UnityEngine;

public static class CardTypes
{
    public static GameObject StringToCardPrefab(string cardName){
        return Resources.Load<GameObject>($"{cardName}/{cardName}Card");
    }

    public const string Wizard = "Wizard";
    public const string Valkyrie = "Valkyrie";
    public const string Giant = "Giant";
    
}