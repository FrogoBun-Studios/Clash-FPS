using UnityEngine;

public class MeleeCard : Card
{
    protected override void Attack()
    {
        base.Attack();

        Vector3 attackPos = player.position
            + player.right * getParamsAsMelee().AttackZone.center.x
            + player.up * getParamsAsMelee().AttackZone.center.y
            + player.forward * getParamsAsMelee().AttackZone.center.z;

        Collider[] colliders = Physics.OverlapBox(attackPos, getParamsAsMelee().AttackZone.size / 2);
        Chat.Singleton.Log($"Found {colliders.Length} objects to attack");

        foreach(Collider col in colliders){
            if(col.CompareTag("Player")){
                Chat.Singleton.Log($"Found player {col.name}");
                if(col.GetComponent<Player>().GetCard().GetSide() != side){
                    Chat.Singleton.Log($"Attacking {col.name}");
                    col.GetComponent<Player>().GetCard().DamageRpc(Params.damage);
                }
            }

            if(col.CompareTag("Tower"))
                AttackTowerRpc(col.name);
        }
    }

    protected MeleeCardParams getParamsAsMelee() => (MeleeCardParams)Params;

    private void OnDrawGizmos(){
        if(!IsOwner)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(player.position
            + player.right * getParamsAsMelee().AttackZone.center.x
            + player.up * getParamsAsMelee().AttackZone.center.y
            + player.forward * getParamsAsMelee().AttackZone.center.z, getParamsAsMelee().AttackZone.size);
    }
}