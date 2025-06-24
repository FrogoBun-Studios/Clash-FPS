using UnityEngine;


public class LookAtCameraScript : MonoBehaviour
{
	private Transform cameraTransform;

	private void Start()
	{
		cameraTransform = GameObject.Find("CineCam").transform;
	}

	private void Update()
	{
		transform.LookAt(cameraTransform);
		transform.Rotate(Vector3.up * 180);
	}
}