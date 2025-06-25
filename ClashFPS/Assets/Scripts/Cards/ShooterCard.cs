using System.Collections;

using Unity.Netcode;

using UnityEngine;


public class ShooterCard : Card
{
	private Bullet bullet;

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

	[ServerRpc(RequireOwnership = false)]
	private void SpawnBulletServerRpc()
	{
		bullet = Instantiate(GetParamsAsShooter().bulletPrefab,
				player.position + player.forward * 0.5f + player.up * 2f,
				movementController.GetCameraTransform().rotation, player)
			.GetComponent<Bullet>();
		bullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

		EnableBulletRpc();

		StartCoroutine(DestroyBullet(bullet.GetComponent<NetworkObject>()));
	}

	[Rpc(SendTo.Owner)]
	private void EnableBulletRpc()
	{
		EnableBulletServerRpc(movementController.GetCameraTransform().forward);
	}

	[ServerRpc(RequireOwnership = false)]
	private void EnableBulletServerRpc(Vector3 cameraForward)
	{
		bullet.Enable(GetParamsAsShooter().bulletSpeed, GetParamsAsShooter().damage,
			GetParamsAsShooter().bulletPiercing, playerScript.GetSide(),
			cameraForward,
			amount => playerScript.UpdateElixirServerRpc(amount),
			OnKilledPlayerServerRpc, playerScript
		);
	}

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