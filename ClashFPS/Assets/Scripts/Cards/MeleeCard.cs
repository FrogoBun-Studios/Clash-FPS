using UnityEngine;


public class MeleeCard : Card
{
	private void OnDrawGizmos()
	{
		if (!IsOwner)
			return;

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(player.position
		                    + player.right * GetParamsAsMelee().attackZone.center.x
		                    + player.up * GetParamsAsMelee().attackZone.center.y
		                    + player.forward * GetParamsAsMelee().attackZone.center.z,
			GetParamsAsMelee().attackZone.size);
	}

	protected override void Attack()
	{
		Chat.Get.Log("1");

		base.Attack();

		Vector3 attackPos = player.position
		                    + player.right * GetParamsAsMelee().attackZone.center.x
		                    + player.up * GetParamsAsMelee().attackZone.center.y
		                    + player.forward * GetParamsAsMelee().attackZone.center.z;

		Collider[] colliders = Physics.OverlapBox(attackPos, GetParamsAsMelee().attackZone.size / 2);

		Chat.Get.Log("2");
		foreach (Collider col in colliders)
		{
			if (col.CompareTag("Player"))
			{
				if (col.GetComponent<Player>().side != playerScript.side)
				{
					Chat.Get.Log("3");
					playerScript.Elixir += cardParams.damage * 0.005f;

					// if (col.GetComponent<Player>().Card.DamageRpc(cardParams.damage))
					// 	KilledPlayer(col.GetComponent<Player>());
					Chat.Get.Log("4");
					col.GetComponent<Player>().card.DamageRpc(cardParams.damage);
					Chat.Get.Log("5");
				}
			}

			if (col.CompareTag("Tower"))
				DamageTowerRpc(col.name);
		}
	}

	protected MeleeCardParams GetParamsAsMelee()
	{
		return (MeleeCardParams)cardParams;
	}
}