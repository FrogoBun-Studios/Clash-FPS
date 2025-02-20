using Unity.Netcode;


public class GameManager : NetworkBehaviour
{
	public NetworkVariable<int> bluePlayersCount = new();
	public NetworkVariable<int> redPlayersCount = new();
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
}