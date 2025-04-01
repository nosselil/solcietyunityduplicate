using UnityEngine;
//this is the Original mover for the OG camera
namespace InsaneSystems.RTSStarterKit.Controls
{
	public class CameraMover : MonoBehaviour
	{
		public static CameraMover sceneInstance { get; private set; }
		
		public enum CameraMoveType
		{
			SpeedupFromCenter,
			Classic
		}
		
		[SerializeField] Transform cameraMoverTransform;
		[SerializeField] Transform cameraTransform;
		[SerializeField] Transform cameraDirectionsTransform;
		[SerializeField] int startCameraHeight = 18;

		[Tooltip("Size of mouse drag 'borders' of screen in pixels. In this area near screen border mouse will move camera in border side.")]
		[SerializeField] int screenBorderForMouse = 10;
		[Tooltip("Sets camera speed.")]
		[SerializeField, Range(2f, 30f)] float cameraSensivity = 8f;
		[SerializeField] CameraMoveType cameraMoveType = CameraMoveType.SpeedupFromCenter;

		[Tooltip("Max camera zoom value. Select value which is better for you. Big values can cause problems with camera.")]
		[SerializeField, Range(0f, 50f)] float maxZoom = 7f;

		Vector3 startCameraLocalPosition;
		float zoomValue;
		float zoomSpeed = 30f;

		Vector2 mouseMoveCenter;

		bool allowCameraRotation, allowCameraZoom;
		bool isCameraRotatingNow;

		float mapSize;

		readonly string mouseXAxisName = "Mouse X";
		readonly string mouseScrollAxisName = "Mouse ScrollWheel";
		readonly string terrainName = "Terrain";

		Camera mainCamera;

		float cameraSensivityForRMB, cameraSensivityForRMBClassic;

		Vector3 pointToRotateAround;
		
		void Awake()
		{
			mainCamera = Camera.main;
			sceneInstance = this;
			transform.position = new Vector3(transform.position.x, startCameraHeight, transform.position.z);
		}

		void Start()
		{
			allowCameraRotation = GameController.instance.MainStorage.allowCameraRotation;
			allowCameraZoom = GameController.instance.MainStorage.allowCameraZoom;

			startCameraLocalPosition = cameraTransform.localPosition;

			mapSize = MatchSettings.currentMatchSettings.selectedMap.mapSize;

			cameraSensivityForRMB = cameraSensivity / 20;
			cameraSensivityForRMBClassic = cameraSensivity * 10;

			InitializeHotkeys();
		}

		void InitializeHotkeys()
		{
			Keymap.loadedKeymap.GetAction(KeyActionType.ShowCommCenter).onPressEvent += OnPressCenterCamera;
		}

		void LateUpdate()
		{
			HandleDefaultMovement();

			HandleZoom();
		}

