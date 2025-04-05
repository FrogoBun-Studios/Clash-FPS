using Unity.Netcode;

using UnityEngine;


public abstract class Card : NetworkBehaviour
{
	[SerializeField] protected CardParams cardParams;

	protected NetworkVariable<float> _health = new(writePerm: NetworkVariableWritePermission.Owner);
	protected Transform _player;
	protected Player _playerScript;
	protected float _attackTimer;
	public bool Started { get; private set; }

	public virtual void StartCard(Transform player)
	{
		Started = true;

		_player = player;
		_playerScript = player.GetComponent<Player>();

		_health.Value = cardParams.health;

		_playerScript.UpdateHealthSliderRpc(_health.Value);

		_playerScript.SetColliderSizeRpc(cardParams.colliderRadius, cardParams.colliderHeight,
			cardParams.colliderYOffset);
		_attackTimer = 1 / cardParams.attackRate;
	}

	public void SetCardForNonOwners(Transform player)
	{
		_player = player;
		_playerScript = player.GetComponent<Player>();
	}

	public virtual void UpdateCard(bool spawned)
	{
		if (GetHealth() <= 0 || !spawned)
			return;

		_playerScript.Elixir += Time.deltaTime * 0.25f;

		_playerScript.ControlCharacter(cardParams.speed, cardParams.jumps, cardParams.jumpStrength);

		_attackTimer -= Time.deltaTime;
		if (Input.GetButtonDown("Fire") && _attackTimer <= 0)
		{
			_attackTimer = 1 / cardParams.attackRate;
			Attack();
		}
	}

	protected virtual void Attack()
	{
		_playerScript.SetAnimatorTrigger("Attack");
	}

	[Rpc(SendTo.Everyone)]
	protected void DamageTowerRpc(string towerName)
	{
		Tower t = GameObject.Find(towerName).GetComponent<Tower>();

		if (t.GetSide() != _playerScript.Side)
		{
			if (t.Damage(cardParams.damage))
				_playerScript.Elixir += 10;
		}
	}

	[Rpc(SendTo.Owner)]
	public void DamageRpc(float amount)
	{
		if (GetHealth() <= 0)
			return;

		Chat.Get.Log($"{_health.Value}");
		_health.Value -= amount;
		Chat.Get.Log($"{_health.Value}");
		_playerScript.UpdateHealthSliderRpc(GetHealth());
		if (GetHealth() <= 0)
			OnDeath();
	}

	public void KilledPlayer(Player killedPlayer)
	{
		_playerScript.Elixir += 3;
		Chat.Get.KillLog(_playerScript.GetPlayerName(), killedPlayer.GetPlayerName(), cardParams.cardName);
	}

	protected virtual void OnDeath()
	{
		Started = false;
		_playerScript.SetAnimatorTrigger("Death");

		_playerScript.EnableColliderRpc(false);
		_playerScript.Respawn();
	}

	public float GetHealth()
	{
		return _health.Value;
	}
}