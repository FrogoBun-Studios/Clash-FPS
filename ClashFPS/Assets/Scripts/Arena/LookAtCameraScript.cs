using UnityEngine;

public class LookAtCameraScript : MonoBehaviour
{
    private Transform CameraTransform;

    void Start()
    {
        CameraTransform = GameObject.Find("CineCam").transform;
    }

    void Update()
    {
        transform.LookAt(CameraTransform);
        transform.Rotate(Vector3.up * 180);
    }
}
