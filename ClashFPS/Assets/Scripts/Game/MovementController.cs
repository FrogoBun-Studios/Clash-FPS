using Unity.Cinemachine;
using Unity.Netcode;

using UnityEngine;


public class MovementController : NetworkBehaviour
{
	// [SerializeField] private CharacterController controller;
	private float sensitivity;
	private float FOV;

	private Animator animator;

	// private float yVelocity;
	private bool movementEnabled;
	private Transform model;
	private Vector3 resetedCameraPosition;
	private Quaternion resetedCameraRotation;
	private float acceleration = 55f;
	private float jumpStrength = 1300f;
	private bool readyToJump;
	private int jumps;
	private int jumpsLeft;

	[SerializeField] private Transform cameraFollow;
	[SerializeField] private Rigidbody rb;
	[SerializeField] private CapsuleCollider collider;

	[Tooltip("How much Movement Control in the Air: 0 = No Air Movement | 1 = Same as Ground")]
	[SerializeField]
	[Range(0.0f, 1.0f)]
	private float airMovement = 0.6f;

	[Tooltip("Player Drag when grounded")] [SerializeField] [Range(0.0f, 10.0f)]
	private float groundDrag = 4f;

	[Tooltip("Player Drag when not grounded")] [SerializeField] [Range(0.0f, 10.0f)]
	private float airDrag = 3f;

	[SerializeField] [Range(0.0f, 30.0f)] private float accelerationMultiplier = 15f;
	[SerializeField] [Range(0.0f, 10.0f)] private float jumpStrengthMultiplier = 1.5f;
	[SerializeField] [Range(0.0f, 10f)] private float stairJumpStrength = 0.1f;

	[Tooltip(
		"Ground Detection Type: Spherecast is more accurate but uses more performance, Raycast uses less performance but is less accurate")]
	[SerializeField]
	private GroundCheckType checkType;


	private enum GroundCheckType
	{
		Spherecast,
		Raycast
	}


	[SerializeField] private Transform groundCheck;
	[SerializeField] private Transform stairCheck;
	[SerializeField] private Transform stairCheck2;
	[SerializeField] private LayerMask groundLayer;

	private bool grounded;
	private Vector3 groundNormal;
	private RaycastHit[] groundHits;

	//Inputs
	private float vertical;
	private float horizontal;
	private bool jump;

	public void SetResetCameraPosition()
	{
		resetedCameraPosition = GameObject.Find("CineCam").transform.position;
		resetedCameraRotation = GameObject.Find("CineCam").transform.rotation;
	}

	public void SetupCardParams(float speed, int jumps, float jumpStrength)
	{
		this.jumpStrength = jumpStrength * jumpStrengthMultiplier;
		this.jumps = jumps;
		acceleration = speed * accelerationMultiplier;

		groundNormal = Vector3.zero;
		readyToJump = true;
		groundHits = new RaycastHit[10];
	}

	public void SettingsUpdated(float sensitivity, float FOV)
	{
		this.sensitivity = sensitivity;
		this.FOV = FOV;
	}

	public void SetModel(Transform model, Vector3 customCameraOffset)
	{
		this.model = model;

		if (IsOwner)
		{
			animator = model.GetComponent<Animator>();

			if (customCameraOffset == new Vector3())
				cameraFollow.localPosition =
					new Vector3(0, 4.625f * model.localScale.y - 2.375f, -2.5f * model.localScale.y + 2.5f);
			else
				cameraFollow.localPosition = customCameraOffset;
		}
	}

	public void SetEnabled(bool enable)
	{
		if (enable != movementEnabled)
		{
			movementEnabled = enable;
			Debug.Log($"Enabled movement controller: {enable}");
		}
	}

	[Rpc(SendTo.Everyone)]
	public void SetEnabledColliderAndMovementRpc(bool enable)
	{
		// controller.enabled = enable;
		rb.isKinematic = !enable;
		Debug.Log($"Enabled character controller and collider of player {OwnerClientId}: {enable}");
	}

	[Rpc(SendTo.Everyone)]
	public void SetColliderSizeRpc(float radius, float height, float yOffset)
	{
		collider.radius = radius;
		collider.height = height;
		collider.center = Vector3.up * yOffset;

		Debug.Log(
			$"Set collider size of player {OwnerClientId}: radius: {radius}, height: {height}, yOffset: {yOffset}");
	}

