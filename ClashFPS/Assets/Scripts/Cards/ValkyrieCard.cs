using UnityEngine;

public class ValkyrieCard : MeleeCard
{
    public override void StartCard(Transform player, Side side)
    {
        base.StartCard(player, new MeleeCardParams(
            health: CardParamHelper.Health.Heavy,
            damage: CardParamHelper.Damage.MediumHigh,
            speed: CardParamHelper.Speed.Medium,
            JumpStrength: CardParamHelper.JumpStrength.MediumHigh,
            jumps: 2,
            flying: false,
            AttackRate: CardParamHelper.AttackRate.Medium,
            side: side,
            ColliderRadius: CardParamHelper.Collider.Radius,
            ColliderHeight: CardParamHelper.Collider.Height,
            ColliderYOffset: CardParamHelper.Collider.YOffset,
            AttackZone: new Bounds(Vector3.up * 1.5f, new Vector3(5, 2, 5))
        ), "Valkyrie");
    }

    public override int GetElixerCost() => 4;
}
