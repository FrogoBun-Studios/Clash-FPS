using UnityEngine;

public static class CardTypes
{
    public const string Wizard = "Wizard";
    public const string Valkyrie = "Valkyrie";

    public static GameObject StringToCardPrefab(string cardName){
        return Resources.Load<GameObject>($"{cardName}/{cardName}Card");
    }
}