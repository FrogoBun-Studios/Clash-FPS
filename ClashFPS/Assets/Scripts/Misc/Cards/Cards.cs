using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Cards
{
    public static Dictionary<string, GameObject> CardPrefabs = new Dictionary<string, GameObject>();
    public static Dictionary<string, CardParams> CardParams = new Dictionary<string, CardParams>();
    
    [RuntimeInitializeOnLoadMethod]
    static void init(){
        string path = Path.Combine(Application.dataPath, "Resources/Cards");
        string[] cardFolders = Directory.GetDirectories(path);

        foreach (string folderPath in cardFolders)
        {
            string folderName = Path.GetFileName(folderPath);
            string resourcePath = $"Cards/{folderName}/{folderName}Card";
            GameObject cardPrefab = Resources.Load<GameObject>(resourcePath);
            CardPrefabs[folderName] = cardPrefab;

            resourcePath = $"Cards/{folderName}/{folderName}Params";
            CardParams[folderName] = Resources.Load<CardParams>(resourcePath);
        }
    }
}