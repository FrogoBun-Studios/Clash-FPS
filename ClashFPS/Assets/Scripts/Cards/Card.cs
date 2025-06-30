using Unity.Netcode;

using UnityEngine;


public abstract class Card : NetworkBehaviour
{
	[SerializeField] protected CardParams cardParams;

	protected NetworkVariable<float> health = new();
	protected Transform player;
	protected Player playerScript;
	protected float attackTimer;
	protected MovementController movementController;

	/// <summary>
	///     Start and setup card on SERVER
	/// </summary>
	public virtual void StartCard(Transform player)
	{
		this.player = player;
		playerScript = player.GetComponent<Player>();
		movementController = player.GetComponent<MovementController>();

		health.Value = cardParams.health;
		playerScript.UpdateHealthSliderRpc(health.Value);
		movementController.SetColliderSizeRpc(cardParams.colliderRadius, cardParams.colliderHeight,
			cardParams.colliderYOffset);

		Debug.Log($"Player {OwnerClientId} card started");
	}

	public void SetPlayerForNonServer(Transform player)
	{
		this.player = player;
		playerScript = player.GetComponent<Player>();
		movementController = player.GetComponent<MovementController>();
	}

	/// <summary>
	///     This card updates and moves on OWNER
	/// </summary>
	public virtual void UpdateCard(bool enableCardControl)
	{
		if (GetHealth() <= 0)
			return;

		playerScript.UpdateElixirServerRpc(Time.deltaTime * Constants.elixirPerSecond);

		movementController.ControlCharacter(cardParams.speed, cardParams.jumps, cardParams.jumpStrength);
		attackTimer -= Time.deltaTime;
		if (enableCardControl && Input.GetButtonDown("Fire") && attackTimer <= 0)
		{
			attackTimer = 1 / cardParams.attackRate;
			Attack();
		}
	}

	/// <summary>
	///     This card attacks. Runs by OWNER, supposed to run on SERVER, that means this func will call server RPCs.
	/// </summary>
	protected virtual void Attack()
	{
		Debug.Log("Attacking");
		movementController.SetAnimatorTriggerRpc("Attack");
	}

	/// <summary>
	///     Damage this card on SERVER
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
	public void DamageServerRpc(ulong sourcePlayerID, float amount)
	{
		if (GetHealth() <= 0)
			return;

		health.Value -= amount;
		Debug.Log($"Player {OwnerClientId} damaged by {amount} to {GetHealth()}");

		playerScript.UpdateHealthSliderRpc(GetHealth());
		if (GetHealth() <= 0)
		{
			if (sourcePlayerID != Constants.nonPlayerID)
				GameManager.Get.GetPlayerByID(sourcePlayerID).GetCard()
					.OnKilledPlayerServerRpc(playerScript.OwnerClientId);
			OnDeath();
		}
	}

	/// <summary>
	///     Runs when this card killed another card on SERVER
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
	protected void OnKilledPlayerServerRpc(ulong killedPlayerID)
	{
		Player killedPlayer = GameManager.Get.GetPlayerByID(killedPlayerID);
		killedPlayer.UpdateElixirServerRpc(Constants.elixirPerKill);
		Chat.Get.KillLog(playerScript.GetPlayerData().name.ToString(), killedPlayer.GetPlayerData().name.ToString(),
			cardParams.cardName);
	}

	/// <summary>
	///     Runs when this card destroyed a tower on SERVER
	/// </summary>
	public void OnDestroyedTower()
	{
		Chat.Get.KillLog(playerScript.GetPlayerData().name.ToString(), "tower", cardParams.cardName);
		playerScript.UpdateElixirServerRpc(Constants.elixirPerTower);
	}

	/// <summary>
	///     Runs when this card dies on SERVER
	/// </summary>
	protected virtual void OnDeath()
	{
		Debug.Log($"Player {OwnerClientId} died");
		movementController.SetAnimatorTriggerRpc("Death");
		movementController.EnableColliderRpc(false);
		playerScript.RespawnRpc();
	}

	/// <returns>
	///     Returns the health of the card, works on EVERYONE
	/// </returns>
	public float GetHealth()
	{
		return health.Value;
	}

	/// <returns>
	///     Returns the name of the card (Wizard, Giant...), works on EVERYONE
	/// </returns>
	public CardParams GetParams()
	{
		return cardParams;
	}
}