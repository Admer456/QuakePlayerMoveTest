using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// This is a 'marker' for level designers to specify
// a location where a player may spawn
public class PlayerSpawn : MonoBehaviour
{
	[MenuItem("GameObject/Entities/PlayerSpawn", false, 10)]
	static void CreateCustom(MenuCommand mc)
	{   // Create new game object
		GameObject go = new GameObject("PlayerSpawn");
		go.AddComponent<PlayerSpawn>();

		// Stuff
		GameObjectUtility.SetParentAndAlign(go, mc.context as GameObject);

		// More editor stuff
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}

	void OnEnable()
	{
		Reset();
	}

	private void Start()
	{
		Reset();
	}

	// Make sure to always register the spawnpoint
	void Reset()
	{
		Game.SpawnRegistry.Add(this);
	}
}
