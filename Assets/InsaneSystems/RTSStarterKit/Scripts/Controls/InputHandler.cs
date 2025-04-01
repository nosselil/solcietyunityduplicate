using InsaneSystems.RTSStarterKit.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.Controls
{
	public class InputHandler : MonoBehaviour
	{
		public static InputHandler sceneInstance { get; private set; }

		/// <summary> Contains current player world cursor hit point, getted by ScreenPointToRay method. </summary>
		public static RaycastHit currentCursorWorldHit;

		static CustomControls customControlsMode;
		public static HotkeysInputType hotkeysInputMode { get; private set; }

		public string buildingInputKeys
		{
			get { return "qwerasdfyxcv"; }
		}

		void Awake()
		{
			sceneInstance = this;
		}

		void Start()
		{
			hotkeysInputMode = HotkeysInputType.Default;

			Selection.productionUnitSelected += OnProductionSelected;
			Selection.selectionCleared += OnClearSelection;
		}

		void OnProductionSelected(Production production)
		{
			hotkeysInputMode = HotkeysInputType.Building;
		}
		
		void OnClearSelection()
		{
			hotkeysInputMode = HotkeysInputType.Default;
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				UIController.instance.pauseMenu.ShowOrHide();
			
			HandleSelectionInput();
			HandleOrdersInput();
			HandleWorldCursorPosition();
			HandleCustomControls();

			HandleHotkeys();
		}

		void HandleHotkeys()
		{
			switch (hotkeysInputMode)
			{
				case HotkeysInputType.Default:
					Keymap.loadedKeymap.CheckAllKeys();
					break;
				
				case HotkeysInputType.Building:
					for (int i = 0; i < buildingInputKeys.Length; i++)
					{
						if (Input.GetKeyDown(buildingInputKeys[i].ToString()))
						{
							var icon = UIController.instance.productionIconsPanel.GetIcon(i);
							
							if (icon)
								icon.OnClick();
						}
					}
					break;
			}
		}

		void HandleCustomControls()
		{
			if (customControlsMode == CustomControls.None)
				return;
			
			if (Input.GetMouseButtonDown(1))
				SetCustomControls(CustomControls.None);

			if (Input.GetMouseButtonDown(0) && currentCursorWorldHit.collider)
			{
				var unit = currentCursorWorldHit.collider.GetComponent<Unit>();

				if (!unit || !unit.data.isBuilding || !unit.IsOwnedByPlayer(Player.localPlayerId))
					return;

				if (customControlsMode == CustomControls.Repair)
				{
					var repair = unit.GetComponent<Repair>();

					if (repair)
						repair.RemoveRepair();
					else
						unit.gameObject.AddComponent<Repair>();
				}
				else if (customControlsMode == CustomControls.Sell)
				{
					var damageable = unit.GetModule<Damageable>();

					if (!damageable)
						return;
					
					var sellPercents = GameController.instance.MainStorage.buildingSellReturnPercents;
					
					unit.GetOwnerPlayer().AddMoney((int)(unit.data.price * sellPercents * damageable.GetHealthPercents()));
					
					damageable.Die();
				}
			}
		}

		void HandleWorldCursorPosition()
		{
			var ray = GameController.cachedMainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 10000))
				currentCursorWorldHit = hit;
		}

		void HandleSelectionInput()
		{
			if (Build.isBuildMode)
				return;

			if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
			{
				Selection.startMousePosition = Input.mousePosition;

				Selection.OnStartSelection();
			}

			if (Input.GetMouseButtonUp(0))
			{
				Selection.endMousePosition = Input.mousePosition;

				if (IsJustClick(Selection.startMousePosition, Selection.endMousePosition) && !EventSystem.current.IsPointerOverGameObject())
					Selection.OnSingleSelection();
				else if (Selection.isSelectionStarted)
					Selection.OnEndSelection();
			}
		}

		static bool IsJustClick(Vector2 positionA, Vector2 positionB) { return Vector2.Distance(positionA, positionB) < 5f; }

		void HandleOrdersInput()
		{
			if (Selection.selectedUnits.Count == 0)
				return;

			if (Input.GetMouseButtonUp(1))
				Ordering.GiveOrder(Input.mousePosition, Input.GetKey(KeyCode.LeftShift));
		}

		public void SetCustomControls(CustomControls newControls)
		{
			customControlsMode = customControlsMode != newControls ? newControls : CustomControls.None;

			switch (customControlsMode)
			{
				case CustomControls.Sell: Cursors.SetSellCursor(); break;
				case CustomControls.Repair: Cursors.SetRepairCursor(); break;
				case CustomControls.None:
					Cursors.lockCursorChange = false; 
					Cursors.SetDefaultCursor();
					break;
			}
		}
	}
}