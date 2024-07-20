using UnityEngine;

public class Sun : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 0.5f;

    void Update()
    {
        transform.Rotate(0, Time.deltaTime * rotationSpeed, 0);
    }
}
