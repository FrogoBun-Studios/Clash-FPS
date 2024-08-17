using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    [SerializeField] private float Health = 1000f;
    [SerializeField] private GameObject DeathPrefab;
    [SerializeField] private bool IsKing = false;
    [SerializeField] private Side side;
    [SerializeField] private Slider HealthSlider;

    private void Start(){
        HealthSlider.maxValue = Health;
        HealthSlider.value = Health;
    }

    public void Damage(float amount)
    {
        Chat.Singleton.Log(gameObject.name + ": " + amount);
        Health -= amount;

        StartCoroutine(UpdateSlider(Health));

        if(Health <= 0){
            Instantiate(DeathPrefab, transform.position + Vector3.down * (IsKing ? 8.3f : 5.8f), Quaternion.identity);
            Destroy(gameObject);
        }
    }

    protected IEnumerator UpdateSlider(float value){
        if(value <= 0){
            HealthSlider.value = 0;
            yield break;
        }

        float StepSize = 0.5f;
        float dir = value > HealthSlider.value ? StepSize : -StepSize;
        float wait = 0.01f / (Mathf.Abs(HealthSlider.value - value) / StepSize);

        for(float v = HealthSlider.value; Mathf.Abs(value - v) > StepSize; v += dir){
            HealthSlider.value = v;
            yield return new WaitForSeconds(wait);
        }
    }

    public Side GetSide(){
        return side;
    }
}
