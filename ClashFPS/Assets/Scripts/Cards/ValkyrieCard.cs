using UnityEngine;

public class ValkyrieCard : MeleeCard
{
    public override void StartCard(Transform player)
    {
        base.StartCard(player, new MeleeCardParams(
            health: CardParamHelper.Health.Heavy,
            damage: CardParamHelper.Damage.MediumHigh,
            speed: CardParamHelper.Speed.Medium,
            JumpStrength: CardParamHelper.JumpStrength.MediumHigh,
            jumps: 2,
            elixer: 4,
            flying: false,
            AttackRate: CardParamHelper.AttackRate.Medium,
            side: Side.Blue,
            AttackZone: new Bounds(Vector3.up * 1.5f, new Vector3(5, 2, 5))
        ), "Valkyrie");
    }
}
