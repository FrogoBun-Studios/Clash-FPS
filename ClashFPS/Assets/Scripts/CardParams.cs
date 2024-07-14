public struct CardParams
{
    public float health;
    public float damage;
    public float speed;
    public float JumpStrength;
    public int jumps;
    public int elixer;
    public bool flying;
    public float attackRate;

    public CardParams(float health, float damage, float speed, float JumpStrength, int jumps, int elixer, bool flying, float attackRate)
    {
        this.health = health;
        this.damage = damage;
        this.speed = speed;
        this.JumpStrength = JumpStrength;
        this.jumps = jumps;
        this.elixer = elixer;
        this.flying = flying;
        this.attackRate = attackRate;
    }
}
