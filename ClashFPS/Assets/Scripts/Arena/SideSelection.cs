using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.UI;


public class SideSelection : MonoBehaviour
{
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private Button blueSide;
	[SerializeField] private Button redSide;
	[SerializeField] private TextMeshProUGUI blueCount;
	[SerializeField] private TextMeshProUGUI redCount;
	private Player _playerScript;

	public IEnumerator Show()
	{
		blueCount.GetComponent<TextMeshProUGUI>().text = $"{GameManager.Get.bluePlayersCount.Value}/4";
		redCount.GetComponent<TextMeshProUGUI>().text = $"{GameManager.Get.redPlayersCount.Value}/4";

		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;

		float timeToOpen = 0.2f;
		float timeStep = 0.01f;
		for (float t = 0; t < 1; t += timeStep / timeToOpen)
		{
			canvasGroup.alpha = t;
			yield return new WaitForSeconds(timeStep);
		}

		canvasGroup.alpha = 1;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;

		blueSide.interactable = GameManager.Get.bluePlayersCount.Value < 4;
		redSide.interactable = GameManager.Get.redPlayersCount.Value < 4;
	}

	public IEnumerator Hide()
	{
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;

		for (float t = 1; t > 0; t -= 0.05f)
		{
			canvasGroup.alpha = t;
			yield return new WaitForSeconds(0.01f);
		}

		canvasGroup.alpha = 0;
	}

	public void Set(Player playerScript)
	{
		_playerScript = playerScript;
	}

	public void BlueSide()
	{
		_playerScript.SetSide(Side.Blue);
		GameManager.Get.bluePlayersCount.Value++;
		StartCoroutine(Hide());
	}

	public void RedSide()
	{
		_playerScript.SetSide(Side.Red);
		GameManager.Get.redPlayersCount.Value++;
		StartCoroutine(Hide());
	}
}