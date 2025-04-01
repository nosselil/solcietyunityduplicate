using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This class handles Fog of War, sends info abouts units to FOW shader. </summary>
	public class FogOfWar : MonoBehaviour
	{
		public const int unitsLimit = 1000;
		
		readonly List<Unit> unitsToShowInFOW = new List<Unit>();
		
		[SerializeField] Transform fogOfWarPlane;
		[SerializeField] Material fogOfWarMaterial;

		float updateTime = 0.1f;
		float updateTimer;
		
		static readonly int maxUnitsId = Shader.PropertyToID("_MaxUnits");
		static readonly int totalUnitsId = Shader.PropertyToID("_ActualUnitsCount");
		static readonly int visionRadiusesTextureId = Shader.PropertyToID("_FOWVisionRadiusesTexture");
		static readonly int positionsTextureId = Shader.PropertyToID("_FOWPositionsTexture");

		Texture2D positionsTexture, visionRadiusesTexture;
		
		void Awake()
		{			
			Unit.unitSpawnedEvent += OnUnitSpawned;
			Unit.unitDestroyedEvent += OnUnitDestroyed;
		}
		
		void Start()
		{
			updateTime = GameController.instance.MainStorage.fowUpdateDelay;
			
			fogOfWarMaterial.SetFloat("_Enabled", GameController.instance.MainStorage.isFogOfWarOn ? 1f : 0f);
			Shader.SetGlobalFloat(maxUnitsId, unitsLimit);
			//fogOfWarPlane.position = new Vector3(0, 1, 0);
			
			// we use textures to send data to shader, because shaders GPU-based and for them it is simplier to work with graphics-type variables.
			// By the way, shader arrays works not so good on different OS, so texture is a preffered way to send big data to the shaders.
			positionsTexture = new Texture2D(unitsLimit, 1, TextureFormat.RGBAFloat, false, true);
			visionRadiusesTexture = new Texture2D(unitsLimit, 1, TextureFormat.RFloat, false, true);
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
				if (i >= unitsLimit)
					break;

				var pos = unitsToShowInFOW[i].transform.position;
				var positionColor = new Color(pos.x / 1024f, pos.y / 1024f, pos.z / 1024f, 1f); // Decreasing size to fit it in color and left free space for maps up to 1024 meters.
				
				positionsTexture.SetPixel(i, 0, positionColor);
				visionRadiusesTexture.SetPixel(i, 0, new Color(unitsToShowInFOW[i].data.visionDistance / 512f, 0f, 0f, 0f)); // Decreasing size to fit it in color and left free space for vision up to 512 meters.
			}
			
			visionRadiusesTexture.Apply();
			positionsTexture.Apply();
			
			Shader.SetGlobalFloat(totalUnitsId, unitsToShowInFOW.Count);
			Shader.SetGlobalTexture(visionRadiusesTextureId, visionRadiusesTexture);
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