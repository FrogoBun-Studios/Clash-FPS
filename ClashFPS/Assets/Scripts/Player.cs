using System.Collections;

using TMPro;

using Unity.Cinemachine;
using Unity.Netcode;

using UnityEngine;
using UnityEngine.UI;


public class Player : NetworkBehaviour
{
	[SerializeField] private CharacterController controller;
	[SerializeField] private Transform cameraFollow;
	[SerializeField] private Slider healthSlider;
	[SerializeField] private TextMeshProUGUI playerNameText;
	[SerializeField] private NetworkObject gameManager;
	[SerializeField] private NetworkObject chatNetworkHelper;
	[SerializeField] private float timeToRespawn;
	[SerializeField] private float sensitivity;
	[SerializeField] private float acceleration;
	private Animator _animator;
	private Card _card;
	private CardSelection _cardSelection;
	private int _elixir = 5;
	private int _jumpsLeft;
	private string _playerName;
	private Vector3 _resetedCameraPosition;
	private Quaternion _resetedCameraRotation;
	private SettingsMenu _settingsMenu;
	private Side _side;
	private SideSelection _sideSelection;
	private bool _spawned;
	private SlewRateLimiter _xAccelerationLimiter;
	private float _yVelocity;
	private SlewRateLimiter _zAccelerationLimiter;
	public PlayerSettings PlayerSettings;

	private void Update()
	{
		if (!IsOwner)
			return;

		if (Input.GetKeyDown(KeyCode.Escape) && !_sideSelection.IsShowen() && !_cardSelection.IsShowen())
		{
			if (_settingsMenu.IsShowen())
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;

				StartCoroutine(_settingsMenu.Hide());
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;

				StartCoroutine(_settingsMenu.Show());
			}
		}

