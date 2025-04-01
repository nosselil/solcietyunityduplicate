using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit
{
	public class PlayerEntry : MonoBehaviour
	{
		public PlayerSettings selfPlayerSettings { get; protected set; }

		[SerializeField] Text nickNameText;
		[SerializeField] ColorDropdown colorDropdown;
		[SerializeField] UI.FactionDropdown factionDropdown;
		[SerializeField] Dropdown teamDropdown;
		[SerializeField] Button removeButton;

		Lobby parentLobby;
public void Start()
{
selfPlayerSettings.selectedFaction = parentLobby.GetStorage.availableFactions[1];
	}
		public void SetupWithPlayerSettings(PlayerSettings playerSettings, Lobby fromLobby)
		{
			selfPlayerSettings = playerSettings;

			colorDropdown.SetupWithData(fromLobby, this);
			colorDropdown.SetColorValue(playerSettings.color);

			teamDropdown.value = playerSettings.team;

			parentLobby = fromLobby;

			removeButton.interactable = playerSettings.isAI;
			nickNameText.text = playerSettings.nickName;

			factionDropdown.SetupWithData(fromLobby, this);
		}

		public void OnColorDropdownChanged(Color value)
		{
			parentLobby.PlayerChangeColor(selfPlayerSettings, value);
		}

		public void OnTeamDropdownChanged()
		{
			selfPlayerSettings.team = (byte)teamDropdown.value;
		}

		public void OnFactionDropdownChanged()
		{
			int selectedId = factionDropdown.GetComponent<Dropdown>().value;

			if (selectedId < parentLobby.GetStorage.availableFactions.Count)
				selfPlayerSettings.selectedFaction = parentLobby.GetStorage.availableFactions[selectedId];
			else
				selfPlayerSettings.selectedFaction = parentLobby.GetStorage.availableFactions[Random.Range(0, parentLobby.GetStorage.availableFactions.Count - 1)];
		}

		public void OnRemoveButton()
		{
			Destroy(gameObject);

			parentLobby.RemovePlayer(selfPlayerSettings);
		}
	}
}