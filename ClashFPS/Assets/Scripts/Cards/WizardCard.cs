using Unity.Netcode;
using UnityEngine;

public class WizardCard : Card
{
    public override void StartCard(Transform player, bool IsOwner, ulong OwnerClientId, CardParams Params = new CardParams(), string ModelName = "")
    {
        base.StartCard(player, IsOwner, OwnerClientId, new CardParams(
            health: 100f,
            damage: 50f,
            speed: 2f,
            JumpStrength: 10f,
            jumps: 1,
            elixer: 5,
            flying: false,
            attackRate: 1f
        ), "Wizard");
    }
}
