using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;


public class GameManager : NetworkBehaviour
{
	private readonly NetworkVariable<int> bluePlayersCount = new();
	private readonly NetworkVariable<int> redPlayersCount = new();
	private readonly Dictionary<ulong, Player> playerIDToPlayer = new();
	private readonly List<Player> players = new();

	public static GameManager Get { get; private set; }

	private void OnEnable()
	{
		Get = this;
	}

	[Rpc(SendTo.Server)]
	public void UpdateBluePlayersCountRpc(int amount)
	{
		bluePlayersCount.Value += amount;
	}

	[Rpc(SendTo.Server)]
	public void UpdateRedPlayersCountRpc(int amount)
	{
		redPlayersCount.Value += amount;
	}

	public int GetRedPlayersCount()
	{
		return redPlayersCount.Value;
	}

	public int GetBluePlayersCount()
	{
		return bluePlayersCount.Value;
	}

	public void InitOnOwner()
	{
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
		{
			players.Add(player.GetComponent<Player>());
			playerIDToPlayer.Add(player.GetComponent<NetworkObject>().OwnerClientId, player.GetComponent<Player>());
		}
	}

	public Player GetPlayerByID(ulong playerID)
	{
		return playerIDToPlayer[playerID];
	}
}