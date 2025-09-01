using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class WeaponPlacementData
    {
        [SerializeField] string weaponId;
        public string WeaponId { get => weaponId; set => weaponId = value; }

        [SerializeField] Vector3 localOffset;
        [SerializeField] Vector3 localEulerAngles;
        [SerializeField] Vector3 localScale = Vector3.one;

        private WeaponPlacementData(string weaponId)
        {
            WeaponId = weaponId;
        }

        public WeaponPlacementData(string weaponId, Transform weapon)
        {
            WeaponId = weaponId;
            SetData(weapon);
        }

        public void Apply(Transform weapon)
        {
            weapon.localPosition = localOffset;
            weapon.localEulerAngles = localEulerAngles;
            weapon.localScale = localScale;
        }

        public void SetData(Transform weapon)
        {
            localOffset = weapon.localPosition;
            localEulerAngles = weapon.localEulerAngles;
            localScale = weapon.localScale;
        }

        public bool HasDataChanged(Transform weapon)
        {
            return weapon.localPosition != localOffset || weapon.localEulerAngles != localEulerAngles || weapon.localScale != localScale;
        }

        public WeaponPlacementData Clone()
        {
            var data = new WeaponPlacementData(weaponId);
            data.localOffset = localOffset;
            data.localEulerAngles = localEulerAngles;
            data.localScale = localScale;

            return data;
        }
    }
}
