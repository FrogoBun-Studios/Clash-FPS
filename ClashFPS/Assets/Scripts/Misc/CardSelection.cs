using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSelection : MonoBehaviour
{
    [SerializeField] private CanvasGroup CanvasGroup;
    [SerializeField] private Button LeftCardButton;
    [SerializeField] private Button MiddleCardButton;
    [SerializeField] private Button RightCardButton;

    private Player PlayerScript;
    private string LeftCardName;
    private string MiddleCardName;
    private string RightCardName;

    public IEnumerator Show(){
        PutCards();

        for(float t = 0; t < 1; t += 0.05f){
            CanvasGroup.alpha = t;
            yield return new WaitForSeconds(0.01f);
        }

        CanvasGroup.alpha = 1;
    }

    public IEnumerator Hide(){
        for(float t = 1; t > 0; t -= 0.05f){
            CanvasGroup.alpha = t;
            yield return new WaitForSeconds(0.01f);
        }
        
        CanvasGroup.alpha = 0;
    }

    public void SetPlayerScript(Player playerScript){
        PlayerScript = playerScript;
    }

    private void PutCards(){
        List<string> cards = Cards.CardParams.Keys.ToList();

        LeftCardName = cards[Random.Range(0, cards.Count)];
        cards.Remove(LeftCardName);

        MiddleCardName = cards[Random.Range(0, cards.Count)];
        cards.Remove(MiddleCardName);

        RightCardName = cards[Random.Range(0, cards.Count)];
        cards.Remove(RightCardName);

        LeftCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = LeftCardName;
        LeftCardButton.transform.GetChild(0).GetComponent<RawImage>().texture = Cards.CardParams[LeftCardName].CardImage;

        MiddleCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = MiddleCardName;
        MiddleCardButton.transform.GetChild(0).GetComponent<RawImage>().texture = Cards.CardParams[MiddleCardName].CardImage;

        RightCardButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = RightCardName;
        RightCardButton.transform.GetChild(0).GetComponent<RawImage>().texture = Cards.CardParams[RightCardName].CardImage;
    }

    public void LeftCard(){
        PlayerScript.ChooseCard(LeftCardName, Side.Blue);

        StartCoroutine(Hide());
    }

    public void MiddleCard(){
        PlayerScript.ChooseCard(MiddleCardName, Side.Blue);

        StartCoroutine(Hide());
    }

    public void RightCard(){
        PlayerScript.ChooseCard(RightCardName, Side.Blue);

        StartCoroutine(Hide());
    }
}