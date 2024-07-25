using UnityEngine;

public abstract class MeleeCard : Card
{
    protected override void Attack()
    {
        base.Attack();

        Vector3 attackPos = player.position
            + player.forward * getParamsAsMelee().AttackZone.center.x
            + player.up * getParamsAsMelee().AttackZone.center.y
            + player.right * getParamsAsMelee().AttackZone.center.z;

        Collider[] colliders = Physics.OverlapBox(attackPos, getParamsAsMelee().AttackZone.size / 2);

        foreach(Collider col in colliders){
            if(col.CompareTag("Player")){
                if(col.GetComponent<Player>().GetCard().GetSide() != Params.side)
                    col.GetComponent<Player>().GetCard().DamageRpc(Params.damage);
            }

            if(col.CompareTag("Castle"))
                AttackCastleRpc(col.name);
        }
    }

    protected MeleeCardParams getParamsAsMelee() => (MeleeCardParams)Params;
}