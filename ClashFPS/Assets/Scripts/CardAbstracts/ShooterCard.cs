using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public abstract class ShooterCard : Card
{

    protected override void Attack()
    {
        base.Attack();

        StartCoroutine(SpawnBullet());
    }

    [Rpc(SendTo.Server)]
    protected void SpawnBulletRpc(){
        Bullet bullet = Instantiate(getParamsAsShooter().BulletPrefab, player.position + player.forward * 0.5f + player.up * 2f, PlayerScript.GetCameraRotation(), player).GetComponent<Bullet>();
        bullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);
        SetBulletRpc();

        StartCoroutine(DestroyBullet(bullet.GetComponent<NetworkObject>()));
    }

    [Rpc(SendTo.Everyone)]
    protected void SetBulletRpc(){
        Bullet bullet = GameObject.FindGameObjectsWithTag("Bullet").Last().GetComponent<Bullet>();

        bullet.Enable(getParamsAsShooter().BulletSpeed, getParamsAsShooter().damage, getParamsAsShooter().side, PlayerScript.GetCameraForward());
    }

    protected IEnumerator SpawnBullet(){
        yield return new WaitForSeconds(0.25f);

        SpawnBulletRpc();
    }

    protected IEnumerator DestroyBullet(NetworkObject bullet){
        yield return new WaitForSeconds(3f);

        if(bullet.IsSpawned)
            bullet.Despawn(true);
    }

    protected ShooterCardParams getParamsAsShooter() => (ShooterCardParams)Params;

    private void OnDrawGizmos(){
        if(!IsOwner)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(player.position + player.forward * 0.5f + player.up * 2f, PlayerScript.GetCameraForward());
    }
}