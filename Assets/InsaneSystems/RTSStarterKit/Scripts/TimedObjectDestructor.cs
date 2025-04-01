using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Misc
{
	public class TimedObjectDestructor : MonoBehaviour
	{
		[SerializeField] float timeToDestroy = 3f;

		void Update()
		{
			timeToDestroy -= Time.deltaTime;

			if (timeToDestroy <= 0)
				Destroy(gameObject);
		}

		public void SetCustomTime(float newTime) { timeToDestroy = newTime; }
	}
}