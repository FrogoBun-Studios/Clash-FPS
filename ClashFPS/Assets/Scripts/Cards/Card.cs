using System.Collections;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.UI;


public abstract class Card : NetworkBehaviour
{
	[SerializeField] protected CardParams cardParams;
	private Animator _animator;
	protected float _attackTimer;
	protected float _elixirEarned;

	protected float _health;
	private Slider _healthSlider;
	private Transform _model;
	private GameObject _modelPrefab;
	protected Transform _player;
	protected Player _playerScript;
	protected Side _side;
	private bool _started;
	private string _topSliderName;

	public virtual void StartCard(Transform player, Side side, string topSlider)
	{
		_side = side;
		_modelPrefab = cardParams.modelPrefab;
		_player = player;
		_playerScript = player.GetComponent<Player>();
		_elixirEarned = 0f;
		_topSliderName = topSlider;
		_health = cardParams.health;

		if (!IsOwner)
			return;

		if (_started)
		{
			SetModelRpc();
			return;
		}

		_started = true;

		_playerScript.SetColliderSizeRpc(cardParams.colliderRadius, cardParams.colliderHeight,
			cardParams.colliderYOffset);
		_attackTimer = 1 / cardParams.attackRate;

		CreateModelRpc();
	}

	public virtual void UpdateCard()
	{
		if (_health <= 0)
			return;

		_elixirEarned += Time.deltaTime * 0.25f;

		_playerScript.ControlCharacter(cardParams.speed, cardParams.jumps, cardParams.jumpStrength);

		_attackTimer -= Time.deltaTime;
		if (Input.GetButtonDown("Fire") && _attackTimer <= 0)
		{
			_attackTimer = 1 / cardParams.attackRate;
			Attack();
		}

		_model.position = _player.position;
		_model.localEulerAngles = _player.localEulerAngles;
	}

	[Rpc(SendTo.Server)]
	public void DespawnCardRpc()
	{
		_model.GetComponent<NetworkObject>().Despawn();
		GetComponent<NetworkObject>().Despawn();
	}

	#region Misc

	public void SetSliders(string topSlider)
	{
		if (!IsOwner)
		{
			_healthSlider = GameObject.Find(topSlider).GetComponent<Slider>();
			_healthSlider.transform.parent.position = new Vector3(_healthSlider.transform.parent.position.x,
				_model.localScale.y * 4f + 2.1f, _healthSlider.transform.parent.position.z);
		}
		else
			_healthSlider = GameObject.Find("HealthSliderUI").GetComponent<Slider>();

		_healthSlider.maxValue = _health;
		_healthSlider.value = _health;
	}

	public Side GetSide()
	{
		return _side;
	}

	public Animator GetAnimator()
	{
		return _animator;
	}

	protected IEnumerator UpdateSlider(float value)
	{
		if (value <= 0)
		{
			_healthSlider.value = 0;
			yield break;
		}

		float stepSize = 2f;
		float dir = value > _healthSlider.value ? stepSize : -stepSize;
		float wait = 0.5f / (Mathf.Abs(_healthSlider.value - value) / stepSize);

		for (float v = _healthSlider.value; Mathf.Abs(value - v) > stepSize; v += dir)
		{
			_healthSlider.value = v;
			yield return new WaitForSeconds(wait);
		}

		_healthSlider.value = value;
	}

	#endregion

	#region ModelCreation

	[Rpc(SendTo.Server)]
	private void CreateModelRpc()
	{
		GameObject model = Instantiate(_modelPrefab, new Vector3(), Quaternion.identity, _player);
		model.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

		SetModelRpc();
	}

	[Rpc(SendTo.Everyone)]
	private void SetModelRpc()
	{
		foreach (GameObject model in GameObject.FindGameObjectsWithTag("Model"))
			model.name = $"Model{model.GetComponent<NetworkObject>().OwnerClientId}";

		_model = GameObject.Find($"Model{OwnerClientId}").transform;
		_animator = _model.GetComponent<Animator>();
		_playerScript.SetAnimatorRpc(_model.name);
		_playerScript.SetCameraFollow(new Vector3(0, 4.625f * _model.localScale.y - 2.375f,
			-2.5f * _model.localScale.y + 2.5f));
		SetSliders(_topSliderName);

		_playerScript.Spawned();
	}

	#endregion

	#region CardMethods

	protected virtual void Attack()
	{
		_animator.SetTrigger("Attack");
	}

	[Rpc(SendTo.Everyone)]
	protected void DamageTowerRpc(string towerName)
	{
		Tower t = GameObject.Find(towerName).GetComponent<Tower>();

		if (t.GetSide() != _side)
		{
			if (t.Damage(cardParams.damage))
				_elixirEarned += 10;
		}
	}

	[Rpc(SendTo.Everyone)]
	private void DamageRpc(float amount)
	{
		if (_health <= 0)
			return;

		_health -= amount;
		StartCoroutine(UpdateSlider(_health));
		if (_health <= 0)
			OnDeathRpc();
	}

	public void KilledPlayer(Player killedPlayer)
	{
		_elixirEarned += 3;
		Chat.Get.KillLog(_playerScript.GetPlayerName(), killedPlayer.GetPlayerName(), cardParams.cardName);
	}

	[Rpc(SendTo.Owner)]
	protected virtual void OnDeathRpc()
	{
		_started = false;
		_animator.SetTrigger("Death");

		_playerScript.EnableColliderRpc(false);
		_playerScript.EarnElixir((int)_elixirEarned);
		_playerScript.Respawn();
	}

	public bool Damage(float amount)
	{
		float preHealth = _health;
		DamageRpc(amount);
		return _health <= 0 && preHealth > 0;
	}

	public void Heal(float amount)
	{
		DamageRpc(-amount);
	}

	#endregion
}