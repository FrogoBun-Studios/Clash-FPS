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
			movementController.GetCameraFollowTransform().forward);
	}

	protected override void Attack()
	{
		base.Attack();
		StartCoroutine(SpawnBullet());
	}

	[ServerRpc(RequireOwnership = false)]
	private void SpawnBulletServerRpc(Vector3 fwd, Vector3 up, Vector3 right)
	{
		for (int i = 0; i < GetParams().bulletAmount; i++)
		{
			float randomRight = Random.Range(-GetParams().bulletSpread, GetParams().bulletSpread);
			float randomUp = Random.Range(-GetParams().bulletSpread, GetParams().bulletSpread);
			Vector3 bulletPos = player.position + fwd * 0.5f + up * 2f;
			bulletPos = bulletPos + right * randomRight + up * randomUp;
			Vector3 bulletDir = (fwd + right * randomRight + up * randomUp).normalized;

			Bullet bullet = Instantiate(
				GetParams().bulletPrefab,
				bulletPos,
				Quaternion.Euler(fwd), player
			).GetComponent<Bullet>();
			bullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

			bullet.Enable(
				GetParams().bulletSpeed,
				GetParams().damage,
				GetParams().bulletPiercing,
				playerScript.GetPlayerData().side,
				bulletDir,
				amount => playerScript.UpdateElixirServerRpc(amount), playerScript
			);

			StartCoroutine(DestroyBullet(bullet.GetComponent<NetworkObject>()));
		}
	}

	private IEnumerator SpawnBullet()
	{
		yield return new WaitForSeconds(0.25f);

		SpawnBulletServerRpc(movementController.GetCameraFollowTransform().forward,
			movementController.GetCameraFollowTransform().up,
			movementController.GetCameraFollowTransform().right);
	}

	private IEnumerator DestroyBullet(NetworkObject bullet)
	{
		yield return new WaitForSeconds(3f);

		if (bullet != null)
			bullet.Despawn();
	}

	protected ShooterCardParams GetParams()
	{
		return (ShooterCardParams)cardParams;
	}
}