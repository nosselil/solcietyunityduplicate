using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{ 
	public static class InsaneEditorStyles
	{
		public static Color selectedButtonColor = new Color(0.8f, 1, 1, 1);
		public static GUIStyle headerTextStyle;
		public static GUIStyle smallHeaderTextStyle;
		public static GUIStyle paddedBoxStyle;

		static InsaneEditorStyles()
		{
			Reload();
		}
		
		public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
		{
			var rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			rect.height = thickness;
			rect.y += padding / 2;
			rect.x -= 2;
			rect.width += 6;
			EditorGUI.DrawRect(rect, color);
		}

		public static void Reload()
		{
			var bgColor = EditorGUIUtility.isProSkin ? new Color(0.35f, 0.35f, 0.35f, 1f) : new Color(0.85f, 0.85f, 0.85f, 1f);
			paddedBoxStyle = new GUIStyle();
			paddedBoxStyle.padding = new RectOffset(5, 5, 5, 5);
			paddedBoxStyle.normal.background = EditorExtensions.MakeTex(2, 2, bgColor);

			headerTextStyle = new GUIStyle();
			headerTextStyle.fontSize = 16;
			headerTextStyle.padding = new RectOffset(3, 3, 5, 5);

			smallHeaderTextStyle = new GUIStyle();
			smallHeaderTextStyle.fontSize = 14;
			smallHeaderTextStyle.padding = new RectOffset(3, 3, 5, 5);
			smallHeaderTextStyle.fontStyle = FontStyle.Bold;
		}
	}
}