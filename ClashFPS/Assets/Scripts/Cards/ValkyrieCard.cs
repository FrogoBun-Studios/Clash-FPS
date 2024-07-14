using Unity.Netcode;
using UnityEngine;

public class ValkyrieCard : Card
{
    public override void StartCard(Transform player, bool IsOwner, ulong OwnerClientId, CardParams Params = new CardParams(), string ModelName = "")
    {
        base.StartCard(player, IsOwner, OwnerClientId, new CardParams(
            health: 300f,
            damage: 30f,
            speed: 0.25f,
            JumpStrength: 15f,
            jumps: 1,
            elixer: 4,
            flying: false,
            attackRate: 1f
        ), "Valkyrie");
    }
}
