using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [HideInInspector] public float speed;
    [HideInInspector] public float damage;
    [HideInInspector] public Side side;

    public void Enable(){
        rb.linearVelocity = transform.forward * speed;
    }

    [Rpc(SendTo.Server)]
    private void DestroyRpc(){
        GetComponent<NetworkObject>().Despawn(true);
    }

    [Rpc(SendTo.Everyone)]
    protected void AttackCastleRpc(string CastleName){
        Tower t = GameObject.Find(CastleName).GetComponent<Tower>();

        if(t.GetSide() != side)
            t.Damage(damage);
    }

    private void OnTriggerEnter(Collider other){
        if(!IsServer)
            return;

        if(other.gameObject.CompareTag("Player")){
            if(other.gameObject.GetComponent<Player>().GetCard().GetSide() != side){
                other.gameObject.GetComponent<Player>().GetCard().DamageRpc(damage);
                DestroyRpc();
            }

        }

        if(other.gameObject.CompareTag("Tower")){
            AttackCastleRpc(other.gameObject.name);
            DestroyRpc();
        }
    }
}
