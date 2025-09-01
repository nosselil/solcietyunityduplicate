using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class ExplicitBoosterBehavior : BoosterBehavior
    {
        [SerializeField] protected TMP_Text amountText;
        [SerializeField] protected TMP_Text displayNameText;

        private CharacterBehavior boosterCharacter;
        protected GunBehavior boostedGun;

        protected override void Init()
        {
            base.Init();

            if (BoosterData.BoosterType == BoosterType.GiveCharacter)
            {
                boosterCollider.enabled = false;
                boosterCharacter = LevelController.SkinsManager.GetCharacter(BoosterData.ExplicitId);
                boosterCharacter.transform.SetParent(transform);
                boosterCharacter.transform.position = transform.position + Vector3.up;
                boosterCharacter.transform.rotation = Quaternion.Euler(0, 180, 0);

                boosterCharacter.DisableCollider();
                boosterCharacter.DisableMovingAnimation();
                boosterCharacter.DisableHealthbar();
                boosterCollider.enabled = true;

                CharacterData characterData = LevelController.Database.GetCharacterData(BoosterData.ExplicitId);
                amountText.text = "x" + BoosterData.CharactersAmount.ToString();
                displayNameText.text = characterData.DisplayName;
            }
            else
            {
                boostedGun = LevelController.SkinsManager.GetGun(BoosterData.ExplicitId);
                boostedGun.transform.position = transform.position + Vector3.up;
                boostedGun.transform.SetParent(transform);

                GunData gunData = LevelController.Database.GetGunData(BoosterData.ExplicitId);

                displayNameText.text = gunData.DisplayName;
                if (amountText != null) amountText.text = "";
            }
        }

        protected override void Apply(PlayerBehavior player)
        {
            if (BoosterData.BoosterType == BoosterType.GiveCharacter)
            {
                ActivateCharacter(player, boosterCharacter);
                boosterCharacter = null;

                for (int i = 0; i < BoosterData.CharactersAmount - 1; i++)
                {
                    CharacterBehavior extraCharacter = LevelController.SkinsManager.GetCharacter(BoosterData.ExplicitId);

                    ActivateCharacter(player, extraCharacter);
                }
            }
            else
            {
                player.ChangeGun(BoosterData.ExplicitId);

                boostedGun.gameObject.SetActive(false);
                boostedGun.transform.SetParent(PoolManager.DefaultContainer);
            }

            boosterCollider.enabled = false;
        }

        private void ActivateCharacter(PlayerBehavior player, CharacterBehavior character)
        {
            player.AddCharacter(character);

            character.EnableCollider();
            character.transform.rotation = Quaternion.identity;
            character.transform.SetParent(PoolManager.DefaultContainer);
        }

        public override void Clear()
        {
            base.Clear();

            if (boosterCharacter != null)
            {
                boosterCharacter.gameObject.SetActive(false);
                boosterCharacter.EnableCollider();
                boosterCharacter.transform.rotation = Quaternion.identity;

                boosterCharacter = null;
            }

            if (boostedGun != null)
            {
                boostedGun.gameObject.SetActive(false);
            }
        }
    }
}