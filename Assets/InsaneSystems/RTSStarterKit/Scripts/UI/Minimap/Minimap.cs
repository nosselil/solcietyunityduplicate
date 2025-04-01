using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class Minimap : MonoBehaviour
	{
		const float radarUpdateDelay = 0.25f;
		
		[SerializeField] RectTransform iconsPanel;
		[SerializeField] RawImage mapBackground;
		[SerializeField] RawImage fowImage;
		[SerializeField] Canvas mainCanvas;

		Dictionary<Unit, Image> unitsIcons = new Dictionary<Unit, Image>();

		float radarUpdateTimer;

		float mapSize = 256; 
		float final2DScale;

		public float MapImageSize { get; protected set; }

		public RectTransform IconsPanel { get { return iconsPanel; } }

		bool isFOWUsed;

		void Awake()
		{
			Unit.unitSpawnedEvent += OnSpawnedUnit;
			Unit.unitChangedOwnerEvent += OnChangedUnitOwner;
			
			MapImageSize = iconsPanel.sizeDelta.x;
		}

		void Start()
		{
			mapSize = MatchSettings.currentMatchSettings.selectedMap.mapSize;
			final2DScale = iconsPanel.sizeDelta.x / mapSize;
			
			isFOWUsed = GameController.instance.MainStorage.isFogOfWarOn;
			
			fowImage.enabled = isFOWUsed;
		}

		void Update()
		{
			radarUpdateTimer -= Time.deltaTime;

			if (radarUpdateTimer <= 0)
			{
				foreach (KeyValuePair<Unit, Image> entry in unitsIcons)
				{
					if (entry.Key == null)
					{
						Destroy(entry.Value.gameObject);

						continue;
					}

					var unit = entry.Key;
					var unitIcon = entry.Value;

					if (isFOWUsed)
						unitIcon.enabled = unit.GetModule<FogOfWarModule>().isVisibleInFOW;
					
					unitIcon.rectTransform.anchoredPosition = GetUnitOnMapPoint(unit, true);
				}

				unitsIcons = (from icon in unitsIcons
							  where (icon.Key != null)
							  select icon).ToDictionary(icon => icon.Key, icon => icon.Value);

				radarUpdateTimer = radarUpdateDelay;
			}
		}

		void OnSpawnedUnit(Unit unit)
		{
			var spawnedIconObject = Instantiate(GameController.instance.MainStorage.unitMinimapIconTemplate, iconsPanel);
			Image iconImage = spawnedIconObject.GetComponent<Image>();

			iconImage.color = Player.GetPlayerById(unit.OwnerPlayerId).color;
			unitsIcons.Add(unit, iconImage);
		}

		void OnChangedUnitOwner(Unit unit, int newOwnerPlayerId)
		{
			Image image;
			unitsIcons.TryGetValue(unit, out image);

			if (image)
				image.color = Player.GetPlayerById((byte)newOwnerPlayerId).color;
		}

		public Image GetUnitIcon(Unit unit)
		{
			Image icon;
			unitsIcons.TryGetValue(unit, out icon);

			return icon;
		}

		public Vector2 GetUnitOnMapPoint(Unit unit, bool scaledToMap = false)
		{
			var result = GetPositionInMapCoords(unit.transform.position);

			if (scaledToMap)
				result *= final2DScale;
			
			return result;
		}

		public Vector2 GetPositionInMapCoords(Vector3 position)
		{
			Vector2 resultPosition = new Vector2(Mathf.CeilToInt(position.x), Mathf.CeilToInt(position.z));
			resultPosition.x = Mathf.Clamp(resultPosition.x, 0, mapSize);
			resultPosition.y = Mathf.Clamp(resultPosition.y, 0, mapSize);

			return resultPosition;
		}

		public static Vector3 GetMapPointInWorldCoords(Vector2 mapPoint)
		{
			Vector3 resultPosition = new Vector3(mapPoint.x, 0f, mapPoint.y);

			return resultPosition;
		}

		public void SetMapBackground(Texture2D texture) { mapBackground.texture = texture; }
		public void SetFowTexture(Texture2D texture) { fowImage.texture = texture; }

		public float GetMapSize() { return mapSize; }
		public float GetDrawPanelSize() { return iconsPanel.sizeDelta.x; }
		public float GetScaleFactor() { return final2DScale; }
		
		public static Vector2 InboundPositionToMap(Vector2 position, float mapImageSize)
		{
			position.x = Mathf.Clamp(position.x, 0, mapImageSize);
			position.y = Mathf.Clamp(position.y, 0, mapImageSize);

			return position;
		}

		void OnDestroy()
		{
			Unit.unitSpawnedEvent -= OnSpawnedUnit;
			Unit.unitChangedOwnerEvent -= OnChangedUnitOwner;
		}
	}
}