using UnityEngine;

public class WizardCard : ShooterCard
{
    public override void StartCard(Transform player)
    {
        base.StartCard(player, new ShooterCardParams(
            health: CardParamHelper.Health.Medium,
            damage: CardParamHelper.Damage.Medium,
            speed: CardParamHelper.Speed.MediumFast,
            JumpStrength: CardParamHelper.JumpStrength.Medium,
            jumps: 1,
            elixer: 5,
            flying: false,
            AttackRate: CardParamHelper.AttackRate.SlowMedium,
            side: Side.Red,
            BulletPrefab: Resources.Load("Wizard/Bullet") as GameObject,
            BulletSpead: 30,
            BulletAmount: 1,
            BulletSpread: 0
        ), "Wizard");
    }
}
