using UnityEngine;
using UnityEditor;

namespace Game
{
	// This implementation of a "game" concept is unusual, as it requires
	// the level designer to place a Game component into their level
	// This is to be automated
	public class Game : MonoBehaviour
	{
		private void OnEnable()
		{
			SpawnRegistry.Clear();
		}

		private void Start()
		{
			PlayerSpawn ps = SpawnRegistry.FindRandom();

			Vector3 position = ps.transform.position;
			Quaternion rotation = ps.transform.rotation;

			// Create the player
			GameObject go = new GameObject( "_player" );
			go.transform.SetPositionAndRotation( position, rotation );

			// Create the player camera
			GameObject camera = new GameObject( "PlayerCamera" );
			camera.AddComponent<PlayerCamera>();

			// Set the player as the parent of the player camera
			GameObjectUtility.SetParentAndAlign( camera, go );
			go.AddComponent<BasePlayer>();
		}
	}
}
