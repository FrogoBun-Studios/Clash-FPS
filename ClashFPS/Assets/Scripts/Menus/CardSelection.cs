using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;


public class CardSelection : MonoBehaviour
{
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private Button leftCardButton;
	[SerializeField] private Button middleCardButton;
	[SerializeField] private Button rightCardButton;
	[SerializeField] private TextMeshProUGUI waitText;
	[SerializeField] private Transform elixirText;
	private string leftCardName;
	private string middleCardName;
	private Player playerScript;
	private string rightCardName;
	private bool showen;

	public void Show(float delay)
	{
		NetworkQuery.Instance.Request<float>(
			$"Get Elixir {playerScript.GetComponent<NetworkObject>().OwnerClientId}",
			elixir => StartCoroutine(ShowI(delay, elixir)));
	}

	public IEnumerator ShowI(float delay, float elixir)
	{
		showen = true;

		elixirText.GetChild(0).GetComponent<TextMeshProUGUI>().text = Mathf.FloorToInt(elixir).ToString();
		elixirText.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mathf.FloorToInt(elixir).ToString();

		PutCards();

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

		timeToOpen = delay - timeToOpen;
		for (float t = 0; t < timeToOpen; t += Time.deltaTime)
		{
			waitText.text = $"Respawn In {Mathf.Ceil(timeToOpen - t)} Seconds...";
			yield return null;
		}

		waitText.text = "Respawn Now";

		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;

		leftCardButton.interactable = Cards.CardParams[leftCardName].elixir <= elixir;
		middleCardButton.interactable = Cards.CardParams[middleCardName].elixir <= elixir;
		rightCardButton.interactable = Cards.CardParams[rightCardName].elixir <= elixir;
	}

	public IEnumerator Hide()
	{
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

	private void PutCards()
	{
		List<string> cards = Cards.CardParams.Keys.ToList();

		leftCardName = cards[Random.Range(0, cards.Count)];
		cards.Remove(leftCardName);

		middleCardName = cards[Random.Range(0, cards.Count)];
		cards.Remove(middleCardName);

		rightCardName = cards[Random.Range(0, cards.Count)];
		cards.Remove(rightCardName);

		leftCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = leftCardName;
		leftCardButton.transform.GetChild(0).GetComponent<RawImage>().texture =
			Cards.CardParams[leftCardName].cardImage;
		leftCardButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[leftCardName].elixir.ToString();
		leftCardButton.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[leftCardName].elixir.ToString();

		middleCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = middleCardName;
		middleCardButton.transform.GetChild(0).GetComponent<RawImage>().texture =
			Cards.CardParams[middleCardName].cardImage;
		middleCardButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[middleCardName].elixir.ToString();
		middleCardButton.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[middleCardName].elixir.ToString();

		rightCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = rightCardName;
		rightCardButton.transform.GetChild(0).GetComponent<RawImage>().texture =
			Cards.CardParams[rightCardName].cardImage;
		rightCardButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[rightCardName].elixir.ToString();
		rightCardButton.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[rightCardName].elixir.ToString();
	}

	public void LeftCard()
	{
		playerScript.UpdateElixirServerRpc(-Cards.CardParams[leftCardName].elixir);
		playerScript.ChooseCardServerRpc(leftCardName);
		StartCoroutine(Hide());
	}

	public void MiddleCard()
	{
		playerScript.UpdateElixirServerRpc(-Cards.CardParams[middleCardName].elixir);
		playerScript.ChooseCardServerRpc(middleCardName);
		StartCoroutine(Hide());
	}

	public void RightCard()
	{
		playerScript.UpdateElixirServerRpc(-Cards.CardParams[rightCardName].elixir);
		playerScript.ChooseCardServerRpc(rightCardName);
		StartCoroutine(Hide());
	}

	public void FreeCard()
	{
		playerScript.ChooseCardServerRpc("Wizard");
		StartCoroutine(Hide());
	}
}