	public void ResetCamera()
	{
		GameObject.Find("CineCam").GetComponent<CinemachineCamera>().Follow = null;
		GameObject.Find("CineCam").transform.position = resetedCameraPosition;
		GameObject.Find("CineCam").transform.rotation = resetedCameraRotation;
	}

	public Transform GetCameraFollowTransform()
	{
		return cameraFollow;
	}

	public Transform GetCameraTransform()
	{
		return GameObject.Find("CineCam").transform;
	}

	private void Update()
	{
		if (!IsOwner)
			return;

		if (movementEnabled)
		{
			Look();

			//Input
			vertical = Input.GetAxisRaw("Vertical");
			horizontal = Input.GetAxisRaw("Horizontal");
			if (Input.GetKeyDown(KeyCode.Space))
				jump = true;

			GroundCheck();
		}

		CinemachineCamera cam = GameObject.Find("CineCam").GetComponent<CinemachineCamera>();
		cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, FOV + Mathf.Abs(rb.linearVelocity.magnitude), 0.1f);

		if (model != null)
		{
			model.position = transform.position;
			model.localEulerAngles = transform.localEulerAngles;
		}
	}

	private void FixedUpdate()
	{
		if (!IsOwner || !movementEnabled)
			return;

		//Physics
		rb.linearDamping = grounded ? groundDrag : airDrag;

		if (readyToJump && jump && jumpsLeft > 0)
			Jump();

		jump = false;

		animator.SetBool("Moving", !(vertical == 0 && horizontal == 0));
		if (vertical != 0)
			animator.SetFloat("Speed", Vector3.Dot(rb.linearVelocity, transform.forward) / 6.6f);
		else
			animator.SetFloat("Speed",
				Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.right)) >= 0.2f ? 1 : 0);

		if (vertical == 0 && horizontal == 0)
			return;

		float multi = 1f;

		if (!grounded)
			multi = airMovement;

		if (groundNormal != Vector3.zero)
		{
			rb.AddForce(
				Vector3.Cross(cameraFollow.right, groundNormal) *
				(vertical * acceleration * Time.fixedDeltaTime * multi), ForceMode.VelocityChange);
			rb.AddForce(
				Vector3.Cross(cameraFollow.forward, groundNormal) *
				(-horizontal * acceleration * Time.fixedDeltaTime * multi), ForceMode.VelocityChange);
		}
		else
		{
			rb.AddForce(transform.forward * (vertical * acceleration * Time.fixedDeltaTime * multi),
				ForceMode.VelocityChange);
			rb.AddForce(transform.right * (horizontal * acceleration * Time.fixedDeltaTime * multi),
				ForceMode.VelocityChange);
		}

		int s1 = Physics.OverlapBox(stairCheck.position, stairCheck.lossyScale / 2, transform.rotation, groundLayer)
			.Length;
		int s2 = Physics.OverlapBox(stairCheck2.position, stairCheck2.lossyScale / 2, transform.rotation, groundLayer)
			.Length;
		Debug.Log(s1 + ", " + s2);
		if (grounded
		    && s1 > 0
		    && s2 == 0)
			rb.AddForce(transform.up * stairJumpStrength, ForceMode.VelocityChange);
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

	private void GroundCheck()
	{
		int c = 0;
		float groundCheckRadius = groundCheck.lossyScale.x;

		if (checkType == GroundCheckType.Spherecast)
			c = Physics.SphereCastNonAlloc(groundCheck.position, groundCheckRadius, -transform.up, groundHits,
				groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
		else if (checkType == GroundCheckType.Raycast)
			c = Physics.RaycastNonAlloc(groundCheck.position, -transform.up, groundHits, groundCheckRadius,
				groundLayer, QueryTriggerInteraction.Ignore);

		if (c > 0 && readyToJump)
		{
			grounded = true;
			jumpsLeft = jumps;
			groundNormal = groundHits[0].normal;
		}
		else
		{
			grounded = false;
			groundNormal = Vector3.zero;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;

		if (checkType == GroundCheckType.Spherecast)
			Gizmos.DrawWireSphere(groundCheck.position, groundCheck.lossyScale.x);
		else if (checkType == GroundCheckType.Raycast)
			Gizmos.DrawRay(groundCheck.position, -transform.up * groundCheck.lossyScale.x);

		if (groundNormal != Vector3.zero)
			Gizmos.DrawRay(
				transform.position + Vector3.up * collider.height / 2 + Vector3.forward * collider.radius / 2,
				Vector3.Cross(cameraFollow.right, groundNormal) * 2);
		else
			Gizmos.DrawRay(
				transform.position + Vector3.up * collider.height / 2 + Vector3.forward * collider.radius / 2,
				transform.forward * 2);

		Gizmos.DrawWireCube(stairCheck.position, stairCheck.lossyScale);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(stairCheck2.position, stairCheck2.lossyScale);
	}

	private void Jump()
	{
		if (rb.linearVelocity.y < 0)
			rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

		if (groundNormal != Vector3.zero)
		{
			rb.AddForce(transform.up * jumpStrength / 2, ForceMode.VelocityChange);
			rb.AddForce(groundNormal * jumpStrength / 2, ForceMode.VelocityChange);
		}
		else
		{
			rb.AddForce(transform.up * jumpStrength, ForceMode.VelocityChange);
		}

		readyToJump = false;
		grounded = false;
		jumpsLeft--;
		groundNormal = Vector3.zero;
		Invoke(nameof(ResetJump), 0.15f);
	}

	private void ResetJump()
	{
		readyToJump = true;
	}

	[Rpc(SendTo.Owner)]
	public void SetAnimatorTriggerRpc(string triggerName)
	{
		animator.SetTrigger(triggerName);
		Debug.Log($"Set animation trigger: {triggerName}");
	}

	[Rpc(SendTo.Owner)]
	public void TeleportRpc(Vector3 position, Quaternion rotation)
	{
		// controller.enabled = false;
		transform.position = position;
		transform.rotation = rotation;

		// controller.enabled = true;

		Debug.Log($"Teleported to {transform.position}, meant to teleport to {position}");
	}

	[Rpc(SendTo.Owner)]
	public void TeleportRpc(Vector3 position)
	{
		// controller.enabled = false;
		transform.position = position;

		// controller.enabled = true;

		Debug.Log($"Teleported to {transform.position}, meant to teleport to {position}");
	}

	// public void ControlCharacter(float speed, int jumps, float jumpStrength)
	// {
	// 	if (controller.enabled)
	// 	{
	// 		Move(speed);
	// 		Look();
	//
	// 		if (movementEnabled && Input.GetButtonDown("Jump"))
	// 		{
	// 			if (controller.isGrounded)
	// 				jumpsLeft = jumps;
	//
	// 			if (jumpsLeft > 0)
	// 			{
	// 				yVelocity = jumpStrength;
	// 				SetAnimatorTriggerRpc("Jump");
	// 				jumpsLeft--;
	// 			}
	// 		}
	// 	}
	//
	// 	if (model != null)
	// 	{
	// 		model.position = transform.position;
	// 		model.localEulerAngles = transform.localEulerAngles;
	// 	}
	// 	else
	// 		Debug.LogError("My model is null");
	// }

	// private void Move(float speed)
	// {
	// 	Vector3 movementDir = new();
	// 	if (movementEnabled)
	// 		movementDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
	//
	// 	float xMove = movementDir.x * speed * Time.deltaTime;
	// 	yVelocity += Physics.gravity.y * Time.deltaTime;
	// 	float zMove = movementDir.z * speed * Time.deltaTime;
	//
	// 	controller.Move(transform.right * xMove
	// 	                + Vector3.up * (yVelocity * Time.deltaTime)
	// 	                + transform.forward * zMove);
	//
	// 	if (controller.isGrounded)
	// 		yVelocity = 0;
	//
	// 	animator.SetBool("Moving", movementDir != Vector3.zero);
	// 	if (movementDir.z != 0)
	// 		animator.SetFloat("Speed", Utils.MagnitudeInDirection(controller.velocity, transform.forward) / 6.6f);
	// 	else
	// 		animator.SetFloat("Speed",
	// 			Mathf.Abs(Utils.MagnitudeInDirection(controller.velocity, transform.right)) >= 0.2f ? 1 : 0);
	// }
}