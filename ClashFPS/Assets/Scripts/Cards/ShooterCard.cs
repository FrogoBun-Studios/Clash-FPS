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
		Gizmos.DrawRay(_player.position + _player.forward * 0.5f + _player.up * 2f, _playerScript.GetCameraForward());
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
				_player.position + _player.forward * 0.5f + _player.up * 2f, _playerScript.GetCameraRotation(), _player)
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
			GetParamsAsShooter().bulletPiercing, _playerScript.Side,
			_playerScript.GetCameraForward(), amount => _playerScript.Elixir += amount, KilledPlayer, _playerScript);
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