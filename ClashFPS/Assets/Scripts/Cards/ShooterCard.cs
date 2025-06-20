using System.Collections;

using Unity.Netcode;

using UnityEngine;


public class ShooterCard : Card
{
	private void OnDrawGizmos()
	{
		if (!IsOwner)
			return;

		Gizmos.color = Color.red;
		Gizmos.DrawRay(player.position + player.forward * 0.5f + player.up * 2f,
			movementController.GetCameraTransform().forward);
	}

	protected override void Attack()
	{
		base.Attack();
		StartCoroutine(SpawnBullet());
	}

	[ServerRpc]
	private void SpawnBulletServerRpc()
	{
		Bullet bullet = Instantiate(GetParamsAsShooter().bulletPrefab,
				player.position + player.forward * 0.5f + player.up * 2f,
				movementController.GetCameraTransform().rotation, player)
			.GetComponent<Bullet>();
		bullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

		// SetBulletRpc();
		bullet.Enable(GetParamsAsShooter().bulletSpeed, GetParamsAsShooter().damage,
			GetParamsAsShooter().bulletPiercing, playerScript.GetSide(),
			movementController.GetCameraTransform().forward,
			amount => playerScript.UpdateElixirServerRpc(amount),
			KilledPlayer, playerScript
		);

		StartCoroutine(DestroyBullet(bullet.GetComponent<NetworkObject>()));
	}

	// [Rpc(SendTo.Everyone)]
	// private void SetBulletRpc()
	// {
	// 	Bullet bullet = GameObject.FindGameObjectsWithTag("Bullet").Last().GetComponent<Bullet>();
	//
	// 	bullet.Enable(GetParamsAsShooter().bulletSpeed, GetParamsAsShooter().damage,
	// 		GetParamsAsShooter().bulletPiercing, playerScript.side,
	// 		movementController.GetCameraTransform().forward,
	// 		amount => playerScript.UpdateElixirServerRpc(amount),
	// 		KilledPlayer, playerScript
	// 	);
	// }

	private IEnumerator SpawnBullet()
	{
		yield return new WaitForSeconds(0.25f);

		SpawnBulletServerRpc();
	}

	private IEnumerator DestroyBullet(NetworkObject bullet)
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