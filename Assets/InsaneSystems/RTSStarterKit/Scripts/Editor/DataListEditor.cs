using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary>
	/// Special EditorWindow class, simplifies creation of Data-files editors (UnitDatas, ProductionCategories etc in same way. Should be used as template - derive from it.
	/// </summary>
	public abstract class DataListEditor : EditorWindow
	{
		protected static int selectedDataId = 0, loadedDatasCount = 0;

		protected Vector2 datasListScrollPos, dataEditorScrollPos, actionsScrollPos;

		public virtual void OnGUI()
		{
			if (loadedDatasCount == 0)
			{
				ReloadDatas();
				return;
			}

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical(InsaneEditorStyles.paddedBoxStyle, GUILayout.MaxWidth(230));

			datasListScrollPos = EditorGUILayout.BeginScrollView(datasListScrollPos);
			GUILayout.Label("Datas list", InsaneEditorStyles.headerTextStyle);

			DrawList();

			InsaneEditorStyles.DrawUILine(Color.gray, 1);

			DrawCreateButton();

			EditorGUILayout.EndScrollView();
			GUILayout.EndVertical();

			GUILayout.BeginVertical(InsaneEditorStyles.paddedBoxStyle);

			GUILayout.Label("Editor of " + GetEditorTitle(), InsaneEditorStyles.headerTextStyle);
			DrawEditor();
			GUILayout.EndVertical();

			GUILayout.BeginVertical(InsaneEditorStyles.paddedBoxStyle, GUILayout.MaxWidth(200));
			actionsScrollPos = EditorGUILayout.BeginScrollView(actionsScrollPos);
			GUILayout.Label("Actions", InsaneEditorStyles.headerTextStyle);
			DrawActions();
			EditorGUILayout.EndScrollView();
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
		}

		protected abstract void DrawList();
		protected abstract void DrawCreateButton();
		protected abstract string GetEditorTitle();
		protected abstract void DrawEditor();
		protected abstract void DrawActions();
		public abstract void ReloadDatas();
	}
}