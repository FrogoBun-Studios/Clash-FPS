using System;

using Unity.Netcode;

using UnityEngine;


public class Bullet : NetworkBehaviour
{
	[SerializeField] private Rigidbody rb;
	[SerializeField] private SphereCollider collider;
	private float damage;
	private Action<float> earnElixir;
	private int piercing;
	private Player player;
	private Side side;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, 1.5f * collider.radius * transform.lossyScale.x);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!IsServer)
			return;

		if (other.gameObject == player.gameObject)
			return;

		if (other.gameObject.CompareTag("Bullet"))
			return;

		Collider[] cols = Physics.OverlapSphere(transform.position, 1.5f * collider.radius * transform.lossyScale.x);
		foreach (Collider col in cols)
		{
			if (col.gameObject.CompareTag("Player"))
				if (col.gameObject.GetComponent<Player>().GetSide() != side)
				{
					earnElixir(damage * Constants.elixirPerDamage);
					col.gameObject.GetComponent<Player>().GetCard().DamageServerRpc(OwnerClientId, damage);
				}

			if (col.gameObject.CompareTag("Tower"))
				if (col.gameObject.GetComponent<Tower>().GetSide() != side)
					col.gameObject.GetComponent<Tower>().DamageServerRpc(OwnerClientId, damage);
		}

		piercing--;
		if (piercing <= 0)
			GetComponent<NetworkObject>().Despawn();
	}

	/// <summary>
	///     Enables this bullet to start moving on SERVER.
	/// </summary>
	public void Enable(float speed, float damage, int piercing, Side side, Vector3 dir, Action<float> earnElixir,
		Player player)
	{
		this.damage = damage;
		this.piercing = piercing;
		this.side = side;
		this.earnElixir = earnElixir;
		this.player = player;

		rb.linearVelocity = dir * speed;
	}
}