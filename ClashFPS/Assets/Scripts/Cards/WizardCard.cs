using UnityEngine;

public class WizardCard : ShooterCard
{
    public override void StartCard(Transform player, Side side)
    {
        base.StartCard(player, new ShooterCardParams(
            health: CardParamHelper.Health.Medium,
            damage: CardParamHelper.Damage.Medium,
            speed: CardParamHelper.Speed.MediumFast,
            JumpStrength: CardParamHelper.JumpStrength.Medium,
            jumps: 1,
            flying: false,
            AttackRate: CardParamHelper.AttackRate.SlowMedium,
            side: side,
            ColliderRadius: CardParamHelper.Collider.Radius,
            ColliderHeight: CardParamHelper.Collider.Height,
            ColliderYOffset: CardParamHelper.Collider.YOffset,
            BulletPrefab: Resources.Load("Wizard/Bullet") as GameObject,
            BulletSpeed: 30,
            BulletAmount: 1,
            BulletSpread: 0
        ), "Wizard");
    }

    public override int GetElixerCost() => 5;
}
