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

	private Side side;
	private Card card;

	private readonly NetworkVariable<FixedString32Bytes> playerName =
		new(writePerm: NetworkVariableWritePermission.Owner);

	private float elixir = 5;

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
			card.UpdateCard();
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (IsServer && hit.gameObject.CompareTag("WaterCols"))
			card.DamageServerRpc(999ul, Mathf.Infinity);
	}

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
		}

		if (IsServer)
		{
			NetworkQuery.Instance.Register($"Get Elixir {OwnerClientId}", _ => elixir);
			NetworkQuery.Instance.Register($"Get Side {OwnerClientId}", _ => (int)side);
			NetworkQuery.Instance.Register($"Get Top Slider Height {OwnerClientId}",
				_ => model.localScale.y * 4f + 2.1f);
		}

		topHealthSlider.name = $"TopSlider{OwnerClientId}";
		playerName.OnValueChanged += (value, newValue) => playerNameText.text = newValue.ToString();

		if (!IsOwner)
			return;

		GameManager.Get.InitOnOwner();

		LoadSettings();

		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 0.75f;

		chatNetworkHelper = GameObject.Find("ChatNetworkHelper(Clone)").GetComponent<NetworkObject>();
		Chat.Get.EnableChatNetworking(chatNetworkHelper.GetComponent<ChatNetworkHelper>(), this);
		Chat.Get.Log($"Player {OwnerClientId} logged in");

		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 1;
		Destroy(GameObject.Find("LoadingBar"), 0.25f);

		Destroy(topHealthSlider.gameObject);
		Destroy(playerNameText.gameObject);

		Application.targetFrameRate = 120;

		sideSelection = FindFirstObjectByType<SideSelection>();
		sideSelection.Set(this);
		cardSelection = FindFirstObjectByType<CardSelection>();
		cardSelection.Set(this);
		settingsMenu = FindFirstObjectByType<SettingsMenu>();
		settingsMenu.Set(this);

		ChooseSide();
	}

	/// <summary>
	///     Adds the amount to the elixir of the player on SERVER.
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
	public void UpdateElixirServerRpc(float amount)
	{
		elixir += amount;
	}

	/// <summary>
	///     Updates game to the given settings (makes the settings actually work) on OWNER.
	/// </summary>
	public void UpdateGameToSettings(PlayerSettings playerSettings)
	{
		this.playerSettings = playerSettings;
		movementController.UpdateSensitivity(playerSettings.mouseSensitivity);
		playerName.Value = playerSettings.playerName;
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

	/// <summary>
	///     Opens side selection menu. Can be called from EVERYONE.
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
	///     Returns the side (blue/red) of this player. Only works on SERVER.
	/// </returns>
	public Side GetSide()
	{
		return side;
	}

	/// <summary>
	///     Updates side of player on SERVER.
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
	public void UpdateSideServerRpc(Side side)
	{
		this.side = side;
	}

	/// <returns>
	///     Returns the name of the player on EVERYONE.
	/// </returns>
	public string GetPlayerName()
	{
		return playerName.Value.ToString();
	}

	/// <returns>
	///     Returns the card of this player on EVERYONE.
	///     If you are not in the server, this card is brain-dead, and exists to call server RPCs.
	/// </returns>
	public Card GetCard()
	{
		return card;
	}

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

		cardSelection.Show(delay ? timeToRespawn : 0);
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

		movementController.TeleportServerRpc(new Vector3(0, 2, side == Side.Blue ? -34 : 34),
			Quaternion.Euler(0, side == Side.Blue ? 0 : 180, 0));

		// SpawnCardRpc(cardName);
		GameObject cardGo = Instantiate(Cards.CardPrefabs[cardName]);
		cardGo.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

		model = Instantiate(Cards.CardParams[cardName].modelPrefab).transform;
		model.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

		SetCardsRpc(OwnerClientId);
	}

	[Rpc(SendTo.Owner)]
	public void SetCameraOnCardCreationRpc()
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
	public void SetCardsRpc(ulong newPlayerID)
	{
		foreach (GameObject cardGo in GameObject.FindGameObjectsWithTag("Card"))
		{
			Card card = cardGo.GetComponent<Card>();
			ulong cardID = card.OwnerClientId;

			cardGo.name = $"Card{cardID}";
			GameManager.Get.GetPlayerByID(cardID).card = card;

			if (IsServer && cardID == newPlayerID)
				card.StartCard(transform);
		}

		SetModel();
	}

	public void SetModel()
	{
		movementController.SetModel();
		movementController.EnableColliderRpc(true);

		SetHealthSliders();
	}

	/// <summary>
	///     Sets for every player, every player's health slider, runs on EVERYONE.
	///     <br /><br />For example,
	///     <br />If a third player joins, he will set the health sliders of the 2 other players on his machine,
	///     <br />and set his own health slider on his machine to be the ui one instead of the top one.
	///     <br />Every other player will do the same.
	/// </summary>
	public void SetHealthSliders()
	{
		if (IsOwner)
		{
			currentHealthSlider = GameObject.Find("HealthSliderUI").GetComponent<Slider>();
			currentHealthSlider.maxValue = Cards.CardParams[card.GetCardName()].health;
			currentHealthSlider.value = card.GetHealth();
		}

		foreach (GameObject topSliderGo in GameObject.FindGameObjectsWithTag("TopSlider"))
		{
			Player player = topSliderGo.transform.parent.parent.GetComponent<Player>();
			player.currentHealthSlider = topSliderGo.GetComponent<Slider>();
			NetworkQuery.Instance.Request<float>($"Get Top Slider Height {player.OwnerClientId}", height =>
			{
				player.currentHealthSlider.transform.parent.position = new Vector3(
					currentHealthSlider.transform.parent.position.x,
					height, currentHealthSlider.transform.parent.position.z
				);
			});

			player.currentHealthSlider.maxValue = Cards.CardParams[player.card.GetCardName()].health;
			player.currentHealthSlider.value = card.GetHealth();
		}

		spawned = true;
	}

	// [Rpc(SendTo.Server)]
	// public void DespawnCardRpc()
	// {
	// 	_model.GetComponent<NetworkObject>().Despawn();
	// 	Card.GetComponent<NetworkObject>().Despawn();
	// }

	// [Rpc(SendTo.Server)]
	// private void SpawnCardRpc(string cardName)
	// {
	// 	GameObject card = Instantiate(Cards.CardPrefabs[cardName]);
	// 	card.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);
	//
	// 	GameObject model = Instantiate(Cards.CardParams[cardName].modelPrefab);
	// 	model.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);
	//
	// 	SetCardRpc(cardName);
	// }

	// [Rpc(SendTo.Everyone)]
	// private void SetCardRpc(string cardName)
	// {
	// 	foreach (GameObject cardGo in GameObject.FindGameObjectsWithTag("Card"))
	// 	{
	// 		int i = (int)cardGo.GetComponent<NetworkObject>().OwnerClientId;
	//
	// 		cardGo.name = $"Card{i}";
	// 		Card card = cardGo.GetComponent<Card>();
	// 		Player player = GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<Player>();
	// 		player.Card = card;
	//
	// 		if (card.IsOwner)
	// 		{
	// 			if (!card.Started)
	// 				card.StartCard(player.transform);
	// 		}
	// 		else
	// 		{
	// 			card.SetCardForNonOwners(player.transform);
	// 		}
	// 	}
	//
	// 	EnableColliderRpc(true);
	// 	SetModel(cardName);
	// }

	// private void SetModel(string cardName)
	// {
	// 	foreach (GameObject model in GameObject.FindGameObjectsWithTag("Model"))
	// 		model.name = $"Model{model.GetComponent<NetworkObject>().OwnerClientId}";
	//
	// 	_model = GameObject.Find($"Model{OwnerClientId}").transform;
	// 	_animator = _model.GetComponent<Animator>();
	//
	// 	SetCameraFollow(new Vector3(0, 4.625f * _model.localScale.y - 2.375f,
	// 		-2.5f * _model.localScale.y + 2.5f));
	// 	SetHealthSlider(cardName);
	// }

	#endregion

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
}