using Unity.Netcode;

using UnityEngine;


public class MeleeCard : Card
{
	protected void OnDrawGizmos()
	{
		// if (!IsOwner)
		// 	return;

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(player.position
		                    + player.right * GetParams().attackZone.center.x
		                    + player.up * GetParams().attackZone.center.y
		                    + player.forward * GetParams().attackZone.center.z,
			GetParams().attackZone.size);
	}

	protected override void Attack()
	{
		base.Attack();
		AttackServerRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	private void AttackServerRpc()
	{
		Vector3 attackPos = player.position
		                    + player.right * GetParams().attackZone.center.x
		                    + player.up * GetParams().attackZone.center.y
		                    + player.forward * GetParams().attackZone.center.z;

		Collider[] colliders = Physics.OverlapBox(attackPos, GetParams().attackZone.size / 2);

		foreach (Collider col in colliders)
		{
			if (col.CompareTag("Player"))
			{
				if (col.GetComponent<Player>().GetPlayerData().side != playerScript.GetPlayerData().side)
				{
					playerScript.UpdateElixirServerRpc(cardParams.damage * Constants.elixirPerDamage);
					col.GetComponent<Player>().GetCard().DamageServerRpc(OwnerClientId, cardParams.damage);
				}
			}

			if (col.gameObject.CompareTag("Tower"))
				if (col.gameObject.GetComponent<Tower>().GetSide() != playerScript.GetPlayerData().side)
					col.gameObject.GetComponent<Tower>().DamageServerRpc(OwnerClientId, cardParams.damage);

			// if (col.CompareTag("Tower"))
			// 	DamageTowerRpc(col.name);
		}
	}

	protected MeleeCardParams GetParams()
	{
		return (MeleeCardParams)cardParams;
	}
}