using Unity.Netcode;
using UnityEngine;

public class Castle : MonoBehaviour
{
    [SerializeField] private float Health = 1000f;
    [SerializeField] private GameObject DeathPrefab;
    [SerializeField] private bool IsKing = false;
    [SerializeField] private Side side;

    public void Damage(float amount)
    {
        Health -= amount;

        if(Health <= 0){
            Instantiate(DeathPrefab, transform.position + Vector3.down * (IsKing ? 9.3f : 4.8f), Quaternion.identity);
            Destroy(gameObject);
        }
    }

    public Side GetSide(){
        return side;
    }
}
