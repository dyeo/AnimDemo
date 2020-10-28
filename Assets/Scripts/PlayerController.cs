using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public Animator Animator;
	public CharacterController CharacterController;

	public float MaxLinearSpeed = 10; // in units/second
	public float LinearAcceleration = 10; // in units/second^2

	public float MaxAngularSpeed = 360; // in degrees/second
	public float AngularAcceleration = 180; // in degrees/second^2

	public float MaxJumpHeight = 1; // in units

	private Vector3 localVelocity;
	private float yRotation;

	private bool wasGrounded = true;
	private bool isGrounded = true;

	private bool isWalking => Input.GetAxisRaw("Walk") > 0f;

	private bool isSprinting => Input.GetAxisRaw("Sprint") > 0f;

	private void Update()
	{
		// Record last grounded state so we can tell the frame when we land
		wasGrounded = isGrounded;
		// Simple sphere cast at feet checking for ground to see if we've landed - could be more robust
		isGrounded = Physics.OverlapSphere(transform.position, .1f, LayerMask.GetMask("Ground")).Length > 0;
		
		// Fall according to gravity
		localVelocity.y += Physics.gravity.y * Time.deltaTime;

		if (isGrounded)
		{
			// Stop falling if we're grounded
			localVelocity.y = 0f;

			// Add force upwards if we jump
			if (Input.GetButtonDown("Jump"))
			{
				localVelocity.y = MaxJumpHeight * -Physics.gravity.y;
			}

			// Get forward/back direction adjusted to 2/3rds (normal) speed
			float linearSpeedTarget = Input.GetAxisRaw("Vertical") * MaxLinearSpeed * (2f / 3f);
			linearSpeedTarget *= isWalking ? 0.5f : 1f;
			linearSpeedTarget *= isSprinting ? (3f / 2f) : 1f;
			localVelocity.z = Mathf.Lerp(localVelocity.z, linearSpeedTarget, LinearAcceleration * Time.deltaTime);

			// Get left/right rotation (tank controls)
			float rotationTarget = Input.GetAxisRaw("Horizontal") * MaxAngularSpeed;
			yRotation = Mathf.Lerp(yRotation, rotationTarget, AngularAcceleration * Time.deltaTime);
			transform.Rotate(Vector3.up, yRotation * Time.deltaTime);
		}

		// Finally, move the character.
		CharacterController.Move(transform.TransformDirection(localVelocity) * Time.deltaTime);
	}

	private void LateUpdate()
	{
		// Inverse lerp the current speed value from local velocity between [-MaxLinearSpeed, MaxLinearSpeed]
		// This gives us smooth interpolation between nodes on the blend tree, instead of snapping to different run states
		Animator.SetFloat("Speed", Mathf.InverseLerp(-MaxLinearSpeed, MaxLinearSpeed, localVelocity.z));

		Animator.SetBool("Jumping", !isGrounded && localVelocity.y > 0f);
		Animator.SetBool("Falling", !isGrounded && localVelocity.y < 0f);

		if (!wasGrounded && isGrounded)
		{
			Animator.SetTrigger("Landed");
		}
	}

	private void OnDrawGizmos()
	{
		Color defaultColor = Gizmos.color;
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, 0.1f);
		Gizmos.color = defaultColor;
	}
}