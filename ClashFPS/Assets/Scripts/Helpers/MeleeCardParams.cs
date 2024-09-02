using UnityEngine;

public class MeleeCardParams : CardParams
{
    public Bounds AttackZone;

    public MeleeCardParams(float health, float damage, float speed, float JumpStrength, int jumps, int elixer, bool flying, float AttackRate, Side side, float ColliderRadius, float ColliderHeight, float ColliderYOffset, Bounds AttackZone) : base(health, damage, speed, JumpStrength, jumps, elixer, flying, AttackRate, side, ColliderRadius, ColliderHeight, ColliderYOffset)
    {
        this.AttackZone = AttackZone;
    }
}