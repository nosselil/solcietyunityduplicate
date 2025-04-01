using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public class RemoveDataWindow : EditorWindow
	{
		static string fileToRemoveName;
		static Object selectedData;
		static DataListEditor dataListEditorWindowCaller;

		static GUIStyle textStyle;

		public static void Init(string fileToRemove, Object dataFile, DataListEditor dataListEditorWindow)
		{
			RemoveDataWindow window = (RemoveDataWindow)EditorWindow.GetWindow(typeof(RemoveDataWindow));
			window.titleContent = new GUIContent("Remove data");
			window.Show();

			fileToRemoveName = fileToRemove;
			selectedData = dataFile;
			dataListEditorWindowCaller = dataListEditorWindow;

			textStyle = new GUIStyle();
			textStyle.richText = true;
			textStyle.padding = new RectOffset(3, 3, 10, 10);
			textStyle.fontSize = 14;
		}

		private void OnGUI()
		{
			GUILayout.Label("Are you sure you want to delete data file <b>" + selectedData.name + "</b>?", textStyle);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Yes"))
			{
				AssetDatabase.DeleteAsset(fileToRemoveName);
				dataListEditorWindowCaller.ReloadDatas();
				Close();
			}
			else if (GUILayout.Button("Cancel"))
			{
				Close();
			}
			GUILayout.EndHorizontal();
		}
	}
}