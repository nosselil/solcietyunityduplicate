using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[CreateAssetMenu(fileName = "TextsLibrary", menuName = "RTS Starter Kit/Texts Library")]
	public class TextsLibrary : ScriptableObject
	{
		public List<TextData> uiTextDatas = new List<TextData>();

		public string GetUITextById(string textId)
		{
			return GetTextFromListById(uiTextDatas, textId);
		}

		public string GetTextFromListById(List<TextData> textDatas, string textId)
		{
			for (int i = 0; i < textDatas.Count; i++)
				if (textDatas[i].textId == textId)
					return textDatas[i].englishText;

			return "No text added";
		}
	}

	[System.Serializable]
	public class TextData
	{
		public string textId = "textId";
		public string englishText = "English Text";
	}
}