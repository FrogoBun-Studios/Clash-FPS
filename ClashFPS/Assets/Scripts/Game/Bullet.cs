using System;

using Unity.Netcode;

using UnityEngine;


public class Bullet : NetworkBehaviour
{
	[SerializeField] private Rigidbody rb;
	private float damage;
	private Action<float> earnElixir;
	private Action<Player> killedPlayer;
	private int piercing;
	private Player player;
	private Side side;
	private float speed;

	private void OnTriggerEnter(Collider other)
	{
		if (!IsServer)
			return;

		if (other.gameObject == player.gameObject)
			return;

		// Collider[] cols = Physics.OverlapSphere(transform.position,
		// 	Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
		// foreach (Collider col in cols)
		// {
		// 	if (col.gameObject.CompareTag("Player"))
		// 		if (col.gameObject.GetComponent<Player>().side != side)
		// 		{
		// 			earnElixir(damage * 0.005f);
		//
		// 			// if (col.gameObject.GetComponent<Player>().Card.DamageRpc(_damage))
		// 			// 	_killedPlayer(col.gameObject.GetComponent<Player>());
		// 			col.gameObject.GetComponent<Player>().card.DamageRpc(damage);
		// 		}
		//
		// 	if (col.gameObject.CompareTag("Tower"))
		// 		DamageTowerRpc(col.gameObject.name);
		// }
		if (other.gameObject.CompareTag("Player"))
		{
			if (other.gameObject.GetComponent<Player>().GetSide() != side)
			{
				earnElixir(damage * 0.005f);
				other.gameObject.GetComponent<Player>().GetCard().DamageServerRpc(OwnerClientId, damage);
			}
		}

		if (other.gameObject.CompareTag("Tower"))
		{
			if (other.gameObject.GetComponent<Tower>().GetSide() != side)
				other.gameObject.GetComponent<Tower>().DamageServerRpc(OwnerClientId, damage);
		}

		// SelfDestroy();
		piercing--;
		if (piercing <= 0)
			GetComponent<NetworkObject>().Despawn();
	}

	/// <summary>
	///     Enables this bullet to start moving on SERVER.
	/// </summary>
	public void Enable(float speed, float damage, int piercing, Side side, Vector3 dir, Action<float> earnElixir,
		Action<Player> killedPlayer, Player player)
	{
		this.speed = speed;
		this.damage = damage;
		this.piercing = piercing;
		this.side = side;
		this.earnElixir = earnElixir;
		this.killedPlayer = killedPlayer;
		this.player = player;

		// SetVelocityRpc(dir);
		rb.linearVelocity = dir * speed;
	}

	// [Rpc(SendTo.Server)]
	// private void SetVelocityRpc(Vector3 dir)
	// {
	// 	rb.linearVelocity = dir * speed;
	// }

	// private void SelfDestroy()
	// {
	// 	piercing--;
	//
	// 	if (piercing <= 0)
	// 		GetComponent<NetworkObject>().Despawn();
	// }

	// [Rpc(SendTo.Everyone)]
	// protected void DamageTowerRpc(string towerName)
	// {
	// 	Tower t = GameObject.Find(towerName).GetComponent<Tower>();
	//
	// 	if (t.GetSide() != side)
	// 	{
	// 		earnElixir(damage * 0.005f);
	// 		if (t.Damage(damage))
	// 			earnElixir(10);
	// 	}
	// }
}