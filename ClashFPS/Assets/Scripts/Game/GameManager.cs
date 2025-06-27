using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;


public class GameManager : NetworkBehaviour
{
	private readonly NetworkVariable<int> bluePlayersCount = new();
	private readonly NetworkVariable<int> redPlayersCount = new();
	private readonly Dictionary<ulong, Player> playerIDToPlayer = new();
	private readonly List<Player> players = new();
	private readonly Dictionary<ulong, string> playerIDToName = new();

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

		playerIDToPlayer.Clear();
		players.Clear();
		playerIDToName.Clear();
		Debug.Log("Cleared game manager lists");

		foreach (GameObject playerGo in GameObject.FindGameObjectsWithTag("Player"))
		{
			Player player = playerGo.GetComponent<Player>();
			players.Add(player);
			playerIDToPlayer.Add(player.OwnerClientId, player);
			playerIDToName.Add(player.OwnerClientId, player.GetPlayerName());

			Debug.Log($"Found player {player.OwnerClientId}, added to game manager lists...");
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

	public string GetPlayerNameByID(ulong playerID)
	{
		return playerIDToName[playerID];
	}

	public void UpdatePlayerNameInDict(ulong playerID, string name)
	{
		playerIDToName[playerID] = name;
	}
}