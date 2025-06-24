using Unity.Netcode;

using UnityEngine;


public class MeleeCard : Card
{
	private void OnDrawGizmos()
	{
		// if (!IsOwner)
		// 	return;

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(player.position
		                    + player.right * GetParamsAsMelee().attackZone.center.x
		                    + player.up * GetParamsAsMelee().attackZone.center.y
		                    + player.forward * GetParamsAsMelee().attackZone.center.z,
			GetParamsAsMelee().attackZone.size);
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
		                    + player.right * GetParamsAsMelee().attackZone.center.x
		                    + player.up * GetParamsAsMelee().attackZone.center.y
		                    + player.forward * GetParamsAsMelee().attackZone.center.z;

		Collider[] colliders = Physics.OverlapBox(attackPos, GetParamsAsMelee().attackZone.size / 2);

		foreach (Collider col in colliders)
		{
			if (col.CompareTag("Player"))
			{
				if (col.GetComponent<Player>().GetSide() != playerScript.GetSide())
				{
					playerScript.UpdateElixirServerRpc(cardParams.damage * 0.005f);
					col.GetComponent<Player>().GetCard().DamageServerRpc(OwnerClientId, cardParams.damage);
				}
			}

			if (col.gameObject.CompareTag("Tower"))
				if (col.gameObject.GetComponent<Tower>().GetSide() != playerScript.GetSide())
					col.gameObject.GetComponent<Tower>().DamageServerRpc(OwnerClientId, cardParams.damage);

			// if (col.CompareTag("Tower"))
			// 	DamageTowerRpc(col.name);
		}
	}

	private MeleeCardParams GetParamsAsMelee()
	{
		return (MeleeCardParams)cardParams;
	}
}