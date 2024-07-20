using UnityEngine;

public class WizardCard : Card
{
    public override void StartCard(Transform player)
    {
        base.StartCard(player, new CardParams(
            health: 100f,
            damage: 50f,
            speed: 1f,
            JumpStrength: 10f,
            jumps: 1,
            elixer: 5,
            flying: false,
            side: Side.Red,
            AttackRate: 1f
        ), "Wizard");
    }
}
