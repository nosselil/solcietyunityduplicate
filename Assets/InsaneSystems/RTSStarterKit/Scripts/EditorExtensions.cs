#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{ 
	public static class EditorExtensions
	{
		public static void LoadAssetsToList<T>(List<T> listToAddIn, string searchFilter) where T : Object
		{
			listToAddIn.Clear();
            
			var assets = AssetDatabase.FindAssets(searchFilter);

			for (int i = 0; i < assets.Length; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(T)) as T;
				listToAddIn.Add(asset);
			}
		}
		
		public static Texture2D MakeTex(int width, int height, Color col)
		{
			var pixels = new Color[width * height];
 
			for(int i = 0; i < pixels.Length; i++)
				pixels[i] = col;
 
			var result = new Texture2D(width, height);
			result.SetPixels(pixels);
			result.Apply();
 
			return result;
		}
	}
}
#endif