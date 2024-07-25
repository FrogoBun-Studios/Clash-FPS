using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class ShooterCard : Card
{

    protected override void Attack()
    {
        base.Attack();

        SpawnBulletRpc();
    }

    [Rpc(SendTo.Server)]
    protected void SpawnBulletRpc(){
        Bullet bullet = Instantiate(getParamsAsShooter().BulletPrefab, player.position + player.forward * 0.5f + player.up * 2f, PlayerScript.GetCameraRotation(), player).GetComponent<Bullet>();
        bullet.GetComponent<NetworkObject>().Spawn(true);
        bullet.speed = getParamsAsShooter().BulletSpeed;
        bullet.damage = getParamsAsShooter().damage;
        bullet.side = getParamsAsShooter().side;
        bullet.Enable();

        StartCoroutine(DestroyBullet(bullet.GetComponent<NetworkObject>()));
    }

    protected IEnumerator DestroyBullet(NetworkObject bullet){
        yield return new WaitForSeconds(3f);

        if(bullet.IsSpawned)
            bullet.Despawn(true);
    }

    protected ShooterCardParams getParamsAsShooter() => (ShooterCardParams)Params;
}