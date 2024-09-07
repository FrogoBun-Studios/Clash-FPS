public class CardParams
{
    public float health;
    public float damage;
    public float speed;
    public float JumpStrength;
    public int jumps;
    public int elixer;
    public bool flying;
    public float AttackRate;
    public Side side;
    public float ColliderRadius;
    public float ColliderHeight;
    public float ColliderYOffset;

    public CardParams(float health, float damage, float speed, float JumpStrength, int jumps, bool flying, float AttackRate, Side side, float ColliderRadius, float ColliderHeight, float ColliderYOffset)
    {
        this.health = health;
        this.damage = damage;
        this.speed = speed;
        this.JumpStrength = JumpStrength;
        this.jumps = jumps;
        this.flying = flying;
        this.AttackRate = AttackRate;
        this.side = side;
        this.ColliderRadius = ColliderRadius;
        this.ColliderHeight = ColliderHeight;
        this.ColliderYOffset = ColliderYOffset;
    }
}
