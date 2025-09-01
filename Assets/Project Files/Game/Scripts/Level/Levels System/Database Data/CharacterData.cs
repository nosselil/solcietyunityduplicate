using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CharacterData : AbstractData
    {
        [SerializeField] CharacterType characterType;
        public CharacterType CharacterType => characterType;

        [SerializeField] bool isDefault;
        public bool IsDefault => isDefault;

        [SerializeField] bool isGunLocked;
        public bool IsGunLocked => isGunLocked;

        [SerializeField] string lockedGunId;
        public string LockedGunId => lockedGunId;

        [SerializeField] Sprite previewSprite;
        public Sprite PreviewSprite => previewSprite;

        [SerializeField] ShootingImpactType shootingImpactType;
        public ShootingImpactType ShootingImpactType => shootingImpactType;

        [SerializeField] string displayName;
        public string DisplayName => displayName;
    }
}
