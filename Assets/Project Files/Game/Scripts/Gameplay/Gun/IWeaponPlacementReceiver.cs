using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public interface IWeaponPlacementReceiver
    {
        public void TransferWeaponData(string weaponId, Transform weaponTransform);
        public void SetWeaponData(string weaponId, Transform weaponTransform);
        public bool HasWeaponDataChanged(string weaponId, Transform weaponTransform);
        public void CloneWeaponPlacementData(IWeaponPlacementReceiver other);

        public List<WeaponPlacementData> WeaponPlacementData { get; }
        public Transform GunHolder { get; }
        public GameObject GameObject { get; }
    }
}
