using Unity.Cinemachine;
using Unity.Netcode;

using UnityEngine;


public class MovementController : NetworkBehaviour
{
	[SerializeField] private CharacterController controller;
	[SerializeField] private float sensitivity;
	[SerializeField] private Transform cameraFollow;
	private Animator animator;
	private int jumpsLeft;
	private string playerName;
	private float yVelocity;
	private bool movementEnabled;
	private Transform model;
	private Vector3 resetedCameraPosition;
	private Quaternion resetedCameraRotation;

	public void SetResetCameraPosition()
	{
		resetedCameraPosition = GameObject.Find("CineCam").transform.position;
		resetedCameraRotation = GameObject.Find("CineCam").transform.rotation;
	}

	public void ControlCharacter(float speed, int jumps, float jumpStrength)
	{
		Move(speed);
		Look();

		if (movementEnabled && Input.GetButtonDown("Jump"))
		{
			if (controller.isGrounded)
				jumpsLeft = jumps;

			if (jumpsLeft > 0)
			{
				yVelocity = jumpStrength;
				SetAnimatorTriggerRpc("Jump");
				jumpsLeft--;
			}
		}

		if (model != null)
		{
			model.position = transform.position;
			model.localEulerAngles = transform.localEulerAngles;
		}
		else
		{
			Debug.LogError("My model is null");
		}
	}

	private void Move(float speed)
	{
		Vector3 movementDir = new();
		if (movementEnabled)
			movementDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));


		float xMove = movementDir.x * speed * Time.deltaTime;
		yVelocity += Physics.gravity.y * Time.deltaTime;
		float zMove = movementDir.z * speed * Time.deltaTime;

		controller.Move(transform.right * xMove
		                + Vector3.up * (yVelocity * Time.deltaTime)
		                + transform.forward * zMove);

		if (controller.isGrounded)
			yVelocity = 0;

		animator.SetBool("Moving", movementDir != Vector3.zero);
		if (movementDir.z != 0)
			animator.SetFloat("Speed", Utils.MagnitudeInDirection(controller.velocity, transform.forward) / 6.6f);
		else
			animator.SetFloat("Speed",
				Mathf.Abs(Utils.MagnitudeInDirection(controller.velocity, transform.right)) >= 0.2f ? 1 : 0);
	}

	private void Look()
	{
		if (!movementEnabled)
			return;

		transform.localEulerAngles =
			new Vector3(0, transform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivity, 0);

		float xAngle = cameraFollow.rotation.eulerAngles.x;
		if (xAngle >= 180)
			xAngle -= 360;

		cameraFollow.localEulerAngles =
			new Vector3(Mathf.Clamp(xAngle - Input.GetAxis("Mouse Y") * sensitivity, -40, 75), 0, 0);
	}

	[Rpc(SendTo.Owner)]
	public void TeleportRpc(Vector3 position, Quaternion rotation)
	{
		controller.enabled = false;
		transform.position = position;
		transform.rotation = rotation;
		controller.enabled = true;

		Debug.Log($"Teleported to {transform.position}, meant to teleport to {position}");
	}

	[Rpc(SendTo.Owner)]
	public void TeleportRpc(Vector3 position)
	{
		controller.enabled = false;
		transform.position = position;
		controller.enabled = true;

		Debug.Log($"Teleported to {transform.position}, meant to teleport to {position}");
	}

	[Rpc(SendTo.Owner)]
	public void SetAnimatorTriggerRpc(string triggerName)
	{
		animator.SetTrigger(triggerName);
		Debug.Log($"Set animation trigger: {triggerName}");
	}

	public void Enable(bool enable)
	{
		if (enable != movementEnabled)
		{
			movementEnabled = enable;
			Debug.Log($"Enabled movement controller: {enable}");
		}
	}

	public void ResetCamera()
	{
		GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = null;
		GameObject.Find("CineCam").transform.position = resetedCameraPosition;
		GameObject.Find("CineCam").transform.rotation = resetedCameraRotation;
	}

	[Rpc(SendTo.Everyone)]
	public void EnableColliderRpc(bool enable)
	{
		controller.enabled = enable;
		Debug.Log($"Enabled collider of player {OwnerClientId}: {enable}");
	}

	public void SetModel()
	{
		foreach (GameObject m in GameObject.FindGameObjectsWithTag("Model"))
		{
			if (m.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId)
			{
				model = m.transform;
				animator = model.gameObject.GetComponent<Animator>();
				break;
			}
		}

		Debug.Log("Found my model");

		cameraFollow.localPosition =
			new Vector3(0, 4.625f * model.localScale.y - 2.375f, -2.5f * model.localScale.y + 2.5f);
	}

	public Transform GetCameraTransform()
	{
		return cameraFollow;
	}

	[Rpc(SendTo.Everyone)]
	public void SetColliderSizeRpc(float radius, float height, float yOffset)
	{
		controller.radius = radius;
		controller.height = height;
		controller.center = Vector3.up * yOffset;

		Debug.Log(
			$"Set collider size of player {OwnerClientId}: radius: {radius}, height: {height}, yOffset: {yOffset}");
	}

	public void UpdateSensitivity(float sensitivity)
	{
		this.sensitivity = sensitivity;
	}
}