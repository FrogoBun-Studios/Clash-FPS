using UnityEngine;

public class ValkyrieCard : MeleeCard
{
    public override void StartCard(Transform player)
    {
        base.StartCard(player, new MeleeCardParams(
            health: CardParamHelper.Health.Heavy,
            damage: CardParamHelper.Damage.Medium,
            speed: CardParamHelper.Speed.Medium,
            JumpStrength: CardParamHelper.JumpStrength.MediumHigh,
            jumps: 2,
            elixer: 4,
            flying: false,
            AttackRate: CardParamHelper.AttackRate.Medium,
            side: Side.Blue,
            AttackZone: new Bounds(Vector3.up * 2, new Vector3(4, 2, 4))
        ), "Valkyrie");
    }
}
