using System.Collections;
using System.Linq;

using Unity.Netcode;

using UnityEngine;


public class ShooterCard : Card
{
	private void OnDrawGizmos()
	{
		if (!IsOwner)
			return;

		Gizmos.color = Color.red;
		Gizmos.DrawRay(player.position + player.forward * 0.5f + player.up * 2f, playerScript.GetCameraForward());
	}

	protected override void Attack()
	{
		base.Attack();

		StartCoroutine(SpawnBullet());
	}

	[Rpc(SendTo.Server)]
	protected void SpawnBulletRpc()
	{
		Bullet bullet = Instantiate(GetParamsAsShooter().bulletPrefab,
				player.position + player.forward * 0.5f + player.up * 2f, playerScript.GetCameraRotation(), player)
			.GetComponent<Bullet>();
		bullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);
		SetBulletRpc();

		StartCoroutine(DestroyBullet(bullet.GetComponent<NetworkObject>()));
	}

	[Rpc(SendTo.Everyone)]
	protected void SetBulletRpc()
	{
		Bullet bullet = GameObject.FindGameObjectsWithTag("Bullet").Last().GetComponent<Bullet>();

		bullet.Enable(GetParamsAsShooter().bulletSpeed, GetParamsAsShooter().damage,
			GetParamsAsShooter().bulletPiercing, playerScript.side,
			playerScript.GetCameraForward(), amount => playerScript.Elixir += amount, KilledPlayer, playerScript);
	}

	protected IEnumerator SpawnBullet()
	{
		yield return new WaitForSeconds(0.25f);

		SpawnBulletRpc();
	}

	protected IEnumerator DestroyBullet(NetworkObject bullet)
	{
		yield return new WaitForSeconds(3f);

		if (bullet.IsSpawned)
			bullet.Despawn();
	}

	protected ShooterCardParams GetParamsAsShooter()
	{
		return (ShooterCardParams)cardParams;
	}
}