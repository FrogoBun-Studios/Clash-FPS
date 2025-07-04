using UnityEngine;


[CreateAssetMenu(fileName = "New CardParams", menuName = "ClashFPS/CardParams")]
public class CardParams : ScriptableObject
{
	public string cardName;
	public float health;
	public float damage;
	public float speed;
	public float jumpStrength;
	public int jumps;
	public int elixir;
	public float attackRate;
	public float colliderRadius;
	public float colliderHeight;
	public float colliderYOffset;
	public Vector3 customCameraOffset;
	public GameObject modelPrefab;
	public Texture cardImage;
}