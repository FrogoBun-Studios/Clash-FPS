using UnityEngine;

public class ShooterCardParams : CardParams
{
    public GameObject BulletPrefab;
    public float BulletSpeed;
    public int BulletAmount;
    public float BulletSpread;

    public ShooterCardParams(float health, float damage, float speed, float JumpStrength, int jumps, int elixer, bool flying, float AttackRate, Side side, float ColliderRadius, float ColliderHeight, float ColliderYOffset, GameObject BulletPrefab, float BulletSpeed, int BulletAmount, float BulletSpread) : base(health, damage, speed, JumpStrength, jumps, elixer, flying, AttackRate, side, ColliderRadius, ColliderHeight, ColliderYOffset)
    {
        this.BulletPrefab = BulletPrefab;
        this.BulletSpeed = BulletSpeed;
        this.BulletAmount = BulletAmount;
        this.BulletSpread = BulletSpread;
    }
}