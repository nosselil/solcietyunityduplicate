using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class ResourcesField : MonoBehaviour
	{
		public static List<ResourcesField> sceneResourceFields { get; private set; }
		
		[SerializeField] bool infResources = true;
		[Tooltip("Resources count on this field. This value will be ignored if Inf Resources set.")]
		[SerializeField] int resourcesAmount = 5000;

		[Header("New, if you want to limit harvesting to specific units then fill this field, if it's NULL, any unity can harvest")]
		public UnitData WhoCanHarvestThisResource = null;

		[Header("If NULL, the default resource is gold")]
		public GameResource ResourceData = null;
		void Awake()
		{
			if (sceneResourceFields == null)
				sceneResourceFields = new List<ResourcesField>();
			
			sceneResourceFields.Add(this);
		}

		public void OnMouseEnter()
		{
			if (Selection.selectedUnits.Count == 0 || !Selection.selectedUnits[0].data.isHarvester)
				return;

			if (WhoCanHarvestThisResource != null && Selection.selectedUnits[0].data != WhoCanHarvestThisResource)
			{
                Cursors.SetRestrictCursor();
                return;
            }

            var selectedHarvester = Selection.selectedUnits[0].GetModule<Harvester>();
			var needResourcesCursour = selectedHarvester.harvestedResources < selectedHarvester.MaxResources;
			
			if (needResourcesCursour)
				Cursors.SetResourcesCursor();
			else
				Cursors.SetRestrictCursor();
		}
		
		public void OnMouseExit()
		{
			Cursors.SetDefaultCursor();
		}

		public int TakeResources(int value)
		{
			if (infResources)
				return value;

			if (resourcesAmount >= value)
			{
				resourcesAmount -= value;
				
				return value;
			}

			var maxVal = resourcesAmount;
			resourcesAmount = 0;

			return maxVal;
		}

		public bool HaveResources()
		{
			return infResources || resourcesAmount > 0;
		}

		void OnDestroy()
		{
			sceneResourceFields.Remove(this);
		}
	}
}