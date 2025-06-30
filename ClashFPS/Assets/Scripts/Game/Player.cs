using System.Collections;

using TMPro;

using Unity.Cinemachine;
using Unity.Netcode;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Player : NetworkBehaviour
{
	[SerializeField] private Slider topHealthSlider;
	[SerializeField] private TextMeshProUGUI playerNameText;
	[SerializeField] private NetworkObject gameManager;
	[SerializeField] private NetworkObject chatNetworkHelper;
	[SerializeField] private NetworkObject networkQuery;
	[SerializeField] private float timeToRespawn;
	[SerializeField] private MovementController movementController;
	[SerializeField] private NetworkObject[] towers;

	private readonly NetworkVariable<PlayerData> data = new(new PlayerData(Side.Blue, "", 5));
	private Card card;

	private SideSelection sideSelection;
	private CardSelection cardSelection;
	private SettingsMenu settingsMenu;

	private bool spawned;
	private readonly bool enableCardControl = true;
	public PlayerSettings playerSettings;
	private Transform model;
	private Slider currentHealthSlider;

	private void Update()
	{
		if (!IsOwner)
			return;

		if (Input.GetKeyDown(KeyCode.Escape) && !sideSelection.IsShowen() && !cardSelection.IsShowen())
		{
			if (settingsMenu.IsShowen())
				StartCoroutine(settingsMenu.Hide());
			else
				StartCoroutine(settingsMenu.Show());
		}

		movementController.Enable(spawned && !settingsMenu.IsShowen() && enableCardControl);
		if (card != null && spawned)
			card.UpdateCard(!settingsMenu.IsShowen() && enableCardControl);
	}

	/// <summary>
	///     Runs probably only on the OWNER because it runs by the one that initiates Move of the controller.
	/// </summary>
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (IsOwner && hit.gameObject.CompareTag("WaterCols"))
		{
			Debug.Log("Player touched water");
			card.DamageServerRpc(Constants.nonPlayerID, Mathf.Infinity);
		}
	}

	/// <summary>
	///     Adds the amount to the elixir of the player on SERVER.
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
	public void UpdateElixirServerRpc(float amount)
	{
		if (amount > 0)
			GameManager.Get.UpdateScore(OwnerClientId, amount);

		if (data.Value.elixir < Constants.maxElixir)
		{
			data.Value = GetPlayerData().PlusElixir(amount);
			if (amount >= 0.5)
				Debug.Log($"Increased elixir of player {OwnerClientId} by {amount} to {data.Value.elixir}");
		}
	}

	public PlayerData GetPlayerData()
	{
		return data.Value;
	}

	/// <returns>
	///     Returns the card of this player on EVERYONE.
	///     If you are not in the server, this card is brain-dead, and exists to call server RPCs.
	/// </returns>
	public Card GetCard()
	{
		return card;
	}

	/// <summary>
	///     Calls the UpdateHealthSlider func on EVERYONE
	/// </summary>
	[Rpc(SendTo.Everyone)]
	public void UpdateHealthSliderRpc(float health)
	{
		Debug.Log($"Updating health of player {OwnerClientId} to {health}");
		StartCoroutine(UpdateHealthSlider(health));
	}

	/// <summary>
	///     Updates health slider of this player to given health, supposed to run on EVERYONE
	/// </summary>
	private IEnumerator UpdateHealthSlider(float health)
	{
		if (currentHealthSlider == null)
		{
			Debug.LogError($"Can't update health slider of player {OwnerClientId} because currentHealthSlider is null");
			yield break;
		}

		if (health <= 0)
		{
			currentHealthSlider.value = 0;
			yield break;
		}

		float stepSize = 2f;
		float dir = health > currentHealthSlider.value ? stepSize : -stepSize;
		float wait = 0.5f / (Mathf.Abs(currentHealthSlider.value - health) / stepSize);

		for (float v = currentHealthSlider.value; Mathf.Abs(health - v) > stepSize; v += dir)
		{
			currentHealthSlider.value = v;
			yield return new WaitForSeconds(wait);
		}

		currentHealthSlider.value = health;
	}

	private void OnConnectionEvent(NetworkManager sender, ConnectionEventData eventData)
	{
		if (eventData.EventType == ConnectionEvent.PeerDisconnected)
		{
			if (IsServer)
			{
				PlayerData data = GameManager.Get.GetPlayerDataByID(eventData.ClientId);
				Chat.Get.Log($"{data.name} has disconnected");

				if (data.side == Side.Blue)
					GameManager.Get.UpdateBluePlayersCountServerRpc(-1);
				else
					GameManager.Get.UpdateRedPlayersCountServerRpc(-1);

				NetworkObject card = null;
				NetworkObject model = null;
				foreach (GameObject cardGo in GameObject.FindGameObjectsWithTag("Card"))
					if (cardGo.name == "Card" + eventData.ClientId)
					{
						card = cardGo.GetComponent<NetworkObject>();
						break;
					}

				foreach (GameObject modelGo in GameObject.FindGameObjectsWithTag("Model"))
					if (modelGo.name == "Model" + eventData.ClientId)
					{
						model = modelGo.GetComponent<NetworkObject>();
						break;
					}

				if (card == null || model == null)
				{
					Debug.LogError(
						$"Can't find player {eventData.ClientId} card and model to destroy after disconnection");
				}
				else
				{
					card.Despawn();
					model.Despawn();
					Debug.Log($"Destroyed player {eventData.ClientId} card and model after disconnection");
				}
			}

			StartCoroutine(RefreshGameManagerAfterPlayerDisconnection(GameManager.Get.GetPlayers().Count - 1));
		}
		else if (eventData.EventType == ConnectionEvent.ClientDisconnected &&
		         eventData.ClientId == NetworkManager.Singleton.LocalClientId)
			LeaveGame();
	}

	public void LeaveGame()
	{
		NetworkManager.Singleton.Shutdown();

		Destroy(NetworkManager.Singleton.gameObject);
		Destroy(GameManager.Get.gameObject);

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		SceneManager.LoadScene(0);
	}

	private IEnumerator RefreshGameManagerAfterPlayerDisconnection(int expectedPlayerCount)
	{
		yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Player").Length <= expectedPlayerCount);

		Debug.Log("Refreshing game manager after player disconnection");
		GameManager.Get.Refresh();
	}

	public override void OnNetworkDespawn()
	{
		if (IsOwner)
			NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
	}

	public Transform GetModel()
	{
		return model;
	}

	#region Init

	public override void OnNetworkSpawn()
	{
		if (IsServer)
		{
			if (IsOwner)
			{
				SpawnHelpers();

				for (int i = 0; i < towers.Length; i++)
				{
					if (!towers[i].IsSpawned)
					{
						towers[i] = Instantiate(towers[i].gameObject).GetComponent<NetworkObject>();
						towers[i].Spawn();
						Debug.Log($"Spawned tower {i + 1}/{towers.Length}");
					}
				}
			}

			registerNetworkQueries();
		}

		data.OnValueChanged += (value, newValue) =>
		{
			GameManager.Get.UpdatePlayerData(OwnerClientId, newValue);
			if (newValue.name != value.name)
				UpdatePlayerName();
		};

		if (topHealthSlider != null)
			topHealthSlider.name = $"TopSlider{OwnerClientId}";
		StartCoroutine(InitGameManager());

		if (!IsOwner)
			return;

		if (topHealthSlider != null)
			Destroy(topHealthSlider.gameObject);
		if (playerNameText != null)
			Destroy(playerNameText.gameObject);

		chatNetworkHelper = GameObject.Find("ChatNetworkHelper(Clone)").GetComponent<NetworkObject>();
		Chat.Get.EnableChatNetworking(chatNetworkHelper.GetComponent<ChatNetworkHelper>(), this);
		Debug.Log("Enabled chat networking");

		movementController.SetResetCameraPosition();
		Debug.Log($"Set reset camera position to {movementController.transform.position}");

		LoadSettings();
		InitMenus();

		NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
		Application.targetFrameRate = 120;

		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 1;
		Destroy(GameObject.Find("LoadingBar"), 0.25f);
		Chat.Get.Log($"Player {OwnerClientId} logged in");

		ChooseSide();
	}

	private void registerNetworkQueries()
	{
		NetworkQuery.Instance.Register($"Get Canvas Height {OwnerClientId}", _ => model.localScale.y * 4f + 2.1f);
		Debug.Log($"Registered \"Get Canvas Height\" for player {OwnerClientId} in NetworkQuery");
	}

	private void SpawnHelpers()
	{
		if (GameManager.Get == null)
		{
			gameManager = Instantiate(gameManager.gameObject).GetComponent<NetworkObject>();
			gameManager.Spawn();
			Debug.Log("Spawned GameManager");
		}

		if (!chatNetworkHelper.IsSpawned)
		{
			chatNetworkHelper = Instantiate(chatNetworkHelper.gameObject).GetComponent<NetworkObject>();
			chatNetworkHelper.Spawn();
			Debug.Log("Spawned ChatNetworkHelper");
		}

		if (!networkQuery.IsSpawned)
		{
			networkQuery = Instantiate(networkQuery.gameObject).GetComponent<NetworkObject>();
			networkQuery.Spawn();
			Debug.Log("Spawned NetworkQuery");
		}
	}

	private IEnumerator InitGameManager()
	{
		Debug.Log("Waiting for game manager to spawn");
		yield return new WaitUntil(() => GameManager.Get != null);
		GameManager.Get.Refresh();

		if (IsOwner)
			InitAllPlayers();
	}

	private void InitMenus()
	{
		sideSelection = FindFirstObjectByType<SideSelection>();
		if (sideSelection != null)
		{
			Debug.Log("Found side selection menu");
			sideSelection.Set(this);
		}
		else
			Debug.LogError("Could not find side selection menu");

		cardSelection = FindFirstObjectByType<CardSelection>();
		if (sideSelection != null)
		{
			Debug.Log("Found card selection menu");
			cardSelection.Set(this);
		}
		else
			Debug.LogError("Could not find card selection menu");

		settingsMenu = FindFirstObjectByType<SettingsMenu>();
		if (sideSelection != null)
		{
			Debug.Log("Found settings menu");
			settingsMenu.Set(this);
		}
		else
			Debug.LogError("Could not find settings menu");
	}

	private void InitAllPlayers()
	{
		Debug.Log("Starting to init all players");

		// Setting players' names on new player's pc
		foreach (Player player in GameManager.Get.GetPlayers())
		{
			player.playerNameText.text = player.data.Value.name.ToString();
			Debug.Log($"Set name for player {player.OwnerClientId}: {player.data.Value.name}");
		}

		// Setting other players' cards on new player's pc
		foreach (GameObject cardGo in GameObject.FindGameObjectsWithTag("Card"))
		{
			Card card = cardGo.GetComponent<Card>();
			ulong cardID = card.OwnerClientId;
			Debug.Log($"Found card of player {cardID}");

			cardGo.name = $"Card{cardID}";
			GameManager.Get.GetPlayerByID(cardID).card = card;
			card.SetPlayerForNonServer(GameManager.Get.GetPlayerByID(cardID).transform);
		}

		// Setting other players' sliders' height and max value on new player's pc (only if the other player chose a card already) 
		foreach (GameObject topSliderGo in GameObject.FindGameObjectsWithTag("TopSlider"))
		{
			Player player = topSliderGo.transform.parent.parent.GetComponent<Player>();

			Debug.Log($"Found canvas of player {player.OwnerClientId}");
			if (player == this)
				Debug.Log("This player is me, ignoring...");
			else if (player.card == null)
				Debug.Log("This player didn't choose a card yet, ignoring...");

			if (player != this && player.card != null)
			{
				player.currentHealthSlider = player.topHealthSlider;
				Debug.Log($"Set canvas of player {player.OwnerClientId}");

				NetworkQuery.Instance.Request<float>($"Get Canvas Height {player.OwnerClientId}",
					height =>
					{
						Debug.Log($"Set canvas height of player {player.OwnerClientId} to {height}");
						player.currentHealthSlider.transform.parent.localPosition = new Vector3(0, height, 0);
					});

				player.currentHealthSlider.maxValue = player.card.GetParams().health;
				Debug.Log(
					$"Set health slider max value of player {player.OwnerClientId} to {player.currentHealthSlider.maxValue}");
				player.currentHealthSlider.value = player.card.GetHealth();
				Debug.Log(
					$"Set health slider value of player {player.OwnerClientId} to {player.currentHealthSlider.value}");
			}
		}

		Debug.Log("Finished to init all players");
	}

	#endregion

	#region Name

	[ServerRpc(RequireOwnership = false)]
	private void SetPlayerNameServerRpc(string name)
	{
		Debug.Log($"Setting name of player {OwnerClientId} to {name}");
		data.Value = GetPlayerData().WithName(name);
	}

	// [Rpc(SendTo.Everyone)]
	private void UpdatePlayerName()
	{
		Debug.Log($"Updating player {OwnerClientId} name to {data.Value.name}");
		playerNameText.text = data.Value.name.ToString();
	}

	#endregion

	#region Settings

	/// <summary>
	///     Updates game to the given settings (makes the settings actually work) on OWNER.
	/// </summary>
	public void UpdateGameToSettings(PlayerSettings playerSettings)
	{
		this.playerSettings = playerSettings;
		movementController.UpdateSensitivity(playerSettings.mouseSensitivity);
		Debug.Log($"Updated sensitivity of player to new settings: {playerSettings.mouseSensitivity}");
		SetPlayerNameServerRpc(playerSettings.playerName);
		GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Lens.FieldOfView = playerSettings.FOV;
		Debug.Log($"Updated FOV of player to new settings: {playerSettings.FOV}");
	}

	/// <summary>
	///     Loads player settings on OWNER.
	/// </summary>
	private void LoadSettings()
	{
		Debug.Log("Loading settings");

		PlayerSettings loadedSettings = new();
		loadedSettings.playerName = PlayerPrefs.GetString("playerName", $"Player {OwnerClientId}");
		loadedSettings.volume = PlayerPrefs.GetFloat("volume", 1);
		loadedSettings.mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 1.5f);
		loadedSettings.quality = PlayerPrefs.GetInt("quality", 0);
		loadedSettings.FOV = PlayerPrefs.GetFloat("FOV", 90);

		Debug.Log($"Loaded player name: {loadedSettings.playerName}");
		Debug.Log($"Loaded volume: {loadedSettings.volume}");
		Debug.Log($"Loaded sensitivity: {loadedSettings.mouseSensitivity}");
		Debug.Log($"Loaded quality: {loadedSettings.quality}");
		Debug.Log($"Loaded FOV: {loadedSettings.FOV}");

		UpdateGameToSettings(loadedSettings);
	}

	#endregion

	#region SideSelection

	/// <summary>
	///     Opens side selection menu. Can be called from OWNER.
	/// </summary>
	public void ChooseSide()
	{
		Debug.Log("Choosing side");

		spawned = false;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		movementController.ResetCamera();

		StartCoroutine(sideSelection.Show());
	}

	/// <summary>
	///     Sets side of player and respawns. Can be called from EVERYONE.
	/// </summary>
	public void SetSide(Side side)
	{
		Debug.Log($"Chose side {side}");

		UpdateSideServerRpc(side);
		RespawnRpc(false);
	}

	/// <summary>
	///     Updates side of player on SERVER.
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
	private void UpdateSideServerRpc(Side side)
	{
		data.Value = GetPlayerData().WithSide(side);
		Debug.Log($"Updated side of player {OwnerClientId} to {side}");
	}

	#endregion

	#region CardCreation

	/// <summary>
	///     Resets camera and shows card selection screen to respawn on OWNER.
	/// </summary>
	[Rpc(SendTo.Owner)]
	public void RespawnRpc(bool delay = true)
	{
		Debug.Log("Respawning");

		spawned = false;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		movementController.ResetCamera();

		StartCoroutine(cardSelection.Show(delay ? timeToRespawn : 0));
	}

	/// <summary>
	///     Deletes the old card and spawn a new one ready to go (respawn) on SERVER.
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
	public void ChooseCardServerRpc(string cardName)
	{
		Debug.Log($"Player {OwnerClientId} chose card \"{cardName}\"");

		StartCoroutine(ChooseCard(cardName));
	}

	private IEnumerator ChooseCard(string cardName)
	{
		SetCameraOnCardCreationRpc();

		if (card != null)
		{
			Debug.Log($"Player {OwnerClientId} already had a card");

			model.GetComponent<NetworkObject>().Despawn();
			card.GetComponent<NetworkObject>().Despawn();

			Debug.Log($"Despawning player {OwnerClientId} card");
			yield return new WaitUntil(() => card == null && model == null);
			Debug.Log($"Player {OwnerClientId} card despawned");
		}

		movementController.TeleportRpc(new Vector3(0, 2, data.Value.side == Side.Blue ? -34 : 34),
			Quaternion.Euler(0, data.Value.side == Side.Blue ? 0 : 180, 0));

		GameObject cardGo = Instantiate(Cards.CardPrefabs[cardName]);
		cardGo.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);
		Debug.Log($"Spawned a new card for player {OwnerClientId}: {cardName}");

		model = Instantiate(Cards.CardParams[cardName].modelPrefab).transform;
		model.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);
		model.gameObject.name = $"Model{OwnerClientId}";
		Debug.Log($"Spawned a new model for player {OwnerClientId}: {model.name}");

		SetCardRpc();
	}

	[Rpc(SendTo.Owner)]
	private void SetCameraOnCardCreationRpc()
	{
		GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow =
			movementController.GetCameraFollowTransform();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	/// <summary>
	///     Sets for every player, every player's card, runs on EVERYONE.
	///     <br /><br />For example,
	///     <br />If a third player joins, he will set the cards of the 2 other players on his machine,
	///     <br />and set his own card on his own machine too.
	///     <br />Every other player will do the same.
	///     <br /><br />*Notice: the cards that are set in clients are brain-dead and only exist to call server RPCs.
	/// </summary>
	[Rpc(SendTo.Everyone)]
	private void SetCardRpc()
	{
		//Find card of player who just chose a card
		Card card = null;
		foreach (GameObject cardGo in GameObject.FindGameObjectsWithTag("Card"))
		{
			if (cardGo.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId)
			{
				card = cardGo.GetComponent<Card>();
				break;
			}
		}

		if (card == null)
			Debug.LogError($"Didn't find card of player {OwnerClientId} who chose a card now");
		else
		{
			Debug.Log($"Found card of player {OwnerClientId} who chose a card now");

			//Set the card to the matching player on each pc
			card.gameObject.name = $"Card{OwnerClientId}";
			this.card = card;
			card.SetPlayerForNonServer(transform);

			Debug.Log($"Set the card to player {OwnerClientId}");
		}

		if (IsOwner)
			SetModel();
	}

	private void SetModel()
	{
		movementController.SetModel(card.GetParams().customCameraOffset);
		Debug.Log("Set model");
		movementController.EnableColliderRpc(true);

		SetHealthSliderRpc();
	}

	/// <summary>
	///     Sets for every player, every player's health slider, runs on EVERYONE.
	///     <br /><br />For example,
	///     <br />If a third player joins, he will set the health sliders of the 2 other players on his machine,
	///     <br />and set his own health slider on his machine to be the ui one instead of the top one.
	///     <br />Every other player will do the same.
	/// </summary>
	[Rpc(SendTo.Everyone)]
	private void SetHealthSliderRpc()
	{
		if (IsOwner)
		{
			//Setting health slider of the player the just chose a card on his pc
			currentHealthSlider = GameObject.Find("HealthSliderUI").GetComponent<Slider>();
			Debug.Log("Set my health slider to be the UI one");
			currentHealthSlider.maxValue = card.GetParams().health;
			Debug.Log($"Set my health slider max value: {currentHealthSlider.maxValue}");
		}
		else
		{
			//Setting the player that just chose a card slider's height and max value on other players' machines'
			currentHealthSlider = topHealthSlider;
			Debug.Log($"Set health slider of player {OwnerClientId}");

			NetworkQuery.Instance.Request<float>($"Get Canvas Height {OwnerClientId}", height =>
			{
				currentHealthSlider.transform.parent.localPosition = new Vector3(0, height, 0);
				Debug.Log($"Set the canvas height of player {OwnerClientId} to {height}");
			});

			currentHealthSlider.maxValue = card.GetParams().health;
			Debug.Log($"Set health slider max value of player {OwnerClientId}: {currentHealthSlider.maxValue}");
		}

		StartCardServerRpc();
	}

	private int clientReadyCounter;

	[ServerRpc(RequireOwnership = false)]
	private void StartCardServerRpc()
	{
		clientReadyCounter++;

		Debug.Log(
			$"{clientReadyCounter} out of {GameManager.Get.GetPlayers().Count} players acknowledged that player {OwnerClientId} chose a card");
		if (clientReadyCounter == GameManager.Get.GetPlayers().Count)
		{
			Debug.Log($"Starting player {OwnerClientId} card");
			GameManager.Get.GetPlayerByID(OwnerClientId).card.StartCard(transform);
			clientReadyCounter = 0;
			SpawnedRpc();
		}
	}

	[Rpc(SendTo.Everyone)]
	private void SpawnedRpc()
	{
		spawned = true;
		Debug.Log($"Player {OwnerClientId} spawned and chose a card");
	}

	#endregion
}