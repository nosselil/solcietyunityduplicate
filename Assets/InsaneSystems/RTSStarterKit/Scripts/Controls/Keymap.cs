using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Controls
{
	public enum KeyActionType
	{
		SelectSameUnitsOnScreen,
		SelectAllUnitsOnScreen,
		SelectAllUnits,
		AttackOrder,
		StopOrder,
		ShowCommCenter,
		GroupControlsHoldKey,
		AddToSelectionHoldKey,
		MoveToBase,
		SelectUnitAlternative,
		UnitsMoveWithLowestSpeed,
		SelectHarvesterAlternative,
		LockUnitMovement,
		DisperseUnitsToCorners,
		UseFirstAbility,
		UseSecondAbility,
		UseThirdAbility,
		UseFourthAbility
	}
	
	/// <summary> Keymap, which contains hotkeys for game actions. Keys can be changed in runtime.</summary>
	[System.Serializable]
	public class Keymap
	{
		public static Keymap loadedKeymap;

		public List<KeyAction> registeredKeys = new List<KeyAction>();

		static Keymap()
		{
			Load(true);
		}
		
		public Keymap()
		{
			SetupDefaultScheme();
		}
		
		public void CheckAllKeys()
		{
			for (int i = 0; i < registeredKeys.Count; i++)
				registeredKeys[i].IsKeyDown();
		}

		public void SetupDefaultScheme()
		{
			RegisterKey(new KeyAction(KeyActionType.SelectSameUnitsOnScreen, KeyCode.Q, true));
			RegisterKey(new KeyAction(KeyActionType.SelectAllUnitsOnScreen, KeyCode.E));
			RegisterKey(new KeyAction(KeyActionType.SelectAllUnits, KeyCode.W));
			RegisterKey(new KeyAction(KeyActionType.AttackOrder, KeyCode.A));
			RegisterKey(new KeyAction(KeyActionType.StopOrder, KeyCode.S));
			RegisterKey(new KeyAction(KeyActionType.ShowCommCenter, KeyCode.Space));
			RegisterKey(new KeyAction(KeyActionType.GroupControlsHoldKey, KeyCode.LeftControl));
			RegisterKey(new KeyAction(KeyActionType.AddToSelectionHoldKey, KeyCode.LeftShift));
			RegisterKey(new KeyAction(KeyActionType.MoveToBase, KeyCode.G));
			RegisterKey(new KeyAction(KeyActionType.SelectUnitAlternative, KeyCode.L));
			RegisterKey(new KeyAction(KeyActionType.SelectHarvesterAlternative, KeyCode.O));
			RegisterKey(new KeyAction(KeyActionType.UnitsMoveWithLowestSpeed, KeyCode.R));
			RegisterKey(new KeyAction(KeyActionType.LockUnitMovement, KeyCode.F));
			RegisterKey(new KeyAction(KeyActionType.DisperseUnitsToCorners, KeyCode.X));
			RegisterKey(new KeyAction(KeyActionType.UseFirstAbility, KeyCode.D));
			RegisterKey(new KeyAction(KeyActionType.UseSecondAbility, KeyCode.C));
			RegisterKey(new KeyAction(KeyActionType.UseThirdAbility, KeyCode.V));
			RegisterKey(new KeyAction(KeyActionType.UseFourthAbility, KeyCode.B));
		}

		/// <summary> You can get any action to setup it's Press event. </summary>
		public KeyAction GetAction(KeyActionType type)
		{
			for (int i = 0; i < registeredKeys.Count; i++)
				if (registeredKeys[i].type == type)
					return registeredKeys[i];

			var action = new KeyAction(type, KeyCode.K);
			RegisterKey(action);
			
			return action;
		}

		void RegisterKey(KeyAction keyAction)
		{
			var match = registeredKeys.Find(ka => ka.type == keyAction.type);
			if (match != null)
				registeredKeys.Remove(match);
			
			registeredKeys.Add(keyAction);
		}

		/// <summary> Saves keymap to the PC. </summary>
		public static void Save()
		{
			var jsonString = JsonUtility.ToJson(loadedKeymap);
			
			PlayerPrefs.SetString("Keymap", jsonString);
		}

		public static void Load(bool ignoreLoaded = false)
		{
			if (loadedKeymap != null && !ignoreLoaded)
				return;
			
			if (PlayerPrefs.HasKey("Keymap"))
			{
				loadedKeymap = JsonUtility.FromJson<Keymap>(PlayerPrefs.GetString("Keymap"));
				return;
			}
			
			loadedKeymap = new Keymap();
		}
	}

	[System.Serializable]
	public class KeyAction
	{
		public KeyActionType type;
		public KeyCode key;
		public bool haveDoublePress;
		public event OnKeyPressed onPressEvent, onDoublePressEvent;

		float doublePressTimer;

		public KeyAction(KeyActionType newType, KeyCode keyToUse, bool haveDoublePress = false)
		{
			type = newType;
			key = keyToUse;
			this.haveDoublePress = haveDoublePress;
		}

		public bool IsKeyActive()
		{
			return Input.GetKey(key);
		}
		
		public bool IsKeyDown()
		{
			if (haveDoublePress && doublePressTimer > 0)
				doublePressTimer -= Time.deltaTime;
			
			bool keyDown = Input.GetKeyDown(key);

			if (keyDown)
			{
				if ((doublePressTimer <= 0 || !haveDoublePress) && onPressEvent != null)
				{
					onPressEvent.Invoke();
					doublePressTimer = 0.2f;
				}

				if (haveDoublePress && doublePressTimer > 0 && onDoublePressEvent != null)
				{
					onDoublePressEvent.Invoke();
					doublePressTimer = 0f;
				}
			}
			
			return keyDown;
		}

		public delegate void OnKeyPressed();
	}
}