		if (_card != null)
			_card.UpdateCard(_spawned);
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (IsOwner && hit.gameObject.CompareTag("WaterCols"))
			_card.Damage(Mathf.Infinity);
	}

	public override void OnNetworkSpawn()
	{
		healthSlider.name = $"Slider{OwnerClientId}";

		if (!IsOwner)
			return;

		_xAccelerationLimiter = new SlewRateLimiter(acceleration);
		_zAccelerationLimiter = new SlewRateLimiter(acceleration);

		LoadSettings();

		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 0.75f;

		if (IsServer)
		{
			gameManager = Instantiate(gameManager.gameObject).GetComponent<NetworkObject>();
			gameManager.Spawn();

			chatNetworkHelper = Instantiate(chatNetworkHelper.gameObject).GetComponent<NetworkObject>();
			chatNetworkHelper.Spawn();
		}

		gameManager = GameObject.Find("GameManager(Clone)").GetComponent<NetworkObject>();

		chatNetworkHelper = GameObject.Find("ChatNetworkHelper(Clone)").GetComponent<NetworkObject>();
		Chat.Get.EnableChatNetworking(chatNetworkHelper.GetComponent<ChatNetworkHelper>(), this);
		Chat.Get.Log($"Player {OwnerClientId} logged in");

		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 1;
		Destroy(GameObject.Find("LoadingBar"), 0.25f);

		Destroy(healthSlider.gameObject);
		Destroy(playerNameText.gameObject);

		Application.targetFrameRate = 120;

		_resetedCameraPosition = GameObject.Find("CineCam").transform.position;
		_resetedCameraRotation = GameObject.Find("CineCam").transform.rotation;

		_sideSelection = FindFirstObjectByType<SideSelection>();
		_sideSelection.Set(this);
		_cardSelection = FindFirstObjectByType<CardSelection>();
		_cardSelection.Set(this);
		_settingsMenu = FindFirstObjectByType<SettingsMenu>();
		_settingsMenu.Set(this);

		ChooseSide();
	}

	public void UpdateSettings(PlayerSettings playerSettings)
	{
		PlayerSettings = playerSettings;
		sensitivity = playerSettings.mouseSensitivity;
		UpdateNameRpc(playerSettings.playerName);
		GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Lens.FieldOfView = playerSettings.FOV;
	}

	private void LoadSettings()
	{
		PlayerSettings loadedSettings = new();
		loadedSettings.playerName = PlayerPrefs.GetString("playerName", $"Player {OwnerClientId}");
		loadedSettings.volume = PlayerPrefs.GetFloat("volume", 1);
		loadedSettings.mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 1.5f);
		loadedSettings.quality = PlayerPrefs.GetInt("quality", 0);
		loadedSettings.FOV = PlayerPrefs.GetFloat("FOV", 90);

		UpdateSettings(loadedSettings);
	}

	[Rpc(SendTo.Everyone)]
	public void UpdateNameRpc(string newName)
	{
		_playerName = newName;

		if (!IsOwner)
			playerNameText.text = _playerName;
	}

	public void ChooseSide()
	{
		_spawned = false;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		ResetCamera();

		StartCoroutine(_sideSelection.Show());
	}

	public void SetSide(Side side)
	{
		UpdateSideRpc(side);
		Respawn(false);
	}

	[Rpc(SendTo.Everyone)]
	public void UpdateSideRpc(Side side)
	{
		_side = side;
	}

	public string GetPlayerName()
	{
		return _playerName;
	}

	public Card GetCard()
	{
		return _card;
	}

	public Side GetSide()
	{
		return _side;
	}

	public int GetElixir()
	{
		return _elixir;
	}

	public void SpendElixir(int amount)
	{
		_elixir -= amount;
	}

	public void EarnElixir(int amount)
	{
		_elixir += amount;
	}

	public void SetCard(Card card)
	{
		_card = card;
	}

	public Quaternion GetCameraRotation()
	{
		return cameraFollow.rotation;
	}

	public Vector3 GetCameraForward()
	{
		return cameraFollow.forward;
	}

	public void Spawned()
	{
		_spawned = true;
	}

	private void ResetCamera()
	{
		GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = null;
		GameObject.Find("CineCam").transform.position = _resetedCameraPosition;
		GameObject.Find("CineCam").transform.rotation = _resetedCameraRotation;
	}

	[Rpc(SendTo.Everyone)]
	public void SetColliderSizeRpc(float radius, float height, float yOffset)
	{
		controller.radius = radius;
		controller.height = height;
		controller.center = Vector3.up * yOffset;
	}

	[Rpc(SendTo.Everyone)]
	public void EnableColliderRpc(bool enable)
	{
		controller.enabled = enable;
	}

	public void SetCameraFollow(Vector3 pos)
	{
		cameraFollow.localPosition = pos;
	}

	#region CardCreation

	public void Respawn(bool delay = true)
	{
		_spawned = false;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		ResetCamera();

		StartCoroutine(_cardSelection.Show(delay ? timeToRespawn : 0));
	}

	public IEnumerator ChooseCard(string cardName)
	{
		GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = cameraFollow;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		if (_card != null)
		{
			_card.DespawnCardRpc();
			yield return new WaitUntil(() => _card == null);
		}

		Teleport(new Vector3(0, 2, _side == Side.Blue ? -34 : 34),
			Quaternion.Euler(0, _side == Side.Blue ? 0 : 180, 0));
		SpawnCardRpc(cardName);
	}

	[Rpc(SendTo.Server)]
	private void SpawnCardRpc(string cardName)
	{
		GameObject card = Instantiate(Cards.CardPrefabs[cardName]);
		card.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

		SetCardRpc();
	}

	[Rpc(SendTo.Everyone)]
	private void SetCardRpc()
	{
		foreach (GameObject cardGo in GameObject.FindGameObjectsWithTag("Card"))
		{
			int i = (int)cardGo.GetComponent<NetworkObject>().OwnerClientId;

			cardGo.name = $"Card{i}";
			Card card = cardGo.GetComponent<Card>();
			GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<Player>().SetCard(card);

			Chat.Get.Log(
				$"Starting card {i} with side {GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<Player>().GetSide()}");

			card.StartCard(GameObject.FindGameObjectsWithTag("Player")[i].transform,
				GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<Player>().GetSide(), $"Slider{i}");
		}

		EnableColliderRpc(true);
	}

	[Rpc(SendTo.Everyone)]
	public void SetAnimatorRpc(string modelName)
	{
		_animator = GameObject.Find(modelName).GetComponent<Animator>();
	}

	#endregion

	#region Movement

	public void ControlCharacter(float speed, int jumps, float jumpStrength)
	{
		Move(speed);
		Look();

		if (Input.GetButtonDown("Jump") && _spawned && !_settingsMenu.IsShowen())
		{
			if (controller.isGrounded)
				_jumpsLeft = jumps;

			if (_jumpsLeft > 0)
			{
				_yVelocity = jumpStrength;
				_animator.SetTrigger("Jump");
				_jumpsLeft--;
			}
		}
	}

	private void Move(float speed)
	{
		Vector3 movementDir = new();
		if (_spawned && !_settingsMenu.IsShowen())
			movementDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

		// _xAccelerationLimiter.SetRateLimit(controller.isGrounded ? acceleration : acceleration / 5f);
		// _zAccelerationLimiter.SetRateLimit(controller.isGrounded ? acceleration : acceleration / 5f);
		_xAccelerationLimiter.SetRateLimit(Mathf.Infinity);
		_zAccelerationLimiter.SetRateLimit(Mathf.Infinity);

		float xMove = _xAccelerationLimiter.Calculate(movementDir.x * speed) * Time.deltaTime;
		_yVelocity += Physics.gravity.y * Time.deltaTime;
		float zMove = _zAccelerationLimiter.Calculate(movementDir.z * speed) * Time.deltaTime;

		controller.Move(transform.right * xMove
		                + Vector3.up * (_yVelocity * Time.deltaTime)
		                + transform.forward * zMove);

		if (controller.isGrounded)
			_yVelocity = 0;

		_animator.SetBool("Moving", movementDir != Vector3.zero);
		if (movementDir.z != 0)
			_animator.SetFloat("Speed", Utils.MagnitudeInDirection(controller.velocity, transform.forward) / 6.6f);
		else
			_animator.SetFloat("Speed",
				Mathf.Abs(Utils.MagnitudeInDirection(controller.velocity, transform.right)) >= 0.2f ? 1 : 0);
	}

	private void Look()
	{
		if (!_spawned || _settingsMenu.IsShowen())
			return;

		transform.localEulerAngles =
			new Vector3(0, transform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivity, 0);

		float xAngle = cameraFollow.rotation.eulerAngles.x;
		if (xAngle >= 180)
			xAngle -= 360;

		cameraFollow.localEulerAngles =
			new Vector3(Mathf.Clamp(xAngle - Input.GetAxis("Mouse Y") * sensitivity, -40, 75), 0, 0);
	}

	private void Teleport(Vector3 position, Quaternion rotation)
	{
		controller.enabled = false;
		transform.position = position;
		transform.rotation = rotation;
		controller.enabled = true;
	}

	private void Teleport(Vector3 position)
	{
		controller.enabled = false;
		transform.position = position;
		controller.enabled = true;
	}

	#endregion
}