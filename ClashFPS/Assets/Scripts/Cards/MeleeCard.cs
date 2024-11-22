using UnityEngine;


public class MeleeCard : Card
{
	private void OnDrawGizmos()
	{
		if (!IsOwner)
			return;

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(_player.position
		                    + _player.right * GetParamsAsMelee().attackZone.center.x
		                    + _player.up * GetParamsAsMelee().attackZone.center.y
		                    + _player.forward * GetParamsAsMelee().attackZone.center.z,
			GetParamsAsMelee().attackZone.size);
	}

	protected override void Attack()
	{
		base.Attack();

		Vector3 attackPos = _player.position
		                    + _player.right * GetParamsAsMelee().attackZone.center.x
		                    + _player.up * GetParamsAsMelee().attackZone.center.y
		                    + _player.forward * GetParamsAsMelee().attackZone.center.z;

		Collider[] colliders = Physics.OverlapBox(attackPos, GetParamsAsMelee().attackZone.size / 2);
		Chat.Singleton.Log($"Found {colliders.Length} objects to attack");

		foreach (Collider col in colliders)
		{
			if (col.CompareTag("Player"))
			{
				Chat.Singleton.Log($"Found player {col.name}");
				if (col.GetComponent<Player>().GetCard().GetSide() != _side)
				{
					Chat.Singleton.Log($"Attacking {col.name}");
					col.GetComponent<Player>().GetCard().DamageRpc(cardParams.damage);
				}
			}

			if (col.CompareTag("Tower"))
				AttackTowerRpc(col.name);
		}
	}

	protected MeleeCardParams GetParamsAsMelee()
	{
		return (MeleeCardParams)cardParams;
	}
}