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
}