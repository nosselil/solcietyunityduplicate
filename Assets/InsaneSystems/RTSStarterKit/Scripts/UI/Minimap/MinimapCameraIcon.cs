using UnityEngine;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class MinimapCameraIcon : MonoBehaviour, IBeginDragHandler, IEndDragHandler
	{
		[SerializeField] Minimap minimap;

		RectTransform rectTransform;

		bool isDragging;

		Camera mainCamera;

		void Awake()
		{
			rectTransform = GetComponent<RectTransform>();

			mainCamera = Camera.main;
		}

		void Update()
		{
			if (!isDragging)
				HandleCameraIcon();
			else
				ImitateDrag();

			if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
				isDragging = false;
		}

		void ImitateDrag()
		{
			rectTransform.position = Input.mousePosition;

			var anchoredPosition = rectTransform.anchoredPosition;

			if (IsPositionIconOutOfBounds(anchoredPosition))
				anchoredPosition = Minimap.InboundPositionToMap(anchoredPosition, minimap.MapImageSize);

			SetIconPosition(anchoredPosition, true);
		}

		void HandleCameraIcon()
		{
			RaycastHit hit;

			var viewCenter = Vector3.zero;

			if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, 1000))
				viewCenter = hit.point;

			var cameraPosition = WorldPositionToUI(viewCenter, true);

			SetIconPosition(cameraPosition);

			HandleIconRotation();
		}

		void HandleIconRotation()
		{
			var angles = transform.localEulerAngles;
			angles.z = -GameController.instance.cameraMover.transform.localEulerAngles.y;
			transform.localEulerAngles = angles;
		}
		
		public void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			
			isDragging = true;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			isDragging = false;
		}

		public void OnMapClick(PointerEventData eventData)
		{
			var offset = eventData.position - (Vector2)minimap.IconsPanel.position + Vector2.one * minimap.MapImageSize / 2f; // adding half of map size because draw zone have centered pivot.
			var resultPos = Minimap.InboundPositionToMap(offset, minimap.MapImageSize);

			SetIconPosition(resultPos, true);

			OnBeginDrag(eventData);
		}

		public void SetIconPosition(Vector2 newPosition, bool updateCameraPosition = false)
		{
			rectTransform.anchoredPosition = newPosition;

			if (updateCameraPosition)
			{ 
				Vector3 worldPosition = UIPositionToWorld(rectTransform.anchoredPosition, true);

				GameController.instance.cameraMover.SetPosition(worldPosition);
			}
		}

		bool IsPositionIconOutOfBounds(Vector2 position)
		{ 
			return position.x < 0 || position.y < 0 || position.x > minimap.MapImageSize || position.y > minimap.MapImageSize;
		}

		Vector2 WorldPositionToUI(Vector3 worldPosition, bool scaleToRadar = false)
		{
			Vector2 result = Vector2.zero;

			result.x = worldPosition.x;
			result.y = worldPosition.z;

			if (scaleToRadar)
				result *= minimap.GetScaleFactor();

			return result;
		}

		Vector3 UIPositionToWorld(Vector2 uiPosition, bool scaleToRealCoords = false)
		{
			Vector3 result = Vector3.zero;

			result.x = uiPosition.x;
			result.z = uiPosition.y;

			if (scaleToRealCoords)
				result /= minimap.GetScaleFactor();

			return result;
		}
	}
}
