using System.Collections.Generic;
using System.Linq;

using TMPro;

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

	private readonly NetworkVariable<float> gameTime = new();
	private readonly Dictionary<ulong, float> scores = new();
	private TextMeshProUGUI timerText;
	private bool suddenDeath;
	private bool gameEnded;

	private int blueTeamScore;
	private int redTeamScore;

	public static GameManager Get { get; private set; }

	private void OnEnable()
	{
		Get = this;
		DontDestroyOnLoad(gameObject);
		timerText = GameObject.Find("GameTime").GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		if (gameEnded)
			return;

		if (IsServer)
		{
			gameTime.Value += Time.deltaTime;

			if (!suddenDeath)
			{
				if (gameTime.Value >= Constants.gameLength)
				{
					suddenDeath = true;
					gameTime.Value = 0;
				}
			}
			else
			{
				if (gameTime.Value >= Constants.suddenDeathTime || blueTeamScore > 0 || redTeamScore > 0)
					EndGame();
			}
		}

		float timeLeft;
		if (!suddenDeath)
			timeLeft = Constants.gameLength - gameTime.Value;
		else
			timeLeft = Constants.suddenDeathTime - gameTime.Value;

		if (timeLeft >= 0)
		{
			int minutes = (int)(timeLeft / 60);
			int secs = (int)(timeLeft % 60f);

			string minutesStr = minutes.ToString("00");
			string secsStr = secs.ToString("00");

			timerText.text = $"{minutesStr}:{secsStr}";
		}
		else
			timerText.text = "00:00";

		if (suddenDeath)
			timerText.color = Color.red;
	}

	private void EndGame()
	{
		gameEnded = true;

		List<FixedString32Bytes> names = new();
		List<float> scores = new();

		foreach (ulong playerID in this.scores.Keys)
		{
			names.Add(playerIDToData[playerID].name);
			scores.Add(this.scores[playerID]);
		}

		SetOnEndGameSceneLoadedRpc(names.ToArray(), scores.ToArray(),
			blueTeamScore > redTeamScore ? Side.Blue : Side.Red, blueTeamScore == redTeamScore);
		NetworkManager.Singleton.SceneManager.LoadScene("Game Over", LoadSceneMode.Single);
	}

	[Rpc(SendTo.Everyone)]
	private void SetOnEndGameSceneLoadedRpc(FixedString32Bytes[] names, float[] scores, Side winner, bool tie)
	{
		NetworkManager.Singleton.SceneManager.OnLoadComplete += (id, sceneName, mode) =>
		{
			Dictionary<string, float> scoresDict = new();
			for (int i = 0; i < scores.Length; i++)
				scoresDict.Add(names[i].ToString(), scores[i]);

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			GameOverMenu menu = GameObject.Find("Game Over Menu").GetComponent<GameOverMenu>();
			menu.Show(scoresDict, winner, tie);
		};
	}

	private int playAgainCounter;

	[ServerRpc(RequireOwnership = false)]
	public void PlayAgainServerRpc()
	{
		playAgainCounter++;

		if (playAgainCounter == players.Count)
		{
			NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);

			foreach (Player player in players)
			{
				ulong id = player.OwnerClientId;
				player.GetComponent<NetworkObject>().Despawn();
				player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
			}

			playAgainCounter = 0;
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdateBluePlayersCountServerRpc(int amount)
	{
		bluePlayersCount.Value += amount;
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdateRedPlayersCountServerRpc(int amount)
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

	public void OnTowerDestroy(Side side, bool isKing)
	{
		if (isKing)
		{
			Debug.Log($"On {side} king tower destroy");

			if (side == Side.Blue)
				redTeamScore = int.MaxValue;
			else
				blueTeamScore = int.MaxValue;

			EndGame();

			return;
		}

		Debug.Log($"On {side} tower destroy");

		if (side == Side.Blue)
			redTeamScore++;
		else
			blueTeamScore++;

		if (blueTeamScore >= 2 || redTeamScore >= 2)
			EndGame();
	}
}