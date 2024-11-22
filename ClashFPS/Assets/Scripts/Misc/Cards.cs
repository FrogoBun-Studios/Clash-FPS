using System.Collections.Generic;

using UnityEngine;


public static class Cards
{
	public static Dictionary<string, GameObject> CardPrefabs = new();
	public static Dictionary<string, CardParams> CardParams = new();

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		GameObject[] cardPrefabs = Resources.LoadAll<GameObject>("CardsPrefabs");
		foreach (GameObject card in cardPrefabs)
		{
			string cardName = card.name;
			CardPrefabs[cardName] = Resources.Load<GameObject>($"CardsPrefabs/{cardName}");
			CardParams[cardName] = Resources.Load<CardParams>($"CardsPrefabs/{cardName}Params");
		}
	}
}