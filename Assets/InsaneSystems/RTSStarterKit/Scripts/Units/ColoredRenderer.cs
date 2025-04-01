using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[System.Serializable]
	public class ColoredRenderer
	{
		public bool usesHouseColorShader;
		[Range(0, 10)] public int materialId;
		public Renderer renderer;
		
		static readonly int houseColorId = Shader.PropertyToID("_HouseColor");

		public void SetMaterial(Material newMaterial)
		{
			var materials = renderer.materials;
			materials[materialId] = newMaterial;
			renderer.materials = materials;
		}

		public void SetColor(Color color)
		{
			var materials = renderer.materials;
			materials[materialId].SetColor(houseColorId, color);
			renderer.materials = materials;
		}
	}
}