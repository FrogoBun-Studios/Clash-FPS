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

	public void Init()
	{
		Debug.Log("Game manager init");
		foreach (GameObject playerGo in GameObject.FindGameObjectsWithTag("Player"))
		{
			Player player = playerGo.GetComponent<Player>();
			Debug.Log($"Found player {player.OwnerClientId}");
			if (!playerIDToPlayer.ContainsKey(player.OwnerClientId))
			{
				Debug.Log($"Player {player.OwnerClientId} was not already in my players list, adding now...");
				players.Add(player.GetComponent<Player>());
				playerIDToPlayer.Add(player.GetComponent<NetworkObject>().OwnerClientId, player.GetComponent<Player>());
			}
		}

		Debug.Log("Game manager init completed");
	}

	public Player GetPlayerByID(ulong playerID)
	{
		return playerIDToPlayer[playerID];
	}

	public List<Player> GetPlayers()
	{
		return players;
	}
}