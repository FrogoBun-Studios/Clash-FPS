using System;

using Unity.Netcode;

using UnityEngine;


public class Bullet : NetworkBehaviour
{
	[SerializeField] private Rigidbody rb;
	private float _damage;
	private Action<float> _earnElixir;
	private Action<Player> _killedPlayer;
	private int _piercing;
	private Player _player;
	private Side _side;
	private float _speed;

	private void OnTriggerEnter(Collider other)
	{
		if (!IsServer)
			return;

		if (other.gameObject == _player.gameObject)
			return;

		Collider[] cols = Physics.OverlapSphere(transform.position,
			Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
		foreach (Collider col in cols)
		{
			if (col.gameObject.CompareTag("Player"))
				if (col.gameObject.GetComponent<Player>().GetCard().GetSide() != _side)
				{
					_earnElixir(_damage * 0.005f);
					if (col.gameObject.GetComponent<Player>().GetCard().Damage(_damage))
						_killedPlayer(col.gameObject.GetComponent<Player>());
				}

			if (col.gameObject.CompareTag("Tower"))
				DamageTowerRpc(col.gameObject.name);
		}

		SelfDestroy();
	}

	public void Enable(float speed, float damage, int piercing, Side side, Vector3 dir, Action<float> earnElixir,
		Action<Player> killedPlayer, Player player)
	{
		_speed = speed;
		_damage = damage;
		_piercing = piercing;
		_side = side;
		_earnElixir = earnElixir;
		_killedPlayer = killedPlayer;
		_player = player;
		SetVelocityRpc(dir);
	}

	[Rpc(SendTo.Server)]
	private void SetVelocityRpc(Vector3 dir)
	{
		rb.linearVelocity = dir * _speed;
	}

	private void SelfDestroy()
	{
		_piercing--;

		if (_piercing <= 0)
			GetComponent<NetworkObject>().Despawn();
	}

	[Rpc(SendTo.Everyone)]
	protected void DamageTowerRpc(string towerName)
	{
		Tower t = GameObject.Find(towerName).GetComponent<Tower>();

		if (t.GetSide() != _side)
		{
			_earnElixir(_damage * 0.005f);
			if (t.Damage(_damage))
				_earnElixir(10);
		}
	}
}