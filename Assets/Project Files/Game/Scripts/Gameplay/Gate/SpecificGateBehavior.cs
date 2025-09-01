using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class SpecificGateBehavior : GateBehavior
    {
        [Header("Image Gate Info")]
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text amountText;
        [SerializeField] TMP_Text displayNameText;

        public override void Init(AbstractLevelData data)
        {
            base.Init(data);

            switch (Data.GateType)
            {
                case GateType.GiveCharacter:

                    CharacterData characterData = LevelController.Database.GetCharacterData(Data.ExplicitId);

                    iconImage.sprite = characterData.PreviewSprite;
                    amountText.text = "x" + Data.CharactersAmount.ToString();
                    displayNameText.text = characterData.DisplayName;

                    break;

                case GateType.GiveWeapon:

                    GunData gunData = LevelController.Database.GetGunData(Data.ExplicitId);

                    iconImage.sprite = gunData.PreviewSprite;
                    displayNameText.text = gunData.DisplayName;
                    amountText.text = "";

                    break;
            }
        }

        protected override void Apply(PlayerBehavior player)
        {
            base.Apply(player);

            switch (Data.GateType)
            {
                case GateType.GiveWeapon:
                    GiveGun(player);
                    break;

                case GateType.GiveCharacter:
                    GiveCharacter(player);
                    break;
            }

            PlayGateSound(true);
        }

        public void GiveCharacter(PlayerBehavior player)
        {
            int count = Data.CharactersAmount;
            if(count < 1) count = 1;

            for(int i = 0; i < count; i++)
            {
                CharacterBehavior character = LevelController.SkinsManager.GetCharacter(Data.ExplicitId);
                player.AddCharacter(character);
            }
        }

        public void GiveGun(PlayerBehavior player)
        {
            player.ChangeGun(Data.ExplicitId);
        }
    }
}
