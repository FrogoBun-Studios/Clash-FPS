using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;


public class Chat : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI chatText;
	[SerializeField] private float timeToDisappear = 5f;
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private int maxMessages;
	private readonly List<string> _chatMessages = new();
	private ChatNetworkHelper chatNetworkHelper;
	private bool _isShown;
	private Player playerScript;
	private float _time;

	public static Chat Get { get; private set; }

	private void Update()
	{
		_time -= Time.deltaTime;

		if (_time <= 0 && _isShown)
			StartCoroutine(Disappear());

		if (Input.GetKeyUp(KeyCode.Return))
			Show();

		if (chatNetworkHelper is not null)
		{
			string messages = string.Join("\n", chatNetworkHelper.GetChatMessages());

			if (chatText.text != messages)
				Show();

			chatText.text = messages;
		}
	}

	private void OnEnable()
	{
		Get = this;
		_time = timeToDisappear;
	}

	private void OnDestroy()
	{
		if (Get == this)
			Get = null;
	}

	private void AddMessage(string message)
	{
		Debug.Log(message);

		if (chatNetworkHelper is null)
		{
			_chatMessages.Add(message);
			for (int i = 0; i < _chatMessages.Count - maxMessages; i++) _chatMessages.RemoveAt(0);
			chatText.text = string.Join("\n", _chatMessages);
		}
		else
		{
			chatNetworkHelper.AddMessage(message);
			for (int i = 0; i < chatNetworkHelper.GetChatMessages().Length - maxMessages; i++)
				chatNetworkHelper.RemoveMessage(0);
		}
	}

	public void Log(params object[] message)
	{
		Show();
		if (chatNetworkHelper is not null)
			AddMessage($"[{playerScript.GetPlayerName()}'s System]: {string.Join(' ', message)}");
		else
			AddMessage($"[System]: {string.Join(' ', message)}");
	}

	public void PlayerWrite(string message)
	{
		Show();
		AddMessage($"[{playerScript.GetPlayerName()}]: {message}");
	}

	public void KillLog(string killer, string killed, string killerCard)
	{
		Show();
		AddMessage($"[Kill]: {killer} killed {killed} as a {killerCard}");
	}

	private void Show()
	{
		_time = timeToDisappear;
		_isShown = true;
		canvasGroup.alpha = 1;

		StopAllCoroutines();
	}

	private IEnumerator Disappear()
	{
		_isShown = false;

		for (float t = 1; t >= 0; t -= 0.01f)
		{
			canvasGroup.alpha = t;
			yield return new WaitForSeconds(0.01f);
		}

		canvasGroup.alpha = 0;
	}

	public void EnableChatNetworking(ChatNetworkHelper chatNetworkHelper, Player player)
	{
		this.chatNetworkHelper = chatNetworkHelper;
		playerScript = player;
	}
}