		void HandleDefaultMovement()
		{
			Vector2 mousePos = Input.mousePosition;

			if (Input.GetMouseButtonDown(1))
				mouseMoveCenter = Input.mousePosition;

			HandleRotation();

			if (Input.GetMouseButton(1) && !isCameraRotatingNow)
			{
				if (cameraMoveType == CameraMoveType.SpeedupFromCenter)
				{
					cameraMoverTransform.position += cameraSensivityForRMB *
					                                 (Input.mousePosition.x - mouseMoveCenter.x) * Time.deltaTime *
					                                 cameraDirectionsTransform.right;
					cameraMoverTransform.position += cameraSensivityForRMB *
					                                 (Input.mousePosition.y - mouseMoveCenter.y) * Time.deltaTime *
					                                 cameraDirectionsTransform.forward;
				}
				else if (cameraMoveType == CameraMoveType.Classic)
				{
					var directionNormalized = ((Vector2) Input.mousePosition - mouseMoveCenter).normalized;

					cameraMoverTransform.position += cameraSensivityForRMBClassic * directionNormalized.x *
					                                 Time.deltaTime * cameraDirectionsTransform.right;
					cameraMoverTransform.position += cameraSensivityForRMBClassic * directionNormalized.y *
					                                 Time.deltaTime * cameraDirectionsTransform.forward;
				}
			}
			else if (!isCameraRotatingNow)
			{
				if ((mousePos.x <= screenBorderForMouse && mousePos.x > -1) || IsKeyDown(KeyCode.LeftArrow))
					cameraMoverTransform.position -= cameraDirectionsTransform.right * (cameraSensivity * 10f * Time.deltaTime);
				else if ((mousePos.x >= Screen.width - screenBorderForMouse && mousePos.x < Screen.width + 1) || IsKeyDown(KeyCode.RightArrow))
					cameraMoverTransform.position += cameraDirectionsTransform.right * (cameraSensivity * 10f * Time.deltaTime);

				if ((mousePos.y <= screenBorderForMouse && mousePos.y > -1) || IsKeyDown(KeyCode.DownArrow))
					cameraMoverTransform.position -= cameraDirectionsTransform.forward * (cameraSensivity * 10f * Time.deltaTime);
				else if ((mousePos.y >= Screen.height - screenBorderForMouse && mousePos.y < Screen.height + 1) || IsKeyDown(KeyCode.UpArrow))
					cameraMoverTransform.position += cameraDirectionsTransform.forward * (cameraSensivity * 10f * Time.deltaTime);
			}

			var cameraPosition = cameraMoverTransform.position;

			cameraPosition.x = Mathf.Clamp(cameraPosition.x, 0, mapSize);
			cameraPosition.z = Mathf.Clamp(cameraPosition.z, 0, mapSize);

			cameraMoverTransform.position = cameraPosition;
		}

		void HandleRotation()
		{
			if (Input.GetMouseButtonDown(2) && allowCameraRotation)
			{
				RaycastHit hit;

				if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 1000))
					pointToRotateAround = hit.point;

				isCameraRotatingNow = true;
			}
			
			if (Input.GetMouseButton(2) && allowCameraRotation)
			{
				float inputX = Input.GetAxis(mouseXAxisName);

				if (inputX != 0)
				{
					cameraMoverTransform.RotateAround(pointToRotateAround, Vector3.up, inputX);
					cameraDirectionsTransform.RotateAround(pointToRotateAround, Vector3.up, inputX);
				}
			}

			if (Input.GetMouseButtonUp(2))
				isCameraRotatingNow = false;
		}
		
		void HandleZoom()
		{
			if (!allowCameraZoom)
				return;

			zoomValue = Mathf.Clamp(zoomValue + Input.GetAxis(mouseScrollAxisName) * zoomSpeed, 0, maxZoom);
			
			var localForward = cameraTransform.InverseTransformDirection(cameraTransform.forward) - Vector3.up; // strange fix of local camera problem on rotation. Works fine now.
			cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, startCameraLocalPosition + localForward * zoomValue, Time.deltaTime * 5f);
		}

		public bool IsKeyDown(KeyCode key) { return Input.GetKey(key); }

		
		public void OnPressCenterCamera()
		{
			SetPosition(Player.GetLocalPlayer().playerProductionBuildings[0].transform.position); // production 0 is always stab
		}

		public void SetPosition(Vector3 position)
		{
			if (!mainCamera)
				mainCamera = Camera.main;

			int checkLayer = 1 << LayerMask.NameToLayer(terrainName);
			RaycastHit hit;
			
			float zOffset = 0;

			cameraMoverTransform.position = new Vector3(position.x, cameraMoverTransform.position.y, position.z); 
			
			var midScreenRay = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
			
			if (Physics.Raycast(midScreenRay, out hit, 1000, checkLayer))
				zOffset = hit.point.z - position.z;

			cameraMoverTransform.position = new Vector3(position.x, cameraMoverTransform.position.y, position.z - zOffset); 
		}
	}
}