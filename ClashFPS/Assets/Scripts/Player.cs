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
	[SerializeField] private TextMeshProUGUI playerName;
	[SerializeField] private NetworkObject chatNetworkHelper;
	[SerializeField] private float timeToRespawn = 5f;
	private Animator _animator;

	private Card _card;
	private CardSelection _cardSelection;
	private int _elixr;
	private int _jumpsLeft;
	private Vector3 _resetedCameraPosition;
	private Quaternion _resetedCameraRotation;
	private Side _side;
	private bool _spawned;
	private float _yVelocity;

	private void Update()
	{
		if (!IsOwner || !_spawned)
			return;

		_card.UpdateCard();
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (IsOwner && hit.gameObject.CompareTag("WaterCols"))
			_card.DamageRpc(Mathf.Infinity);
	}

	public override void OnNetworkSpawn()
	{
		playerName.text = $"Player {OwnerClientId}";
		healthSlider.name = $"Slider{OwnerClientId}";

		_side = (Side)(OwnerClientId % 2);

		if (!IsOwner)
			return;

		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 0.75f;

		if (IsServer)
		{
			chatNetworkHelper = Instantiate(chatNetworkHelper.gameObject).GetComponent<NetworkObject>();
			chatNetworkHelper.Spawn();
		}

		chatNetworkHelper = GameObject.Find("ChatNetworkHelper(Clone)").GetComponent<NetworkObject>();
		Chat.Singleton.EnableChatNetworking(chatNetworkHelper.GetComponent<ChatNetworkHelper>());
		Chat.Singleton.Log($"Player {OwnerClientId} logged in");

		GameObject.Find("LoadingBar").GetComponent<Slider>().value = 1;
		Destroy(GameObject.Find("LoadingBar"), 0.25f);

		Destroy(healthSlider.gameObject);
		Destroy(playerName.gameObject);

		Application.targetFrameRate = 120;

		_resetedCameraPosition = GameObject.Find("CineCam").transform.position;
		_resetedCameraRotation = GameObject.Find("CineCam").transform.rotation;

		_cardSelection = FindFirstObjectByType<CardSelection>();
		_cardSelection.SetPlayerScript(this);
		Respawn(false);
	}

	public Card GetCard()
	{
		return _card;
	}

	public Side GetSide()
	{
		return _side;
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

			Chat.Singleton.Log(
				$"Starting card {i} with side {GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<Player>().GetSide()}");

			card.StartCard(GameObject.FindGameObjectsWithTag("Player")[i].transform,
				GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<Player>().GetSide(), $"Slider{i}");
		}
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

		if (Input.GetButtonDown("Jump"))
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
		Vector3 movementDir = new(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

		_yVelocity += Physics.gravity.y * Time.deltaTime;
		controller.Move(transform.right * (movementDir.x * speed * Time.deltaTime)
		                + Vector3.up * (_yVelocity * Time.deltaTime)
		                + transform.forward * (movementDir.z * speed * Time.deltaTime));

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
		transform.localEulerAngles = new Vector3(0, transform.rotation.eulerAngles.y + Input.GetAxis("Mouse X"), 0);

		float xAngle = cameraFollow.rotation.eulerAngles.x;
		if (xAngle >= 180)
			xAngle -= 360;

		cameraFollow.localEulerAngles = new Vector3(Mathf.Clamp(xAngle - Input.GetAxis("Mouse Y"), -40, 75), 0, 0);
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