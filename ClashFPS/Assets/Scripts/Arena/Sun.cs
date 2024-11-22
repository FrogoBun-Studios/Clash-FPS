using UnityEngine;


public class Sun : MonoBehaviour
{
	[SerializeField] private float rotationSpeed = 0.5f;

	private void Update()
	{
		transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * rotationSpeed);
	}
}