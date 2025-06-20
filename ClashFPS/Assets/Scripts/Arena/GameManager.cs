using System.Collections.Generic;

using Unity.Netcode;


public class GameManager : NetworkBehaviour
{
	private readonly NetworkVariable<int> bluePlayersCount = new();
	private readonly NetworkVariable<int> redPlayersCount = new();
	private readonly Dictionary<ulong, Player> playerIDToPlayer = new();

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

	public void AddPlayerToMap(ulong playerID, Player player)
	{
		playerIDToPlayer.Add(playerID, player);
	}

	public Player GetPlayerByID(ulong playerID)
	{
		return playerIDToPlayer[playerID];
	}
}