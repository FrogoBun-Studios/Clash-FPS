using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    private float speed;
    private float damage;
    private Side side;

    public void Enable(float speed, float damage, Side side, Vector3 dir){
        this.speed = speed;
        this.damage = damage;
        this.side = side;
        SetVelocityRpc(dir);
    }

    [Rpc(SendTo.Server)]
    private void SetVelocityRpc(Vector3 dir){
        Chat.Singleton.Log(dir.ToString());
        rb.linearVelocity = dir * speed;
    }

    private IEnumerator SelfDestroy(){
        yield return new WaitForSeconds(0.5f);

        GetComponent<NetworkObject>().Despawn(true);
    }

    [Rpc(SendTo.Everyone)]
    protected void AttackTowerRpc(string TowerName){
        Chat.Singleton.KillLog($"{OwnerClientId}", TowerName, "Wizard");
        Tower t = GameObject.Find(TowerName).GetComponent<Tower>();

        if(t.GetSide() != side)
            t.Damage(damage);
    }

    private void OnTriggerEnter(Collider other){
        if(!IsServer)
            return;

        if(other.gameObject.CompareTag("Player")){
            if(other.gameObject.GetComponent<Player>().GetCard().GetSide() != side){
                other.gameObject.GetComponent<Player>().GetCard().DamageRpc(damage);
                SelfDestroy();
            }

        }

        if(other.gameObject.CompareTag("Tower")){
            AttackTowerRpc(other.gameObject.name);
            SelfDestroy();
        }
    }
}
