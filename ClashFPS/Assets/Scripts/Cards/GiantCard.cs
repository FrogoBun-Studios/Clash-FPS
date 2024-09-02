using UnityEngine;

public class GiantCard : MeleeCard
{
    public override void StartCard(Transform player, Side side)
    {
        base.StartCard(player, new MeleeCardParams(
            health: CardParamHelper.Health.Heavy,
            damage: CardParamHelper.Damage.High,
            speed: CardParamHelper.Speed.Slow,
            JumpStrength: CardParamHelper.JumpStrength.LowMedium,
            jumps: 1,
            elixer: 4,
            flying: false,
            AttackRate: CardParamHelper.AttackRate.Slow,
            side: side,
            ColliderRadius: 5,
            ColliderHeight: 12,
            ColliderYOffset: 6,
            AttackZone: new Bounds(new Vector3(0, 5, 3.5f), new Vector3(7, 8, 5))
        ), "Giant");
    }
}