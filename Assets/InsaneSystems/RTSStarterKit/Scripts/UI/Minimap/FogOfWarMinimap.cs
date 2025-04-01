using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class FogOfWarMinimap : MonoBehaviour
	{
		readonly List<Unit> unitsToShowInFOW = new List<Unit>();

		[SerializeField] Material fogOfWarUIMaterial;
		
		float updateTime = 0.5f;
		float updateTimer;
		
		static readonly int positionsTextureId = Shader.PropertyToID("_FOWMinimapPositionsTexture");

		Minimap minimap;
		
		Texture2D positionsTexture;
		
		void Awake()
		{
			Unit.unitSpawnedEvent += OnUnitSpawned;
			Unit.unitDestroyedEvent += OnUnitDestroyed;
		}
		
		void Start()
		{
			minimap = UIController.instance.minimapComponent;

			updateTime = GameController.instance.MainStorage.fowUpdateDelay;
			
			fogOfWarUIMaterial.SetFloat("_Enabled", GameController.instance.MainStorage.isFogOfWarOn ? 1f : 0f);
			fogOfWarUIMaterial.SetFloat("_MapSize", MatchSettings.currentMatchSettings.selectedMap.mapSize);
			fogOfWarUIMaterial.SetFloat("_MinimapSize", minimap.MapImageSize);
			
			positionsTexture = new Texture2D(FogOfWar.unitsLimit, 1, TextureFormat.RGBAFloat, false, true);
		}

		void Update()
		{
			if (updateTimer > 0f)
			{
				updateTimer -= Time.deltaTime;
				return;
			}

			RecalculateUnitsVisibilityInFOW();
			
			updateTimer = updateTime;
		}

		void RecalculateUnitsVisibilityInFOW()
		{
			for (int i = 0; i < unitsToShowInFOW.Count; i++)
			{
				if (i >= FogOfWar.unitsLimit)
					break;

				var icon = minimap.GetUnitIcon(unitsToShowInFOW[i]);

				if (!icon)
					continue;
				
				var pos = minimap.GetUnitOnMapPoint(unitsToShowInFOW[i]); // all positions being setted to the positions texture.
				var positionColor = new Color(pos.x / 1024f, pos.y / 1024f, 0f, 1f); // Decreasing size to fit it in color and left free space for maps up to 1024 meters.
				
				positionsTexture.SetPixel(i, 0, positionColor);
			}
			
			positionsTexture.Apply();
			
			Shader.SetGlobalTexture(positionsTextureId, positionsTexture);
		}

		void OnUnitSpawned(Unit unit)
		{
			if (unit.IsInMyTeam(Player.GetLocalPlayer().teamIndex))
				unitsToShowInFOW.Add(unit);
		}

		void OnUnitDestroyed(Unit unit)
		{
			unitsToShowInFOW.Remove(unit);
		}

		void OnDestroy()
		{
			Unit.unitSpawnedEvent -= OnUnitSpawned;
			Unit.unitDestroyedEvent -= OnUnitDestroyed;
		}
	}
}