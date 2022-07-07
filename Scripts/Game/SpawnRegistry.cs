using System.Collections.Generic;
using UnityEngine;

namespace Game
{
	// SpawnRegistry can be used by gamemodes to select spawnpoints
	// In singleplayer for instance, a completely random spot is chosen
	public class SpawnRegistry
	{
		public static void Clear()
		{
			spawns.Clear();
		}

		public static PlayerSpawn FindRandom()
		{
			int size = spawns.Count;
			int rand = Random.Range( 0, size );

			if ( size == 0 )
				return null;

			return spawns[rand];
		}

		public static void Add( PlayerSpawn spawn )
		{
			foreach ( PlayerSpawn sp in spawns )
				if ( sp == spawn )
					return;

			spawns.Add( spawn );
		}

		private static List<PlayerSpawn> spawns = new List<PlayerSpawn>();
	}
}