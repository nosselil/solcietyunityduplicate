using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace com.whatereyes.gametutorial
{
	
	[CustomEditor(typeof(PopUpManager), true)]
	public class UIManagerEditor : Editor {
		public override void OnInspectorGUI() {
            this.serializedObject.Update();

            PrintImage("GT_Logo", Color.blue.WithAlpha(0f), 0, 25);
			
			this.serializedObject.ApplyModifiedProperties();
			
			GUILayout.Space(10.0f);
			
			this.DrawDefaultInspector();
		}
		
		public void PrintImage(string image, Color backGround, int leftOffset = 0, int rightOffset = 0) {
			GUILayout.BeginHorizontal();
			Color previousColor = GUI.contentColor;
			GUI.backgroundColor = backGround;
			GUILayout.Box(Resources.Load(image) as Texture, GUILayout.Width(EditorGUIUtility.currentViewWidth - rightOffset), GUILayout.Height(100));
			GUI.backgroundColor = previousColor;
			GUILayout.EndHorizontal();
		}
	}	
	
}