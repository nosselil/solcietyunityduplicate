using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class MinimapPanel : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
	{
		Minimap minimap;
		MinimapCameraIcon minimapCameraIcon;

		bool isPointerOn;

		void Start()
		{
			minimapCameraIcon = FindObjectOfType<MinimapCameraIcon>();
			minimap = FindObjectOfType<Minimap>();
		}

		void Update()
		{
			if (isPointerOn)
				Cursors.SetMapOrderCursor();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				minimapCameraIcon.OnMapClick(eventData);
			}
			else if (eventData.button == PointerEventData.InputButton.Right)
			{
				var offset = eventData.position - (Vector2)minimap.IconsPanel.position + Vector2.one * minimap.MapImageSize / 2f; // adding half of map size because draw zone have centered pivot.
				offset /= minimap.GetScaleFactor();
				
				var boundedMapPos = Minimap.InboundPositionToMap(offset, minimap.MapImageSize);
				
				Ordering.GiveMapOrder(boundedMapPos);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (Controls.Selection.selectedUnits.Count > 0 && Controls.Selection.selectedUnits[0].data.hasMoveModule)
			{
				isPointerOn = true;
				Cursors.SetMapOrderCursor();
			}
		}
		
		public void OnPointerExit(PointerEventData eventData)
		{
			Cursors.SetDefaultCursor();
			isPointerOn = false;
		}
	}
}