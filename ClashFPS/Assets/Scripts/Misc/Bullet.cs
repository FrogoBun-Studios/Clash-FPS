using System.Collections;

using Unity.Netcode;

using UnityEngine;


public class Bullet : NetworkBehaviour
{
	[SerializeField] private Rigidbody rb;
	private float _damage;
	private Side _side;
	private float _speed;

	private void OnTriggerEnter(Collider other)
	{
		if (!IsServer)
			return;

		if (other.gameObject.CompareTag("Player"))
			if (other.gameObject.GetComponent<Player>().GetCard().GetSide() != _side)
			{
				other.gameObject.GetComponent<Player>().GetCard().DamageRpc(_damage);
				StartCoroutine(SelfDestroy());
			}

		if (other.gameObject.CompareTag("Tower"))
		{
			AttackTowerRpc(other.gameObject.name);
			StartCoroutine(SelfDestroy());
		}
	}

	public void Enable(float speed, float damage, Side side, Vector3 dir)
	{
		_speed = speed;
		_damage = damage;
		_side = side;
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
	protected void AttackTowerRpc(string towerName)
	{
		Tower t = GameObject.Find(towerName).GetComponent<Tower>();

		if (t.GetSide() != _side)
			t.Damage(_damage);
	}
}