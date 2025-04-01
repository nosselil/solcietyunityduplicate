using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	[RequireComponent(typeof(BoxCollider))]
	[RequireComponent(typeof(Rigidbody))]
	public class TriggerZone : MonoBehaviour
	{
		[Tooltip("Add here text ids of all triggers you want to call when this zone being triggered. You should setup text id of triggers in Trigger Editor to call them.")]
		[Trigger] public List<string> triggersToCall = new List<string>();
		public TriggerCondition conditionToStartTriggers = new TriggerCondition();
		public bool removeThisZoneAfterTrigger = true;
		public Color zoneColor = new Color(0.6f, 0f, 1f, 1f);

		void Start()
		{
			gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

			if (!GetComponent<BoxCollider>())
				gameObject.AddComponent<BoxCollider>();

			var boxCollider = GetComponent<BoxCollider>();
			boxCollider.isTrigger = true;

			if (!GetComponent<Rigidbody>())
				gameObject.AddComponent<Rigidbody>();

			GetComponent<Rigidbody>().isKinematic = true;
		}

		private void Update()
		{
			if (conditionToStartTriggers.conditionType == TriggerConditionType.ByTimeLeft)
				if (conditionToStartTriggers.IsConditionTrue())
					DoTriggerAction();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (conditionToStartTriggers.conditionType != TriggerConditionType.ByEnteringZoneUnits)
				return;

			var otherUnit = other.GetComponent<Unit>();

			if (!otherUnit)
				return;

			OnTriggerEnterExitCheck(otherUnit, true);
		}

		private void OnTriggerExit(Collider other)
		{
			if (conditionToStartTriggers.conditionType != TriggerConditionType.ByExitingZoneUnits)
				return;

			var otherUnit = other.GetComponent<Unit>();

			if (!otherUnit)
				return;

			OnTriggerEnterExitCheck(otherUnit, false);
		}

		void OnTriggerEnterExitCheck(Unit unitTriggeredZone, bool isEnter)
		{
			if (!conditionToStartTriggers.IsConditionTrue(unitTriggeredZone))
				return;

			DoTriggerAction();
		}

		void DoTriggerAction()
		{
			for (int i = 0; i < triggersToCall.Count; i++)
				TriggerController.instance.ExecuteTrigger(triggersToCall[i]);

			if (removeThisZoneAfterTrigger)
				Destroy(gameObject);
		}

		private void OnDrawGizmos()
		{
			var boxCollider = GetComponent<BoxCollider>();
			var scaleModifier = Vector3.one;

			if (boxCollider)
				scaleModifier = boxCollider.size;

			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.3f);
			Gizmos.DrawCube(Vector3.zero, scaleModifier);
			Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
			Gizmos.DrawWireCube(Vector3.zero, scaleModifier);
		}
	}
}