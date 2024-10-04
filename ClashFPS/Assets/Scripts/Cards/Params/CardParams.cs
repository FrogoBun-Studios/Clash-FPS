using UnityEngine;

[CreateAssetMenu(fileName = "New CardParams", menuName = "ClashFPS/CardParams")]
public class CardParams : ScriptableObject
{
    public float health;
    public float damage;
    public float speed;
    public float JumpStrength;
    public int jumps;
    public int elixer;
    public float AttackRate;
    public float ColliderRadius;
    public float ColliderHeight;
    public float ColliderYOffset;
    public GameObject ModelPrefab;
    public Texture CardImage;
}
