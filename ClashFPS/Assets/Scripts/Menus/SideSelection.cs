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
	private Player playerScript;
	private bool showen;

	public IEnumerator Show()
	{
		Debug.Log("Side selection menu opened");

		showen = true;

		blueCount.GetComponent<TextMeshProUGUI>().text = $"{GameManager.Get.GetBluePlayersCount()}/4";
		redCount.GetComponent<TextMeshProUGUI>().text = $"{GameManager.Get.GetRedPlayersCount()}/4";

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

		blueSide.interactable = GameManager.Get.GetBluePlayersCount() < 4;
		redSide.interactable = GameManager.Get.GetRedPlayersCount() < 4;
	}

	public IEnumerator Hide()
	{
		Debug.Log("Side selection menu closed");

		showen = false;

		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;

		for (float t = 1; t > 0; t -= 0.05f)
		{
			canvasGroup.alpha = t;
			yield return new WaitForSeconds(0.01f);
		}

		canvasGroup.alpha = 0;
	}

	public bool IsShowen()
	{
		return showen;
	}

	public void Set(Player playerScript)
	{
		this.playerScript = playerScript;
	}

	public void BlueSide()
	{
		playerScript.SetSide(Side.Blue);
		GameManager.Get.UpdateBluePlayersCountRpc(1);
		StartCoroutine(Hide());
	}

	public void RedSide()
	{
		playerScript.SetSide(Side.Red);
		GameManager.Get.UpdateRedPlayersCountRpc(1);
		StartCoroutine(Hide());
	}
}