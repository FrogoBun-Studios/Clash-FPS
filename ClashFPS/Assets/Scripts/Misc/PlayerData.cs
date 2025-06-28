using Unity.Collections;
using Unity.Netcode;


public struct PlayerData : INetworkSerializable
{
	public Side side;
	public FixedString32Bytes name;
	public float elixir;

	public PlayerData(Side side, FixedString32Bytes name, float elixir)
	{
		this.side = side;
		this.name = name;
		this.elixir = elixir;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref side);
		serializer.SerializeValue(ref name);
		serializer.SerializeValue(ref elixir);
	}

	public PlayerData WithElixir(float elixir)
	{
		return new PlayerData(side, name, elixir);
	}

	public PlayerData PlusElixir(float amount)
	{
		return new PlayerData(side, name, elixir + amount);
	}

	public PlayerData WithName(FixedString32Bytes name)
	{
		return new PlayerData(side, name, elixir);
	}

	public PlayerData WithSide(Side side)
	{
		return new PlayerData(side, name, elixir);
	}
}