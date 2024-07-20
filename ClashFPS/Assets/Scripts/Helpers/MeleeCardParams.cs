using UnityEngine;

public class MeleeCardParams : CardParams
{
    public Bounds AttackZone;

    public MeleeCardParams(float health, float damage, float speed, float JumpStrength, int jumps, int elixer, bool flying, float AttackRate, Side side, Bounds AttackZone) : base(health, damage, speed, JumpStrength, jumps, elixer, flying, AttackRate, side)
    {
        this.AttackZone = AttackZone;
    }
}