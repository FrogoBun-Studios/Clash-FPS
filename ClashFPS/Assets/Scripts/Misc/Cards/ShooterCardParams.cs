using UnityEngine;

[CreateAssetMenu(fileName = "New ShooterCardParams", menuName = "ClashFPS/ShooterCardParams")]
public class ShooterCardParams : CardParams
{
    public GameObject BulletPrefab;
    public float BulletSpeed;
    public int BulletAmount;
    public float BulletSpread;
}