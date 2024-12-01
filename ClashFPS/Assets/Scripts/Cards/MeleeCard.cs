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

		foreach (Collider col in colliders)
		{
			if (col.CompareTag("Player"))
			{
				if (col.GetComponent<Player>().GetCard().GetSide() != _side)
				{
					_elixirEarned += cardParams.damage * 0.00025f;
					if (col.GetComponent<Player>().GetCard().Damage(cardParams.damage))
						KilledPlayer(col.GetComponent<Player>());
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