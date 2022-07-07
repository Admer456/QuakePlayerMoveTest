using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// BasePlayer is our player controller
// It handles setting camera angles and calculating everything regarding movement
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BasePlayer : MonoBehaviour
{
	#region Editor stuff
	public float mouseSensitivity = 1.0f;

	// A developer shortcut to insert a player object instead of a player spawnpoint into the scene
	[MenuItem("GameObject/Entities/BasePlayer", false, 10)]
	static void CreateCustom(MenuCommand menuCommand)
	{
		// Create new game object and set up everything
		GameObject player = new GameObject("BasePlayer");
		player.AddComponent<BasePlayer>();

		GameObject camera = new GameObject("PlayerCamera");
		camera.AddComponent<PlayerCamera>();

		GameObjectUtility.SetParentAndAlign(player, menuCommand.context as GameObject);

		// Allow this action to be undone and select the resulting player
		Undo.RegisterCreatedObjectUndo(player, "Create " + player.name);
		Selection.activeObject = player;
	}
	#endregion

	void OnEnable()
	{
		Reset();
	}

	private void Start()
	{
		Reset();
	}

	// Resets all properties to their defaults
	private void Reset()
	{
		playerCollider = GetComponent<CapsuleCollider>();
		playerRigidBody = GetComponent<Rigidbody>();
		playerCamera = gameObject.GetComponentInChildren<PlayerCamera>();

		playerCollider.radius = 0.5f;
		playerCollider.height = 1.81f;

		playerRigidBody.mass = 50.0f;
		playerRigidBody.isKinematic = false;
		playerRigidBody.useGravity = false;
		playerRigidBody.constraints = RigidbodyConstraints.FreezeRotation;

		playerCamera.transform.localPosition = viewOffset;
	}

	private void UpdateCamera()
	{
		float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
		float verticalRotation = Input.GetAxis("Mouse Y") * mouseSensitivity;

		playerAngles.x -= verticalRotation;
		playerAngles.y += horizontalRotation;

		gameObject.transform.eulerAngles = new Vector3(0, playerAngles.y, 0);

		playerCamera.SetAngles(playerAngles.x, playerAngles.y, playerAngles.z);
	}

	// Calculates, in world space, the velocity which the player intends to have
	private Vector3 CalculateWishVelocity()
	{
		const float speed = 40.0f;
		Vector3 wishVel = new Vector3();

		// For the purposes of this experiment, I didn't use
		// any form of key bindings
		if (Input.GetKey(KeyCode.W))
		{
			wishVel += transform.forward;
		}
		if (Input.GetKey(KeyCode.S))
		{
			wishVel -= transform.forward;
		}
		if (Input.GetKey(KeyCode.A))
		{
			wishVel -= transform.right;
		}
		if (Input.GetKey(KeyCode.D))
		{
			wishVel += transform.right;
		}

		return wishVel.normalized * speed;
	}

	// Checks whether or not the player stands on the ground
	private bool IsOnGround(out Vector3 groundNormal, out float stick)
	{
		Vector3 down = -transform.up;
		Vector3 bottom = transform.position + down * (1.81f / 2.0f);
		RaycastHit rh;

		// This raycast here is a simple check
		// In case it fails, we gotta test a sphere against the ground
		bool lineHit = Physics.Raycast(bottom, down, out rh, 0.05f);

		groundNormal = rh.normal;
		stick = rh.distance;

		if (!lineHit)
		{
			// Perfrom sphere test
			// TODO: get rid of the magic numbers and parametrise these things
			bool hit = Physics.SphereCast(bottom - down * 0.55f, 0.48f, down, out rh, 0.1f);

			if (hit)
			{
				Vector3 normal = rh.normal;
				float dot = Vector3.Dot(normal, transform.up);

				groundNormal = normal;
				stick = rh.distance;

				if (Mathf.Abs(dot) < 0.5f)
				{   // Too steep
					return false;
				}
				else
				{   // Did hit and is on ground
					return true;
				}
			}

			// Didn't hit
			return false;
		}

		// Check if it's too steep
		float lineDot = Vector3.Dot(rh.normal, Vector3.up);
		if (Mathf.Abs(lineDot) < 0.5f)
			return false;

		// Hit and is on ground
		return true;
	}

	// Prevents the player from running too fast
	private Vector3 ClipVelocity(Vector3 velocity, float maxSpeed)
	{
		if (velocity.magnitude == 0.0f)
			return velocity;

		Vector3 clipped = velocity;
		float speed = velocity.magnitude;

		float d = speed / maxSpeed;
		if (d > 1.0f)
		{
			clipped /= d * d;
		}

		return clipped;
	}

	// Performs all calculations needed for movement and updates the rigid body
	private void UpdateMovement()
	{
		Vector3 wishVel = CalculateWishVelocity();
		Vector3 groundNormal;
		float multiplier = 1.0f; // general speed modifier
		float inverseMultiplier = 0.1f;
		float stick;
		bool onGround = IsOnGround(out groundNormal, out stick);
		float groundDot = Vector3.Dot(transform.up, groundNormal);

		// This is an approximation of air acceleration,
		// might want to move this to a method of its own
		if (!onGround)
		{
			multiplier *= 0.1f;
			inverseMultiplier = 1.0f;
			stick = 0;
			groundDot = 0.0f;
		}

		Vector3 velocity = Vector3.ProjectOnPlane(wishVel, groundNormal) + playerRigidBody.velocity;

		Vector3 friction = -playerRigidBody.velocity * 5.0f;
		float frictionMultiplier = Mathf.Min(wishVel.magnitude / 50.0f, 1.0f);

		if (!onGround)
		{
			frictionMultiplier = 1.0f;
		}

		// TODO: constants instead of magic numbers
		playerRigidBody.drag = 1.0f;
		playerRigidBody.AddForce(velocity * multiplier, ForceMode.Acceleration);
		playerRigidBody.AddForce(friction * (1.0f - frictionMultiplier), ForceMode.Acceleration);
		playerRigidBody.AddForce(Vector3.up * -9.81f * (1.0f - groundDot), ForceMode.Acceleration);
		playerRigidBody.AddForce(-groundNormal * (stick * 40.0f), ForceMode.Impulse);

		if (!onGround)
		{
			playerRigidBody.AddForce(playerRigidBody.velocity * 0.2f, ForceMode.Acceleration);
		}
		else
		{
			playerRigidBody.velocity = ClipVelocity(playerRigidBody.velocity, 5.0f);
		}

		if (Input.GetKey(KeyCode.Space) && onGround)
		{
			playerRigidBody.AddForce(transform.up * 305.0f, ForceMode.Impulse);
		}
	}

	void Update()
	{
		UpdateCamera();
	}

	private void FixedUpdate()
	{
		UpdateMovement();
	}

	private Vector3 GetForward()
	{
		return playerRigidBody.transform.forward;
	}

	private Vector3 playerAngles = new Vector3(0, 0, 0);
	private Vector3 viewOffset = new Vector3(0, 0.75f, 0);

	private CapsuleCollider playerCollider;
	private Rigidbody playerRigidBody;
	private PlayerCamera playerCamera;
}
