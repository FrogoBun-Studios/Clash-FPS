using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TMPro;

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
	private string _leftCardName;
	private string _middleCardName;
	private Player _playerScript;
	private string _rightCardName;
	private bool _showen;

	public IEnumerator Show(float delay)
	{
		_showen = true;

		elixirText.GetChild(0).GetComponent<TextMeshProUGUI>().text = _playerScript.GetElixir().ToString();
		elixirText.GetChild(1).GetComponent<TextMeshProUGUI>().text = _playerScript.GetElixir().ToString();

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

		leftCardButton.interactable = Cards.CardParams[_leftCardName].elixir <= _playerScript.GetElixir();
		middleCardButton.interactable = Cards.CardParams[_middleCardName].elixir <= _playerScript.GetElixir();
		rightCardButton.interactable = Cards.CardParams[_rightCardName].elixir <= _playerScript.GetElixir();
	}

	public IEnumerator Hide()
	{
		_showen = false;

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
		return _showen;
	}

	public void Set(Player playerScript)
	{
		_playerScript = playerScript;
	}

	private void PutCards()
	{
		List<string> cards = Cards.CardParams.Keys.ToList();

		_leftCardName = cards[Random.Range(0, cards.Count)];
		cards.Remove(_leftCardName);

		_middleCardName = cards[Random.Range(0, cards.Count)];
		cards.Remove(_middleCardName);

		_rightCardName = cards[Random.Range(0, cards.Count)];
		cards.Remove(_rightCardName);

		leftCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = _leftCardName;
		leftCardButton.transform.GetChild(0).GetComponent<RawImage>().texture =
			Cards.CardParams[_leftCardName].cardImage;
		leftCardButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[_leftCardName].elixir.ToString();
		leftCardButton.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[_leftCardName].elixir.ToString();

		middleCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = _middleCardName;
		middleCardButton.transform.GetChild(0).GetComponent<RawImage>().texture =
			Cards.CardParams[_middleCardName].cardImage;
		middleCardButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[_middleCardName].elixir.ToString();
		middleCardButton.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[_middleCardName].elixir.ToString();

		rightCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = _rightCardName;
		rightCardButton.transform.GetChild(0).GetComponent<RawImage>().texture =
			Cards.CardParams[_rightCardName].cardImage;
		rightCardButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[_rightCardName].elixir.ToString();
		rightCardButton.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text =
			Cards.CardParams[_rightCardName].elixir.ToString();
	}

	public void LeftCard()
	{
		_playerScript.SpendElixir(Cards.CardParams[_leftCardName].elixir);
		StartCoroutine(_playerScript.ChooseCard(_leftCardName));
		StartCoroutine(Hide());
	}

	public void MiddleCard()
	{
		_playerScript.SpendElixir(Cards.CardParams[_middleCardName].elixir);
		StartCoroutine(_playerScript.ChooseCard(_middleCardName));
		StartCoroutine(Hide());
	}

	public void RightCard()
	{
		_playerScript.SpendElixir(Cards.CardParams[_rightCardName].elixir);
		StartCoroutine(_playerScript.ChooseCard(_rightCardName));
		StartCoroutine(Hide());
	}

	public void FreeCard()
	{
		StartCoroutine(_playerScript.ChooseCard("Wizard"));
		StartCoroutine(Hide());
	}
}