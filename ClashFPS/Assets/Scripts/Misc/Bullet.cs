using System;
using System.Collections;

using Unity.Netcode;

using UnityEngine;


public class Bullet : NetworkBehaviour
{
	[SerializeField] private Rigidbody rb;
	private float _damage;
	private Action<float> _earnElixir;
	private Action<Player> _killedPlayer;
	private Side _side;
	private float _speed;

	private void OnTriggerEnter(Collider other)
	{
		if (!IsServer)
			return;

		if (other.gameObject.CompareTag("Player"))
			if (other.gameObject.GetComponent<Player>().GetCard().GetSide() != _side)
			{
				_earnElixir(_damage * 0.00025f);
				if (other.gameObject.GetComponent<Player>().GetCard().Damage(_damage))
					_killedPlayer(other.gameObject.GetComponent<Player>());

				StartCoroutine(SelfDestroy());
			}

		if (other.gameObject.CompareTag("Tower"))
		{
			DamageTowerRpc(other.gameObject.name);
			StartCoroutine(SelfDestroy());
		}
	}

	public void Enable(float speed, float damage, Side side, Vector3 dir, Action<float> earnElixir,
		Action<Player> killedPlayer)
	{
		_speed = speed;
		_damage = damage;
		_side = side;
		_earnElixir = earnElixir;
		_killedPlayer = killedPlayer;
		SetVelocityRpc(dir);
	}

	[Rpc(SendTo.Server)]
	private void SetVelocityRpc(Vector3 dir)
	{
		rb.linearVelocity = dir * _speed;
	}

	private IEnumerator SelfDestroy()
	{
		yield return new WaitForSeconds(0.5f);

		GetComponent<NetworkObject>().Despawn();
	}

	[Rpc(SendTo.Everyone)]
	protected void DamageTowerRpc(string towerName)
	{
		Tower t = GameObject.Find(towerName).GetComponent<Tower>();

		if (t.GetSide() != _side)
		{
			_earnElixir(_damage * 0.00025f);
			if (t.Damage(_damage))
				_earnElixir(10);
		}
	}
}