using UnityEngine;


[CreateAssetMenu(fileName = "New ShooterCardParams", menuName = "ClashFPS/ShooterCardParams")]
public class ShooterCardParams : CardParams
{
	public GameObject bulletPrefab;
	public float bulletSpeed;
	public int bulletAmount;
	public float bulletSpread;
}