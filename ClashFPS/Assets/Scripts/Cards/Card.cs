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
		attackTimer = 1 / cardParams.attackRate;
	}

	/// <summary>
	///     This card updates and moves on OWNER
	/// </summary>
	public virtual void UpdateCard()
	{
		if (GetHealth() <= 0)
			return;

		playerScript.UpdateElixirServerRpc(Time.deltaTime * 0.25f);

		movementController.ControlCharacter(cardParams.speed, cardParams.jumps, cardParams.jumpStrength);
		attackTimer -= Time.deltaTime;
		if (Input.GetButtonDown("Fire") && attackTimer <= 0)
		{
			attackTimer = 1 / cardParams.attackRate;
			AttackServerRpc();
		}
	}

	/// <summary>
	///     This card attacks on SERVER
	/// </summary>
	[ServerRpc]
	protected virtual void AttackServerRpc()
	{
		movementController.SetAnimatorTriggerServerRpc("Attack");
	}

	/// <summary>
	///     This card damaging a tower on EVERYONE because the towers aren't networked
	/// </summary>
	[Rpc(SendTo.Everyone)]
	protected void DamageTowerRpc(string towerName)
	{
		Tower t = GameObject.Find(towerName).GetComponent<Tower>();

		NetworkQuery.Instance.Request<int>($"Get Side {OwnerClientId}", side =>
		{
			if (t.GetSide() != (Side)side)
				if (t.Damage(cardParams.damage))
					playerScript.UpdateElixirServerRpc(10);
		});
	}

	/// <summary>
	///     Damage this card on SERVER
	/// </summary>
	[ServerRpc]
	public void DamageServerRpc(ulong sourcePlayerID, float amount)
	{
		if (GetHealth() <= 0)
			return;

		health.Value -= amount;
		playerScript.UpdateHealthSliderRpc(GetHealth());
		if (GetHealth() <= 0)
		{
			if (sourcePlayerID != 999999999ul)
				GameManager.Get.GetPlayerByID(sourcePlayerID).GetCard().KilledPlayer(playerScript);
			OnDeath();
		}
	}

	/// <summary>
	///     Runs when this card killed another card on SERVER
	/// </summary>
	protected void KilledPlayer(Player killedPlayer)
	{
		playerScript.UpdateElixirServerRpc(3);
		Chat.Get.KillLog(playerScript.GetPlayerName(), killedPlayer.GetPlayerName(), cardParams.cardName);
	}

	/// <summary>
	///     Runs when this card dies on SERVER
	/// </summary>
	protected virtual void OnDeath()
	{
		movementController.SetAnimatorTriggerServerRpc("Death");
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
	public string GetCardName()
	{
		return cardParams.cardName;
	}
}