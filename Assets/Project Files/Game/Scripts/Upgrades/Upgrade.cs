using UnityEngine;

namespace Watermelon.Upgrades
{
    public abstract class Upgrade<T> : BaseUpgrade where T : BaseUpgradeStage
    {
        [SerializeField]
        protected T[] upgrades;
        public override BaseUpgradeStage[] Upgrades => upgrades;

        public T GetCurrentStage()
        {
            if (upgrades.IsInRange(UpgradeLevel))
                return upgrades[UpgradeLevel];

            UpgradeLevel = upgrades.Length - 1;
            Debug.Log("[Perks]: Perk level is out of range!");

            return upgrades[UpgradeLevel];
        }

        public override void UpgradeStage()
        {
            if (upgrades.IsInRange(UpgradeLevel + 1))
            {
                UpgradeLevel += 1;

                InvokeOnUpgraded();
            }
        }

        public T GetNextStage()
        {
            if (upgrades.IsInRange(UpgradeLevel + 1))
                return upgrades[UpgradeLevel + 1];

            return null;
        }

        public T GetStage(int i)
        {
            if (upgrades.IsInRange(i))
                return upgrades[i];
            return null;
        }

        #region Debug
        [ShowNonSerialized, Label("Current Level")]
        [BoxFoldout("Debug", "Debug", order: 999)]
        private string DebugLevel => save != null ? save.UpgradeLevel.ToString() : "(runtime only)";

        [HorizontalGroup("Debug/Buttons")]
        [Button("<<")]
        public void DevResetUpgrade()
        {
            if(!Application.isPlaying)
            {
                Debug.LogWarning("Debug buttons are functional only during runtime.");

                return;
            }

            UpgradeLevel = 0;

            InvokeOnUpgraded();
        }

        [HorizontalGroup("Debug/Buttons")]
        [Button("<")]
        public void DevPrevStage()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Debug buttons are functional only during runtime.");

                return;
            }

            if (upgrades.IsInRange(UpgradeLevel - 1))
            {
                UpgradeLevel -= 1;

                InvokeOnUpgraded();
            }
        }

        [HorizontalGroup("Debug/Buttons")]
        [Button(">")]
        public void DevNextStage()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Debug buttons are functional only during runtime.");

                return;
            }

            if (upgrades.IsInRange(UpgradeLevel + 1))
            {
                UpgradeLevel += 1;

                InvokeOnUpgraded();
            }
        }

        [HorizontalGroup("Debug/Buttons")]
        [Button(">>")]
        public void DevMaxUpgrade()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Debug buttons are functional only during runtime.");

                return;
            }

            UpgradeLevel = UpgradesCount - 1;

            InvokeOnUpgraded();
        }
        #endregion
    }
}