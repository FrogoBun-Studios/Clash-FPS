using System.Collections;

using TMPro;

using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;

using UnityEngine;
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

	private readonly NetworkVariable<Side> side = new();
	private Card card;

	private readonly NetworkVariable<FixedString32Bytes> playerName = new();

	private readonly NetworkVariable<float> elixir = new(5);

	private SideSelection sideSelection;
	private CardSelection cardSelection;
	private SettingsMenu settingsMenu;

	private bool spawned;
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

		movementController.Enable(spawned && !settingsMenu.IsShowen());
		if (card != null && spawned)
			card.UpdateCard(settingsMenu.IsShowen());
	}

	/// <summary>
	///     Runs probably only on the OWNER because it runs by the one that initiates Move of the controller.
	/// </summary>
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (IsOwner && hit.gameObject.CompareTag("WaterCols"))
			card.DamageServerRpc(999ul, Mathf.Infinity);
	}

	/// <summary>
	///     Adds the amount to the elixir of the player on SERVER.
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
	public void UpdateElixirServerRpc(float amount)
	{
		elixir.Value += amount;
	}

	/// <returns>
	///     Returns the elixir this player has, works on EVERYONE
	/// </returns>
	public float GetElixir()
	{
		return elixir.Value;
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
		StartCoroutine(UpdateHealthSlider(health));
	}

	/// <summary>
	///     Updates health slider of this player to given health, supposed to run on EVERYONE
	/// </summary>
	private IEnumerator UpdateHealthSlider(float health)
	{
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

	#region Init

	public override void OnNetworkSpawn()
	{
		if (IsServer && IsOwner)
		{
			gameManager = Instantiate(gameManager.gameObject).GetComponent<NetworkObject>();
			gameManager.Spawn();

			chatNetworkHelper = Instantiate(chatNetworkHelper.gameObject).GetComponent<NetworkObject>();
			chatNetworkHelper.Spawn();

			networkQuery = Instantiate(networkQuery.gameObject).GetComponent<NetworkObject>();
			networkQuery.Spawn();

			for (int i = 0; i < towers.Length; i++)
			{
				towers[i] = Instantiate(towers[i].gameObject).GetComponent<NetworkObject>();
				towers[i].Spawn();
			}
		}

		if (IsServer)
		{
			NetworkQuery.Instance.Register($"Get Canvas Height {OwnerClientId}", _ => model.localScale.y * 4f + 2.1f);
		}

		topHealthSlider.name = $"TopSlider{OwnerClientId}";
		playerName.OnValueChanged += (value, newValue) => UpdatePlayerNameTextRpc();
		StartCoroutine(InitGameManager());

		if (!IsOwner)
			return;

		chatNetworkHelper = GameObject.Find("ChatNetworkHelper(Clone)").GetComponent<NetworkObject>();
		Chat.Get.EnableChatNetworking(chatNetworkHelper.GetComponent<ChatNetworkHelper>(), this);

		LoadSettings();
		movementController.SetResetCameraPosition();

		sideSelection = FindFirstObjectByType<SideSelection>();
		sideSelection.Set(this);
		cardSelection = FindFirstObjectByType<CardSelection>();
		cardSelection.Set(this);
		settingsMenu = FindFirstObjectByType<SettingsMenu>();
		settingsMenu.Set(this);
		Destroy(topHealthSlider.gameObject);
		Destroy(playerNameText.gameObject);

		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 1;
		Destroy(GameObject.Find("LoadingBar"), 0.25f);

		Application.targetFrameRate = 120;
		Chat.Get.Log($"Player {OwnerClientId} logged in");

		ChooseSide();
	}

	private IEnumerator InitGameManager()
	{
		yield return new WaitUntil(() => GameManager.Get != null);
		GameManager.Get.Init();

		if (IsOwner)
			InitAllPlayers();
	}

	private void InitAllPlayers()
	{
		// Setting players' names on new player's pc
		foreach (Player player in GameManager.Get.GetPlayers())
			player.playerNameText.text = player.playerName.Value.ToString();

		// Setting other players' cards on new player's pc
		foreach (GameObject cardGo in GameObject.FindGameObjectsWithTag("Card"))
		{
			Card card = cardGo.GetComponent<Card>();
			ulong cardID = card.OwnerClientId;

			cardGo.name = $"Card{cardID}";
			GameManager.Get.GetPlayerByID(cardID).card = card;
			card.SetPlayerForNonServer(GameManager.Get.GetPlayerByID(cardID).transform);
		}

		// Setting other players' sliders' height and max value on new player's pc (only if the other player chose a card already) 
		foreach (GameObject topSliderGo in GameObject.FindGameObjectsWithTag("TopSlider"))
		{
			Player player = topSliderGo.transform.parent.parent.GetComponent<Player>();
			if (player != this && player.card != null)
			{
				player.currentHealthSlider = player.topHealthSlider;
				NetworkQuery.Instance.Request<float>($"Get Canvas Height {player.OwnerClientId}",
					height =>
					{
						player.currentHealthSlider.transform.parent.localPosition = new Vector3(0, height, 0);
					});
				player.currentHealthSlider.maxValue = Cards.CardParams[player.card.GetCardName()].health;
				player.currentHealthSlider.value = player.card.GetHealth();
			}
		}
	}

	#endregion

	#region Name

	/// <returns>
	///     Returns the name of the player on EVERYONE.
	/// </returns>
	public string GetPlayerName()
	{
		return playerName.Value.ToString();
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetPlayerNameServerRpc(string name)
	{
		playerName.Value = name;
	}

	[Rpc(SendTo.Everyone)]
	public void UpdatePlayerNameTextRpc()
	{
		playerNameText.text = playerName.Value.ToString();
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
		SetPlayerNameServerRpc(playerSettings.playerName);
		GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Lens.FieldOfView = playerSettings.FOV;
	}

	/// <summary>
	///     Loads player settings on OWNER.
	/// </summary>
	private void LoadSettings()
	{
		PlayerSettings loadedSettings = new();
		loadedSettings.playerName = PlayerPrefs.GetString("playerName", $"Player {OwnerClientId}");
		loadedSettings.volume = PlayerPrefs.GetFloat("volume", 1);
		loadedSettings.mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 1.5f);
		loadedSettings.quality = PlayerPrefs.GetInt("quality", 0);
		loadedSettings.FOV = PlayerPrefs.GetFloat("FOV", 90);

		UpdateGameToSettings(loadedSettings);
	}

	#endregion

	#region SideSelection

	/// <summary>
	///     Opens side selection menu. Can be called from OWNER.
	/// </summary>
	public void ChooseSide()
	{
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
		UpdateSideServerRpc(side);
		RespawnRpc(false);
	}

	/// <returns>
	///     Returns the side (blue/red) of this player. Works on EVERYONE.
	/// </returns>
	public Side GetSide()
	{
		return side.Value;
	}

	/// <summary>
	///     Updates side of player on SERVER.
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
	private void UpdateSideServerRpc(Side side)
	{
		this.side.Value = side;
	}

	#endregion

	#region CardCreation

	/// <summary>
	///     Resets camera and shows card selection screen to respawn on OWNER.
	/// </summary>
	[Rpc(SendTo.Owner)]
	public void RespawnRpc(bool delay = true)
	{
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
		StartCoroutine(ChooseCard(cardName));
	}

	private IEnumerator ChooseCard(string cardName)
	{
		SetCameraOnCardCreationRpc();

		if (card != null)
		{
			// DespawnCardRpc();
			model.GetComponent<NetworkObject>().Despawn();
			card.GetComponent<NetworkObject>().Despawn();

			yield return new WaitUntil(() => card == null && model == null);
		}

		movementController.TeleportRpc(new Vector3(0, 2, side.Value == Side.Blue ? -34 : 34),
			Quaternion.Euler(0, side.Value == Side.Blue ? 0 : 180, 0));

		// SpawnCardRpc(cardName);
		GameObject cardGo = Instantiate(Cards.CardPrefabs[cardName]);
		cardGo.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

		model = Instantiate(Cards.CardParams[cardName].modelPrefab).transform;
		model.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

		SetCardRpc();
	}

	[Rpc(SendTo.Owner)]
	private void SetCameraOnCardCreationRpc()
	{
		GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = movementController.GetCameraTransform();
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
			Debug.Log($"Didn't find card of player {OwnerClientId} as {NetworkManager.LocalClientId}");
		else
		{
			//Set the card to the matching player on each pc
			card.gameObject.name = $"Card{OwnerClientId}";
			this.card = card;
			card.SetPlayerForNonServer(transform);
		}

		SetModelRpc();
	}

	[Rpc(SendTo.Owner)]
	private void SetModelRpc()
	{
		movementController.SetModel();
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
			currentHealthSlider.maxValue = Cards.CardParams[card.GetCardName()].health;
		}
		else
		{
			//Setting the player that just chose a card slider's height and max value on other players' machines'
			currentHealthSlider = topHealthSlider;
			NetworkQuery.Instance.Request<float>($"Get Canvas Height {OwnerClientId}",
				height => { currentHealthSlider.transform.parent.localPosition = new Vector3(0, height, 0); });
			currentHealthSlider.maxValue = Cards.CardParams[card.GetCardName()].health;
		}

		StartCardServerRpc();
	}

	private int clientReadyCounter;

	[ServerRpc(RequireOwnership = false)]
	private void StartCardServerRpc()
	{
		clientReadyCounter++;

		if (clientReadyCounter == GameManager.Get.GetPlayers().Count)
		{
			GameManager.Get.GetPlayerByID(OwnerClientId).card.StartCard(transform);
			clientReadyCounter = 0;
			SpawnedRpc();
		}
	}

	[Rpc(SendTo.Everyone)]
	private void SpawnedRpc()
	{
		spawned = true;
	}

	#endregion
}