using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class UIUpgradesPanel : MonoBehaviour
    {
        [SerializeField] List<UpgradeType> upgrades;

        [SerializeField] RectTransform upgradeItemsParent;
        [SerializeField] GameObject upgradeItemPrefab;

        private List<UIUpgradeItem> upgradeItems;

        public void Init()
        {
            upgradeItems = new List<UIUpgradeItem>();

            for (int i = 0; i < upgrades.Count; i++)
            {
                UpgradeType upgradeType = upgrades[i];

                UIUpgradeItem upgradeItem = Instantiate(upgradeItemPrefab).GetComponent<UIUpgradeItem>();

                upgradeItem.transform.SetParent(upgradeItemsParent);
                upgradeItem.transform.ResetLocal();

                upgradeItem.Init(upgradeType);

                upgradeItems.Add(upgradeItem);
            }
        }
    }
}
