using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Netcode;

using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : NetworkBehaviour
{
	private readonly NetworkVariable<int> bluePlayersCount = new();
	private readonly NetworkVariable<int> redPlayersCount = new();
	private readonly Dictionary<ulong, Player> playerIDToPlayer = new();
	private readonly List<Player> players = new();
	private readonly Dictionary<ulong, PlayerData> playerIDToData = new();
	private float gameTime;
	private readonly Dictionary<ulong, float> scores = new();

	public static GameManager Get { get; private set; }

	private void OnEnable()
	{
		Get = this;
		DontDestroyOnLoad(gameObject);
	}

	private void Update()
	{
		if (!IsServer)
			return;

		gameTime += Time.deltaTime;
		if (gameTime >= Constants.gameLength)
		{
			List<FixedString32Bytes> names = new();
			List<float> scores = new();

			foreach (ulong playerID in this.scores.Keys)
			{
				names.Add(playerIDToData[playerID].name);
				scores.Add(this.scores[playerID]);
			}

			SetOnEndGameSceneLoadedRpc(names.ToArray(), scores.ToArray());
			NetworkManager.Singleton.SceneManager.LoadScene("Game Over", LoadSceneMode.Single);
			gameTime = Mathf.NegativeInfinity;
		}
	}

	[Rpc(SendTo.Everyone)]
	private void SetOnEndGameSceneLoadedRpc(FixedString32Bytes[] names, float[] scores)
	{
		NetworkManager.Singleton.SceneManager.OnLoadComplete += (id, sceneName, mode) =>
		{
			Dictionary<string, float> scoresDict = new();
			for (int i = 0; i < scores.Length; i++)
				scoresDict.Add(names[i].ToString(), scores[i]);

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			GameOverMenu menu = GameObject.Find("Game Over Menu").GetComponent<GameOverMenu>();
			menu.ShowScores(scoresDict);
		};
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

			if (IsServer && !scores.ContainsKey(player.OwnerClientId))
				scores.Add(player.OwnerClientId, 0);

			Debug.Log($"Found player {player.OwnerClientId}, added to game manager lists...");
		}

		foreach (ulong playerID in scores.Keys.Where(id => !playerIDToPlayer.ContainsKey(id)).ToList())
			scores.Remove(playerID);

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

	public void UpdateScore(ulong playerID, float amount)
	{
		scores[playerID] += amount;
		if (amount >= 0.5)
			Debug.Log($"Updated player {playerID} score by {amount} to {scores[playerID]}");
	}

	public Player GetThisPlayer()
	{
		return GetPlayerByID(OwnerClientId);
	}
}