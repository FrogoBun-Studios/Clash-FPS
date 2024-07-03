using UnityEngine;

public class WizardCard : Card
{
    public override void StartCard(Transform transform)
    {
        base.StartCard(transform);
        setCardParams(
            health: 100f,
            damage: 50f,
            speed: 5f,
            JumpStrength: 10f,
            jumps: 1,
            elixer: 5,
            flying: false,
            attackRate: 1f
        );
    }
}
