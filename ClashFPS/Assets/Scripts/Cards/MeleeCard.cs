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
		Chat.Get.Log("1");

		base.Attack();

		Vector3 attackPos = _player.position
		                    + _player.right * GetParamsAsMelee().attackZone.center.x
		                    + _player.up * GetParamsAsMelee().attackZone.center.y
		                    + _player.forward * GetParamsAsMelee().attackZone.center.z;

		Collider[] colliders = Physics.OverlapBox(attackPos, GetParamsAsMelee().attackZone.size / 2);

		Chat.Get.Log("2");
		foreach (Collider col in colliders)
		{
			if (col.CompareTag("Player"))
			{
				if (col.GetComponent<Player>().Side != _playerScript.Side)
				{
					Chat.Get.Log("3");
					_playerScript.Elixir += cardParams.damage * 0.005f;

					// if (col.GetComponent<Player>().Card.DamageRpc(cardParams.damage))
					// 	KilledPlayer(col.GetComponent<Player>());
					Chat.Get.Log("4");
					col.GetComponent<Player>().Card.DamageRpc(cardParams.damage);
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