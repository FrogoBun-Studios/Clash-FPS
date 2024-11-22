using UnityEngine;


public class LookAtCameraScript : MonoBehaviour
{
	private Transform _cameraTransform;

	private void Start()
	{
		_cameraTransform = GameObject.Find("CineCam").transform;
	}

	private void Update()
	{
		transform.LookAt(_cameraTransform);
		transform.Rotate(Vector3.up * 180);
	}
}