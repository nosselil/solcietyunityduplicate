using UnityEngine;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.Controls
{
	public class Build : MonoBehaviour
	{
		public static bool isBuildMode;

		public delegate void OnBuild();

		static int terrainLayer;
		static int allExcludeTerrainLayer;
		static int sqrBuildDistance;

		static GameObject buildingToCreate;
		static Vector3 buildingSize;

		static GameObject drawer;
		static BuildingDrawer buildingDrawer;

		static OnBuild onLocalPlayerBuildCallback;

		static bool startedRotation;
		static bool canBuild;
		Vector3 finalHitPoint = Vector3.zero;

		Camera mainCamera;
		bool useGridForBuildMode;

		void Start()
		{
			terrainLayer = 1 << LayerMask.NameToLayer("Terrain");
			allExcludeTerrainLayer = ~terrainLayer;

			sqrBuildDistance = (int)Mathf.Pow(GameController.instance.MainStorage.maxBuildDistance, 2);

			mainCamera = Camera.main;
			useGridForBuildMode = GameController.instance.MainStorage.useGridForBuildingMode;
		}

		void Update()
		{
			if (isBuildMode)
			{
				if (Input.GetMouseButtonDown(1))
					DisableBuildMode();

				HandleBuilding(buildingSize);
			}
		}

		void HandleBuilding(Vector3 buildingSize)
		{
			var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			
			if (Physics.Raycast(ray, out hit, 1000, terrainLayer))
			{
				if (canBuild && Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
				{
					CreateBuilding(buildingToCreate, finalHitPoint, drawer.transform.rotation);
					return;
				}

				if (Input.GetMouseButton(0) && GameController.instance.MainStorage.allowBuildingsRotation)
				{
					if (startedRotation)
						drawer.transform.rotation = Quaternion.LookRotation(hit.point - drawer.transform.position);
					else if (Vector3.Distance(drawer.transform.position, hit.point) > 1.5f) // todo optimize
						startedRotation = true;
				}
				else
				{
					finalHitPoint = hit.point;

					if (useGridForBuildMode)
						finalHitPoint = new Vector3((int)hit.point.x, hit.point.y, (int)hit.point.z);

					drawer.transform.position = finalHitPoint;
				}

				canBuild = CheckZoneToBuild(finalHitPoint, buildingSize, drawer.transform.rotation, Player.localPlayerId);

				buildingDrawer.SetBuildingAllowedState(canBuild);
			}
		}

		public static bool CheckZoneToBuild(Vector3 atPoint, Vector3 buildingSize, Quaternion rotation, byte playerToCheck)
		{
			var halfSize = buildingSize / 2f;

			RaycastHit hit;
			float startHeight = 0;

			var colliders = Physics.OverlapBox(atPoint, halfSize , Quaternion.identity, allExcludeTerrainLayer); // todo replace with NonAlloc

			if (colliders.Length > 0)
				return false;

			for (int x = 0; x <= buildingSize.x; x++)
				for (int z = 0; z <= buildingSize.z; z++)
				{
					float xCheckPosition = Mathf.Clamp(x - halfSize.x, -halfSize.x, halfSize.x);
					float zCheckPosition = Mathf.Clamp(z - halfSize.z, -halfSize.z, halfSize.z);
					Vector3 checkPosition = new Vector3(xCheckPosition, 0, zCheckPosition);

					Ray ray = new Ray(atPoint + Vector3.up * 10f + checkPosition, -Vector3.up);

					if (Physics.Raycast(ray, out hit, 1000, terrainLayer))
					{
						if (x == 0 && z == 0)
						{
							startHeight = hit.point.y;
						}
						else
						{
							if (Mathf.Abs(startHeight - hit.point.y) > 0.15f)
								return false;
						}
					}
					else
					{
						return false;
					}
				}

			var playerCommCenter = Player.GetPlayerById(playerToCheck).playerProductionBuildings[0];
			return (playerCommCenter.transform.position - atPoint).sqrMagnitude < sqrBuildDistance;
		}

		public static GameObject CreateBuilding(GameObject buildingToSpawn, Vector3 atPoint, Quaternion rotation, byte playerOwner = 0)
		{
			GameObject spawnedBuilding = Instantiate(buildingToSpawn, atPoint, rotation);
			var buildingUnit = spawnedBuilding.GetComponent<Unit>();
			buildingUnit.SetOwner(playerOwner);

			if (playerOwner == Player.localPlayerId && onLocalPlayerBuildCallback != null)
				onLocalPlayerBuildCallback.Invoke();
			
			if (playerOwner == Player.localPlayerId)
				DisableBuildMode();

			return spawnedBuilding;
		}

		public void EnableBuildMode(GameObject buildingObject, OnBuild newOnLocalPlayerBuildCallback = null)
		{
			if (isBuildMode)
				DisableBuildMode();

			onLocalPlayerBuildCallback = newOnLocalPlayerBuildCallback;
			
			buildingToCreate = buildingObject;
			var buildingScript = buildingToCreate.GetComponent<Unit>();

			if (buildingScript)
				drawer = Instantiate(buildingScript.data.drawerObject, Vector3.zero, Quaternion.Euler(0, 180, 0));

			buildingDrawer = drawer.GetComponent<BuildingDrawer>();
			buildingSize = GetBuildingSize(buildingScript.data); 
	
			isBuildMode = true;
		}

		public static Vector3 GetBuildingSize(UnitData buildingData)
		{ 
			return buildingData.selfPrefab.GetComponent<BoxCollider>().size;
		}

		public static void DisableBuildMode()
		{
			Destroy(drawer);
			isBuildMode = false;
			startedRotation = false;
			canBuild = false;
		}
	}
}