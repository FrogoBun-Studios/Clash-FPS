using System.Collections;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.UI;


public abstract class Card : NetworkBehaviour
{
	[SerializeField] protected CardParams cardParams;
	private Animator _animator;
	protected float _attackTimer;

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

		if (!_started)
			_health = cardParams.health;

		_topSliderName = topSlider;
		if (!IsOwner)
		{
			SetModelRpc();
			return;
		}

		_playerScript.SetColliderSizeRpc(cardParams.colliderRadius, cardParams.colliderHeight,
			cardParams.colliderYOffset);

		if (!_started)
			CreateModelRpc();

		if (!_started)
			_attackTimer = 1 / cardParams.attackRate;
	}

	public bool IsStarted()
	{
		return _started;
	}

	public virtual void UpdateCard()
	{
		if (_health <= 0)
			return;

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
			_healthSlider.transform.parent.position = new Vector3(_healthSlider.transform.position.x,
				_model.localScale.y * 4f + 2.1f, _healthSlider.transform.position.z);
		}
		else
			_healthSlider = GameObject.Find("HealthSliderUI").GetComponent<Slider>();

		_healthSlider.maxValue = _health;
		_healthSlider.value = _health;

		_started = true;
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

		float stepSize = 0.5f;
		float dir = value > _healthSlider.value ? stepSize : -stepSize;
		float wait = 0.01f / (Mathf.Abs(_healthSlider.value - value) / stepSize);

		for (float v = _healthSlider.value; Mathf.Abs(value - v) > stepSize; v += dir)
		{
			_healthSlider.value = v;
			yield return new WaitForSeconds(wait);
		}
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
			t.Damage(cardParams.damage);
	}

	[Rpc(SendTo.Everyone)]
	public void DamageRpc(float amount)
	{
		_health -= amount;

		StartCoroutine(UpdateSlider(_health));

		if (_health <= 0)
			OnDeathRpc();
	}

	[Rpc(SendTo.Owner)]
	protected virtual void OnDeathRpc()
	{
		_animator.SetTrigger("Death");
		_playerScript.Respawn();
	}

	public void Heal(float amount)
	{
		DamageRpc(-amount);
	}

	#endregion
}