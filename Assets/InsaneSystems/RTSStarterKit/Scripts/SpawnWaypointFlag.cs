using UnityEngine;
using UnityEngine.AI;

namespace InsaneSystems.RTSStarterKit
{
	public class SpawnWaypointFlag : MonoBehaviour
	{
		GameObject selfObject;
		new Transform transform;

		Production currentProduction;

		void Awake()
		{
			selfObject = gameObject;
			transform = GetComponent<Transform>();
		}

		void Start()
		{
			Production.productionSelected += OnProductionSelected;
			Production.productionUnselected += OnProductionUnselected;

			Hide();
		}

		void Update()
		{
			if (Input.GetMouseButtonDown(1) && currentProduction != null)
			{
				if (currentProduction.SpawnWaypoint == null)
				{
					Debug.LogWarning("No Move Waypoint setted up in selected Production. Please, set up this setting in component of prefab.");
					return;
				}

				NavMeshHit navHit;
				NavMesh.SamplePosition(Controls.InputHandler.currentCursorWorldHit.point, out navHit, 10, NavMesh.AllAreas);

				currentProduction.SpawnWaypoint.position = navHit.position;
				ShowAtPoint(currentProduction.SpawnWaypoint.position);
			}
		}

		void OnProductionSelected(Production production)
		{
			currentProduction = production;

			if (production.SpawnWaypoint)
				ShowAtPoint(production.SpawnWaypoint.position);
			else
				ShowAtPoint(production.transform.position);
		}

		void OnProductionUnselected(Production production) { Hide(); }

		void ShowAtPoint(Vector3 point)
		{
			selfObject.SetActive(true);
			transform.position = point;
		}

		void Hide()
		{
			currentProduction = null;

			selfObject.SetActive(false);
		}

		void OnDestroy()
		{
			Production.productionSelected -= OnProductionSelected;
			Production.productionUnselected -= OnProductionUnselected;
		}
	}
}