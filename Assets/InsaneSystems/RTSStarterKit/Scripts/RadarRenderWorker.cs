using UnityEngine;
using System.Collections;

namespace InsaneSystems.RTSStarterKit
{
	public class RadarRenderWorker : MonoBehaviour
	{
		bool isPreparedToWork;
		Camera radarCamera;

		Texture2D fowRenderedTexture;
		RenderTexture FoWRT;
		
		int resolution = 512;

		public void Start()
		{
			SetupCorrectParameters();
		}

		void SetupCorrectParameters()
		{
			var renderLight = GetComponent<Light>();

			if (renderLight)
				renderLight.enabled = true;

			float mapSize = MatchSettings.currentMatchSettings.selectedMap.mapSize;

			radarCamera = GetComponent<Camera>();

			radarCamera.orthographicSize = mapSize / 2f;
			radarCamera.transform.position = new Vector3(mapSize / 2f, 64, mapSize / 2f);
			radarCamera.aspect = 1.0f;
			
			var tempRT = new RenderTexture(resolution, resolution, 24);
			
			radarCamera.targetTexture = tempRT;
			//radarCamera.Render();

			RenderTexture.active = tempRT;

			isPreparedToWork = true;
		}

		void OnPostRender()
		{
			if (!isPreparedToWork)
				return;

			MapRender();
		}

		void MapRender()
		{
			var renderedTexture = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);

			renderedTexture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
			renderedTexture.Apply();

			RenderTexture.active = null;

			UI.UIController.instance.minimapComponent.SetMapBackground(renderedTexture);
			
			Destroy(gameObject);
		}
	}

}