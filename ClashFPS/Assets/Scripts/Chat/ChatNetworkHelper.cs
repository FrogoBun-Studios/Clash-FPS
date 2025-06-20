using Unity.Collections;
using Unity.Netcode;


public class ChatNetworkHelper : NetworkBehaviour
{
	private readonly NetworkList<FixedString64Bytes> _chatMessages = new();

	public void AddMessage(string message)
	{
		AddMessageRpc(message);
	}

	public void RemoveMessage(int index)
	{
		RemoveMessageRpc(index);
	}

	[Rpc(SendTo.Server)]
	private void AddMessageRpc(string message)
	{
		_chatMessages.Add(message);
	}

	[Rpc(SendTo.Server)]
	private void RemoveMessageRpc(int index)
	{
		_chatMessages.RemoveAt(index);
	}

	public string[] GetChatMessages()
	{
		string[] chatMessages = new string[_chatMessages.Count];
		for (int i = 0; i < _chatMessages.Count; i++)
			chatMessages[i] = _chatMessages[i].ToString();

		return chatMessages;
	}
}