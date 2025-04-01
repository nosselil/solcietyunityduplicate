using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	public class TriggerController : MonoBehaviour
	{
		public static TriggerController instance { get; protected set; }

		[SerializeField] TriggerData[] triggerDatas = new TriggerData[0];

		private void Awake()
		{
			if (instance)
			{
				Debug.LogWarning("Several Trigger Controllers found on scene. All exclude one will be disabled. Your scene should have only one TriggerController.");
				enabled = false;
			}
			else
			{
				instance = this;
			}
		}

		void Start()
		{

		}

		public void ExecuteTrigger(string triggerTextId)
		{
			for (int i = 0; i < triggerDatas.Length; i++)
				if (triggerDatas[i].triggerTextId == triggerTextId && triggerDatas[i].trigger)
				{
					triggerDatas[i].trigger.Execute();
					return;
				}

			Debug.LogWarning("No trigger with name " + triggerTextId + " found!");
		}

		public int GetTriggerIndexByName(string name)
		{
			for (int i = 0; i < triggerDatas.Length; i++)
				if (triggerDatas[i].triggerTextId == name)
					return i;

			return -1;
		}

		public string GetNameByIndex(int index)
		{
			if (index < triggerDatas.Length)
				return triggerDatas[index].triggerTextId;

			return "NO SUCH TRIGGER";
		}

		public string[] GetTriggersNames()
		{
			string[] names = new string[triggerDatas.Length];

			for (int i = 0; i < triggerDatas.Length; i++)
				names[i] = triggerDatas[i].triggerTextId;

			return names;
		}

		public int GetTriggersCount() { return triggerDatas.Length; }
	}
}