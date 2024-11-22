using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.UI;


public class CardSelection : MonoBehaviour
{
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private Button leftCardButton;
	[SerializeField] private Button middleCardButton;
	[SerializeField] private Button rightCardButton;
	private string _leftCardName;
	private string _middleCardName;

	private Player _playerScript;
	private string _rightCardName;

	public IEnumerator Show()
	{
		PutCards();

		for (float t = 0; t < 1; t += 0.05f)
		{
			canvasGroup.alpha = t;
			yield return new WaitForSeconds(0.01f);
		}

		canvasGroup.alpha = 1;
	}

	public IEnumerator Hide()
	{
		for (float t = 1; t > 0; t -= 0.05f)
		{
			canvasGroup.alpha = t;
			yield return new WaitForSeconds(0.01f);
		}

		canvasGroup.alpha = 0;
	}

	public void SetPlayerScript(Player playerScript)
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
		_playerScript.ChooseCard(_leftCardName);

		StartCoroutine(Hide());
	}

	public void MiddleCard()
	{
		_playerScript.ChooseCard(_middleCardName);

		StartCoroutine(Hide());
	}

	public void RightCard()
	{
		_playerScript.ChooseCard(_rightCardName);

		StartCoroutine(Hide());
	}
}