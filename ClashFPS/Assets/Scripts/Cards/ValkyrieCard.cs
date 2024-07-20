using UnityEngine;

public class ValkyrieCard : MeleeCard
{
    public override void StartCard(Transform player)
    {
        base.StartCard(player, new MeleeCardParams(
            health: 300f,
            damage: 30f,
            speed: 0.75f,
            JumpStrength: 15f,
            jumps: 1,
            elixer: 4,
            flying: false,
            AttackRate: 1f,
            side: Side.Blue,
            AttackZone: new Bounds(Vector3.up * 2, new Vector3(4, 2, 4))
        ), "Valkyrie");
    }
}
