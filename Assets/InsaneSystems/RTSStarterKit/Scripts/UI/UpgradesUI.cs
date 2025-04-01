using InsaneSystems.RTSStarterKit.UI;
using InsaneSystems.RTSStarterKit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
    public class UpgradesUI : MonoBehaviour
    {
        [SerializeField] GameObject selfObject;
        [SerializeField] Text upgradePriceUILabel;

        Unit selectedUnit;

        void Start()
        {
            Controls.Selection.unitSelected += OnUnitSelected;
            Controls.Selection.selectionCleared += OnClearSelection;
        }

        void Update()
        {
            if (!selfObject.activeSelf)
                return;
        }

        public void OnClearSelection() { Hide(); }

        public void OnUnitSelected(Unit unit)
        {
            selectedUnit = unit;
            if(selectedUnit.data.UpgradePrice > 0)
            {
                Show();

                upgradePriceUILabel.text = "Upgrade Price:" + selectedUnit.data.UpgradePrice;
            }
            else
            {
                Hide();
            }
        }

        void Show() { selfObject.SetActive(true); }
        void Hide() { selfObject.SetActive(false); }

        public void OnAttemptToUprade()
        {
            if (Player.GetLocalPlayer().money < selectedUnit.data.UpgradePrice)
                return;
            Player.GetLocalPlayer().money -= selectedUnit.data.UpgradePrice;

            Unit[] unitsSpawned = FindObjectsOfType<Unit>();
            Unit _selectedUnit = null;
            for (int i = 0; i < unitsSpawned.Length; i++)
            {
                if (unitsSpawned[i].data == selectedUnit.data.BuildingToUpgrade
                    && unitsSpawned[i].IsOwnedByPlayer(Player.GetLocalPlayer().id))
                {
                    _selectedUnit = unitsSpawned[i];
                    break;
                }
            }
            if (_selectedUnit == null)
                return;

            SpawnController.SpawnUnit(selectedUnit.data.NewBuildingData, Player.GetLocalPlayer().id, _selectedUnit.transform);
            
        }

        void OnDestroy()
        {
            Controls.Selection.unitSelected -= OnUnitSelected;
            Controls.Selection.selectionCleared -= OnClearSelection;
        }
    }
}