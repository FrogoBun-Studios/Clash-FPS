using UnityEngine;

public class ValkyrieCard : Card
{
    public override void StartCard(Transform player)
    {
        base.StartCard(player, new CardParams(
            health: 300f,
            damage: 30f,
            speed: 0.75f,
            JumpStrength: 15f,
            jumps: 1,
            elixer: 4,
            flying: false,
            attackRate: 1f
        ), "Valkyrie");
    }
}
