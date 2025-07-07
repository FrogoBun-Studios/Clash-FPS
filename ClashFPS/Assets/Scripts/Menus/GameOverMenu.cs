using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;


public class GameOverMenu : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI playerNameTemplate;
	[SerializeField] private Transform playerNamesGroup;
	[SerializeField] private TextMeshProUGUI playerScoreTemplate;
	[SerializeField] private Transform playerScoresGroup;
	[SerializeField] private Button playAgainButton;
	[SerializeField] private TextMeshProUGUI winnerText;

	public void Show(Dictionary<string, float> scores, Side winner, bool tie)
	{
		if (tie)
		{
			winnerText.text = "Tie!";
		}
		else
		{
			if (winner == Side.Blue)
			{
				winnerText.text = "Blu Side Won!";
				winnerText.color = new Color(48, 48, 192);
			}
			else
			{
				winnerText.text = "Red Side Won!";
				winnerText.color = new Color(192, 48, 48);
			}
		}

		int i = 0;
		foreach (string name in scores.Keys)
		{
			TextMeshProUGUI playerName = Instantiate(playerNameTemplate, playerNamesGroup);
			TextMeshProUGUI playerScore = Instantiate(playerScoreTemplate, playerScoresGroup);

			playerName.text = name;
			playerScore.text = Mathf.Round(scores[name]).ToString();
			playerName.rectTransform.anchoredPosition = new Vector2(0, -37.5f - (75 + 15) * i);
			playerScore.rectTransform.anchoredPosition = new Vector2(0, -37.5f - (75 + 15) * i);
			Debug.Log($"Showing score {name}: {scores[name]}, (0, {-37.5f - (75 + 15) * i})");

			i++;
		}

		Destroy(playerNameTemplate.gameObject);
		Destroy(playerScoreTemplate.gameObject);
	}

	public void PlayAgain()
	{
		GameManager.Get.PlayAgainServerRpc();
		playAgainButton.interactable = false;
	}

	public void Leave()
	{
		GameManager.Get.GetThisPlayer().LeaveGame();
	}
}