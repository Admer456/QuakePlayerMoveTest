using UnityEngine;

// This is a simple Quake-style camera that works with Euler angles
[RequireComponent(typeof(Camera))]
class PlayerCamera : MonoBehaviour
{
	private void Start()
	{
		cameraComponent = GetComponent<Camera>();
		// TODO: let this be controllable as an in-game option
		cameraComponent.fieldOfView = 90.0f;
	}

	// Changes camera angles by adding to them
	public void AddAngles( float pitch, float yaw, float roll )
	{
		Vector3 angles = new Vector3( pitch, yaw, roll );
		Vector3 originalAngles = transform.eulerAngles;

		transform.eulerAngles = originalAngles + angles;
	}

	public void SetAngles( float pitch, float yaw, float roll )
	{
		transform.eulerAngles = new Vector3( pitch, yaw, roll );
	}

	private Camera cameraComponent;
}
