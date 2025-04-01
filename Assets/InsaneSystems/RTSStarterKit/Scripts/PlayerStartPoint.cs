using UnityEngine;

namespace InsaneSystems.RTSStarterKit 
{
	public class PlayerStartPoint : MonoBehaviour 
	{
		public bool IsHardLockerPlayerOnThisPoint { get { return lockWhichPlayerShouldBeSpawnedHere; } }
		public int IdOfPlayerToSpawn { get { return playerIdInMatchSettings; } }

		[SerializeField] bool lockWhichPlayerShouldBeSpawnedHere;
		[Tooltip("ID of player, which should be spawned here, from match settings (same to players order in this list, starts with 0).")]
		[SerializeField] [Range(0, 255)] int playerIdInMatchSettings = 0;

		void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;
			
			Gizmos.DrawWireSphere(transform.position, 2f);
		}
	}
}