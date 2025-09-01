using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class WeaponBoosterBehavior : ExplicitBoosterBehavior, IWeaponPlacementReceiver
    {
        [SerializeField] Transform gunHolder;
        public Transform GunHolder => gunHolder;

        public GameObject GameObject => gameObject;

        [SerializeField] List<WeaponPlacementData> weaponPlacementData;
        public List<WeaponPlacementData> WeaponPlacementData => weaponPlacementData;

        protected override void Init()
        {
            boosterCollider.enabled = linkedObstacle == null;

            boostedGun = LevelController.SkinsManager.GetGun(BoosterData.ExplicitId);
            boostedGun.transform.SetParent(GunHolder);
            TransferWeaponData(BoosterData.ExplicitId, boostedGun.transform);

            GunData gunData = LevelController.Database.GetGunData(BoosterData.ExplicitId);

            displayNameText.text = gunData.DisplayName;
            if (amountText != null) amountText.text = "";
        }

        public void TransferWeaponData(string weaponId, Transform weaponTransform)
        {
            if (weaponPlacementData == null) weaponPlacementData = new List<WeaponPlacementData>();

            for (int i = 0; i < weaponPlacementData.Count; i++)
            {
                WeaponPlacementData data = weaponPlacementData[i];
                if (data.WeaponId == weaponId)
                {
                    data.Apply(weaponTransform);

                    return;
                }
            }

            weaponPlacementData.Add(new WeaponPlacementData(weaponId, weaponTransform));
        }

        public void SetWeaponData(string weaponId, Transform weaponTransform)
        {
            if (weaponPlacementData == null) weaponPlacementData = new List<WeaponPlacementData>();

            for (int i = 0; i < weaponPlacementData.Count; i++)
            {
                WeaponPlacementData data = weaponPlacementData[i];
                if (data.WeaponId == weaponId)
                {
                    data.SetData(weaponTransform);

                    return;
                }
            }

            weaponPlacementData.Add(new WeaponPlacementData(weaponId, weaponTransform));
        }

        public bool HasWeaponDataChanged(string weaponId, Transform weaponTransform)
        {
            if (weaponPlacementData == null) weaponPlacementData = new List<WeaponPlacementData>();

            for (int i = 0; i < weaponPlacementData.Count; i++)
            {
                WeaponPlacementData data = weaponPlacementData[i];
                if (data.WeaponId == weaponId)
                {
                    return data.HasDataChanged(weaponTransform);
                }
            }

            return true;
        }

        public void CloneWeaponPlacementData(IWeaponPlacementReceiver other)
        {
            if (weaponPlacementData == null) weaponPlacementData = new List<WeaponPlacementData>();
            weaponPlacementData.Clear();

            for (int i = 0; i < other.WeaponPlacementData.Count; i++)
            {
                weaponPlacementData.Add(other.WeaponPlacementData[i].Clone());
            }
        }
    }
}
