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
		foreach (GameObject playerGo in GameObject.FindGameObjectsWithTag("Player"))
		{
			Player player = playerGo.GetComponent<Player>();
			if (!playerIDToPlayer.ContainsKey(player.OwnerClientId))
			{
				players.Add(player.GetComponent<Player>());
				playerIDToPlayer.Add(player.GetComponent<NetworkObject>().OwnerClientId, player.GetComponent<Player>());
			}
		}
	}

	public Player GetPlayerByID(ulong playerID)
	{
		return playerIDToPlayer[playerID];
	}

	public List<Player> GetPlayers()
	{
		return players;
	}

	/// <summary>
	///     Updates all the names and health sliders of the other player on the new player OWNER.
	/// </summary>
	public void UpdateAllNamesAndHealthSliders()
	{
		foreach (Player player in players)
		{
			player.UpdateHealthSliderRpc(player.GetCard().GetHealth());
			player.UpdatePlayerNameTextRpc();
		}
	}
}