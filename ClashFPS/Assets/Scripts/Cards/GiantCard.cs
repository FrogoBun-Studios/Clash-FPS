using UnityEngine;

public class GiantCard : Card
{
    public override void StartCard(Transform player)
    {
        base.StartCard(player, new CardParams(
            health: CardParamHelper.Health.Heavy,
            damage: CardParamHelper.Damage.High,
            speed: CardParamHelper.Speed.Slow,
            JumpStrength: CardParamHelper.JumpStrength.Medium,
            jumps: 1,
            elixer: 4,
            flying: false,
            AttackRate: CardParamHelper.AttackRate.Medium,
            side: Side.Blue
        ), "Giant");
    }
}