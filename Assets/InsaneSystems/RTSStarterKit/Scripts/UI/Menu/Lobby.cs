using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit
{
	public class Lobby : MonoBehaviour
	{
		public delegate void OnFreeColorsChanged();

		[SerializeField] Storage storage;

		[Header("Scene UI")]
		[SerializeField] RectTransform playerEntriesPanel;
		[SerializeField] Button addAiPlayerButton;
		[SerializeField] Text mapNameText, maxPlayersText;
		[SerializeField] Dropdown mapDropdown;
		[SerializeField] Image mapPreviewImage;
		[SerializeField] GameObject loadingScreen;

		public List<Color> freeColors { get; protected set; }

		public event OnFreeColorsChanged freeColorsChangedEvent;

		public Storage GetStorage { get { return storage; } }

		bool startLoading;

		void Awake()
		{
			Application.targetFrameRate = 144;
			Time.timeScale = 1f;
		}

		void Start()
		{		
			freeColors = new List<Color>(storage.availablePlayerColors);
			
			MatchSettings.currentMatchSettings = new MatchSettings();

			SetupMapsDropdown();
			ChangeMap(mapDropdown);
		}

		void ResetPlayers()
		{
			for (var i = MatchSettings.currentMatchSettings.playersSettings.Count - 1; i >= 0 ; i--)
				RemovePlayer(MatchSettings.currentMatchSettings.playersSettings[i]);

			AddEntryForPlayer();
			AddEntryForPlayer(true, 1);
		}

		void Update()
		{
			if (startLoading)
			{
				SceneManager.LoadScene("MiniGame"); /*MatchSettings.currentMatchSettings.selectedMap.mapSceneName); */
				startLoading = false;
			}
		}

		void SetupMapsDropdown()
		{
			mapDropdown.ClearOptions();

			var dropdownOptions = new List<Dropdown.OptionData>();

			for (int i = 0; i < storage.availableMaps.Count; i++)
			{
				if (storage.availableMaps[i] == null)
				{
					Debug.LogWarning("Storage contains empty field in Available Maps, please remove it, now it will be ignored.");
					continue;
				}

				var mapName = storage.availableMaps[i].mapSceneName;
				dropdownOptions.Add(new Dropdown.OptionData(mapName));
			}

			mapDropdown.AddOptions(dropdownOptions);
		}

		void AddEntryForPlayer(bool isAI = false, int team = 0)
		{
			var playerColor = GetFreeColor();
			var playerSettings = new PlayerSettings((byte)team, playerColor, isAI);
			
			if (isAI)
				playerSettings.nickName = "AI Player";

			MatchSettings.currentMatchSettings.AddPlayerSettings(playerSettings);

			var spawnedObject = Instantiate(storage.playerEntry, playerEntriesPanel);
			var playerEntry = spawnedObject.GetComponent<PlayerEntry>();

			playerEntry.SetupWithPlayerSettings(playerSettings, this);
			playerSettings.playerLobbyEntry = playerEntry;
			
			TakeColor(playerColor);

			if (MatchSettings.currentMatchSettings.playersSettings.Count == MatchSettings.currentMatchSettings.selectedMap.maxMapPlayersCount)
				addAiPlayerButton.interactable = false;
		}

		public void RemovePlayer(PlayerSettings playerSettings)
		{
			FreeColor(playerSettings.color);
			MatchSettings.currentMatchSettings.RemovePlayerSettings(playerSettings);

			if (MatchSettings.currentMatchSettings.playersSettings.Count < MatchSettings.currentMatchSettings.selectedMap.maxMapPlayersCount)
				addAiPlayerButton.interactable = true;
			
			if (playerSettings.playerLobbyEntry)
				Destroy(playerSettings.playerLobbyEntry.gameObject);
		}

		public void RemovePlayer(byte id)
		{
			var playerSettings = MatchSettings.currentMatchSettings.playersSettings[id];
			RemovePlayer(playerSettings);
		}

		public void StartGame()
		{
			loadingScreen.SetActive(true);
			startLoading = true;
		}

		public void AddAIPlayer() { AddEntryForPlayer(true); }

		public Color GetFreeColor() { return freeColors[0]; }

		void TakeColor(Color color, bool callEvent = true)
		{
			freeColors.Remove(color);

			if (freeColorsChangedEvent != null && callEvent)
				freeColorsChangedEvent();
		}

		void FreeColor(Color color, bool callEvent = true)
		{
			freeColors.Add(color);

			if (freeColorsChangedEvent != null && callEvent)
				freeColorsChangedEvent();
		}

		public void PlayerChangeColor(PlayerSettings playerSettings, Color newColor)
		{
			FreeColor(playerSettings.color, false);
			playerSettings.color = newColor;
			TakeColor(newColor);
		}

		public List<Color> GetFreeColorsForPlayer(PlayerSettings playerSettings)
		{
			var colorsList = new List<Color>(freeColors);

			if (!colorsList.Contains(playerSettings.color))
				colorsList.Add(playerSettings.color);

			return colorsList;
		}

		public byte GetPlayerSettingsIndex(PlayerSettings playerSettings)
		{
			return (byte)MatchSettings.currentMatchSettings.playersSettings.LastIndexOf(playerSettings);
		}

		public void ChangeMap(Dropdown mapDropdownSender)
		{
			var map = storage.GetMapBySceneName(mapDropdownSender.captionText.text);
			MatchSettings.currentMatchSettings.SelectMap(map);
			
			ResetPlayers();
			
			mapPreviewImage.sprite = map.mapPreviewImage;

			mapNameText.text = "Map: " + map.mapSceneName;
			
			if (maxPlayersText)
				maxPlayersText.text = "Max players: " + map.maxMapPlayersCount;
		}

		public void OpenHotkeysSettings()
		{
			SceneManager.LoadScene("HotkeysSettings");
		}
	}
}