using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;


public class GameManager : NetworkBehaviour
{
	private readonly NetworkVariable<int> bluePlayersCount = new();
	private readonly NetworkVariable<int> redPlayersCount = new();
	private readonly Dictionary<ulong, Player> playerIDToPlayer = new();
	private readonly List<Player> players = new();
	private readonly Dictionary<ulong, PlayerData> playerIDToData = new();

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

	public void Refresh()
	{
		Debug.Log("Game manager refresh");

		playerIDToPlayer.Clear();
		players.Clear();
		playerIDToData.Clear();
		Debug.Log("Cleared game manager lists");

		foreach (GameObject playerGo in GameObject.FindGameObjectsWithTag("Player"))
		{
			Player player = playerGo.GetComponent<Player>();
			players.Add(player);
			playerIDToPlayer.Add(player.OwnerClientId, player);
			playerIDToData.Add(player.OwnerClientId, player.GetPlayerData());

			Debug.Log($"Found player {player.OwnerClientId}, added to game manager lists...");
		}

		Debug.Log("Game manager refresh completed");
	}

	public Player GetPlayerByID(ulong playerID)
	{
		return playerIDToPlayer[playerID];
	}

	public List<Player> GetPlayers()
	{
		return players;
	}

	public PlayerData GetPlayerDataByID(ulong playerID)
	{
		return playerIDToData[playerID];
	}

	public void UpdatePlayerData(ulong playerID, PlayerData data)
	{
		playerIDToData[playerID] = data;
	}
}