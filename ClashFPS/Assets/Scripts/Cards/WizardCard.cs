using UnityEngine;

public class WizardCard : Card
{
    public override void StartCard(Transform player)
    {
        base.StartCard(player, new CardParams(
            health: CardParamHelper.Health.Medium,
            damage: CardParamHelper.Damage.MediumHigh,
            speed: CardParamHelper.Speed.MediumFast,
            JumpStrength: CardParamHelper.JumpStrength.Medium,
            jumps: 1,
            elixer: 5,
            flying: false,
            side: Side.Red,
            AttackRate: CardParamHelper.AttackRate.SlowMedium
        ), "Wizard");
    }
}
