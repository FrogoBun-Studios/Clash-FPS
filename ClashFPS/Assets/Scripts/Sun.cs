using UnityEngine;

public class Sun : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, Time.deltaTime * 0.2f, 0);
    }
}
