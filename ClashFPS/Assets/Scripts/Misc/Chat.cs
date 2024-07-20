using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Chat : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ChatText;
    [SerializeField] private float TimeToDisappear = 5f;
    [SerializeField] private CanvasGroup CanvasGroup;
    [SerializeField] private int maxMessages;
    private float time;
    private List<string> ChatMessages = new List<string>();
    private bool isShown = false;

    public static Chat Singleton { get; private set; }

    private void OnEnable(){
        Singleton = this;
        time = TimeToDisappear;
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    public void Log(string message)
    {
        Show();

        message = $"[System]: {message}";
        ChatMessages.Add(message);
        Debug.Log(message);
    }

    public void PlayerWrite(string message, string playerName)
    {
        Show();

        message = $"[{playerName}]: {message}";
        ChatMessages.Add(message);
        Debug.Log(message);
    }

    public void KillLog(string killer, string killed, string killerCard)
    {
        Show();

        string message = $"[System]: {killer} killed {killed} as a {killerCard}";
        ChatMessages.Add(message);
        Debug.Log(message);
    }

    private void Update(){
        time -= Time.deltaTime;

        if (time <= 0 && isShown)
            StartCoroutine(Disappear());

        if(Input.GetKeyUp(KeyCode.Return))
            Show();

        for(int i = 0; i < (ChatMessages.Count - maxMessages); i++){
            ChatMessages.RemoveAt(0);
        }

        ChatText.text = string.Join("\n", ChatMessages);
    }

    private void Show(){
        time = TimeToDisappear;
        isShown = true;
        CanvasGroup.alpha = 1;

        StopAllCoroutines();
    }

    private IEnumerator Disappear(){
        isShown = false;

        for(float t = 1; t >= 0; t -= 0.01f){
            CanvasGroup.alpha = t;
            yield return new WaitForSeconds(0.01f);
        }
        CanvasGroup.alpha = 0;
    }
}
