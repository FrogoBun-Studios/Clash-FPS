using Unity.Collections;
using Unity.Netcode;


public class ChatNetworkHelper : NetworkBehaviour
{
	private readonly NetworkList<FixedString64Bytes> chatMessages = new();

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
		chatMessages.Add(message);
	}

	[Rpc(SendTo.Server)]
	private void RemoveMessageRpc(int index)
	{
		chatMessages.RemoveAt(index);
	}

	public string[] GetChatMessages()
	{
		string[] chatMessages = new string[this.chatMessages.Count];
		for (int i = 0; i < this.chatMessages.Count; i++)
			chatMessages[i] = this.chatMessages[i].ToString();

		return chatMessages;
	}
}