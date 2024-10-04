using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Cards
{
    public static Dictionary<string, GameObject> CardPrefabs = new Dictionary<string, GameObject>();
    public static Dictionary<string, CardParams> CardParams = new Dictionary<string, CardParams>();
    
    [RuntimeInitializeOnLoadMethod]
    static void init(){
        GameObject[] cardPrefabs = Resources.LoadAll<GameObject>("CardsPrefabs");
        foreach (GameObject card in cardPrefabs)
        {
            string cardName = card.name;
            CardPrefabs[cardName] = Resources.Load<GameObject>($"CardsPrefabs/{cardName}");
            CardParams[cardName] = Resources.Load<CardParams>($"CardsPrefabs/{cardName}Params");
        }
    }
}