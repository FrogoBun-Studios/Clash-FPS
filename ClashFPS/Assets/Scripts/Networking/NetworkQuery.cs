using System;
using System.Collections.Generic;
using System.Text;

using Unity.Netcode;

using UnityEngine;


public class NetworkQuery : NetworkBehaviour
{
	public static NetworkQuery Instance;

	private ulong localClientId => NetworkManager.Singleton.LocalClientId;

	private readonly Dictionary<string, Func<ulong, byte[]>> serverHandlers = new();
	private readonly Dictionary<int, Action<byte[]>> clientCallbacks = new();
	private int requestIdCounter;

	private void Awake()
	{
		if (Instance == null) Instance = this;
	}

	// -----------------------
	// Client-side
	// -----------------------

	public void Request<T>(string queryKey, Action<T> callback)
	{
		int requestId = requestIdCounter++;
		clientCallbacks[requestId] = bytes =>
		{
			T result = Deserialize<T>(bytes);
			callback(result);
		};

		SendRequestServerRpc(queryKey, requestId);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendRequestServerRpc(string key, int requestId, ServerRpcParams rpcParams = default)
	{
		if (serverHandlers.TryGetValue(key, out Func<ulong, byte[]> handler))
		{
			byte[] response = handler(rpcParams.Receive.SenderClientId);
			SendResponseClientRpc(key, requestId, response, rpcParams.Receive.SenderClientId);
		}
	}

	[ClientRpc]
	private void SendResponseClientRpc(string key, int requestId, byte[] response, ulong clientId)
	{
		if (clientId != localClientId) return;

		if (clientCallbacks.TryGetValue(requestId, out Action<byte[]> callback))
		{
			callback(response);
			clientCallbacks.Remove(requestId);
		}
	}

	// -----------------------
	// Server-side
	// -----------------------

	public void Register<T>(string queryKey, Func<ulong, T> resolver)
	{
		serverHandlers[queryKey] = clientId =>
		{
			T result = resolver(clientId);
			return Serialize(result);
		};
	}

	// -----------------------
	// Utility
	// -----------------------


	[Serializable]
	public struct Wrapper<T>
	{
		public T value;
	}


	private byte[] Serialize<T>(T obj)
	{
		Wrapper<T> wrapped = new() { value = obj };
		string json = JsonUtility.ToJson(wrapped);
		return Encoding.UTF8.GetBytes(json);
	}

	private T Deserialize<T>(byte[] bytes)
	{
		string json = Encoding.UTF8.GetString(bytes);
		Wrapper<T> wrapped = JsonUtility.FromJson<Wrapper<T>>(json);
		return wrapped.value;
	}
}