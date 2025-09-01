using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon
{
    [ExecuteInEditMode]
    public class SavableBooster : AbstractSavable, IEquatable<SavableBooster>
    {
        [SerializeField, Hide] BoosterType boosterType;
        [SerializeField, Space, ShowIf("isNumerical"), OnValueChanged("UpdateBoosterVisuals")] OperationType operationType;
        [SerializeField, ShowIf("isNumerical"), OnValueChanged("UpdateBoosterVisuals")] float numericalValue = 1f;
        [SerializeField, ShowIf("IsCharacter"), LevelDataPicker(LevelDataType.Character), OnValueChanged("UpdateBoosterVisuals")] string characterID;
        [SerializeField, ShowIf("IsWeapon"), LevelDataPicker(LevelDataType.Weapon), OnValueChanged("UpdateBoosterVisuals")] string weaponID;
        [SerializeField, ShowIf("IsCharacter"), OnValueChanged("UpdateBoosterVisuals")] int charactersAmount = 1;

        [SerializeField, HideInInspector] private bool isNumerical; //SerializeField so we can duplicate object
        [HideInInspector] public bool needToUpdateVisuals; // tells EditorSceneController that visuals needs to be updated on next frame. Important for loading level
        private GameObject gun;
        private GameObject character;

        public BoosterType BoosterType { get => boosterType; set => boosterType = value; }

        public OperationType OperationType
        {
            get => operationType; set
            {
                operationType = value;
                needToUpdateVisuals = true;
            }
        }

        public float NumericalValue
        {
            get => numericalValue; set
            {
                this.numericalValue = value;
                needToUpdateVisuals = true;
            }
        }

        public string CharacterID
        {
            get => characterID; set
            {
                characterID = value;
                needToUpdateVisuals = true;
            }
        }

        public string WeaponID
        {
            get => weaponID; set
            {
                weaponID = value;
                needToUpdateVisuals = true;
            }
        }

        public int CharactersAmount
        {
            get => charactersAmount; set
            {
                charactersAmount = value;
                needToUpdateVisuals = true;
            }
        }

        public bool IsNumerical { get => isNumerical; set => isNumerical = value; }

        private bool IsCharacter()
        {
            return (boosterType == BoosterType.GiveCharacter);
        }

        private bool IsWeapon()
        {
            return (boosterType == BoosterType.GiveWeapon);
        }

        private void Awake() //this method exist to catch duplicates
        {
            needToUpdateVisuals = true;
        }

        public void UpdateBoosterVisuals()
        {

#if UNITY_EDITOR
            LevelsDatabase levelsDatabase = EditorUtils.GetAsset<LevelsDatabase>();
            needToUpdateVisuals = false;

            if (isNumerical)
            {
                NumericalBoosterBehavior numericalBoosterBehavior = GetComponent<NumericalBoosterBehavior>();
                numericalBoosterBehavior.GetType().GetProperty("OperationType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).SetValue(numericalBoosterBehavior, operationType);
                numericalBoosterBehavior.GetType().GetProperty("Value", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).SetValue(numericalBoosterBehavior, numericalValue);
                numericalBoosterBehavior.GetType().GetMethod("UpdateVisuals", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(numericalBoosterBehavior, new object[] { });
                numericalBoosterBehavior.GetType().GetMethod("UpdateText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(numericalBoosterBehavior, new object[] { });
            }
            else
            {
                ExplicitBoosterBehavior explicitBoosterBehavior = GetComponent<ExplicitBoosterBehavior>();

                if (boosterType == BoosterType.GiveCharacter)
                {
                    CharacterData characterData = levelsDatabase.GetCharacterData(characterID);

                    if (characterData == null)
                    {
                        return;
                    }

                    if (character != null)
                    {
                        DestroyImmediate(character);
                    }

                    character = EditorSceneController.Instance.SpawnPrefab(characterData.Prefab, transform.position + Vector3.up, true, false);
                    character.SetParent(transform);
                    character.transform.rotation = Quaternion.Euler(0, 180, 0);

                    ((TMP_Text)explicitBoosterBehavior.GetType().GetField("amountText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(explicitBoosterBehavior)).text = "x" + charactersAmount.ToString();
                    ((TMP_Text)explicitBoosterBehavior.GetType().GetField("displayNameText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(explicitBoosterBehavior)).text = characterData.DisplayName;
                }
                else if (boosterType == BoosterType.GiveWeapon)
                {
                    GunData gunData = levelsDatabase.GetGunData(weaponID);

                    if (gunData == null)
                    {
                        return;
                    }

                    if (gun != null)
                    {
                        DestroyImmediate(gun);
                    }

                    gun = EditorSceneController.Instance.SpawnPrefab(gunData.Prefab, transform.position + Vector3.up, true, false);
                    gun.SetParent(transform);


                    ((TMP_Text)explicitBoosterBehavior.GetType().GetField("displayNameText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(explicitBoosterBehavior)).text = gunData.DisplayName;
                    TMP_Text amountText = ((TMP_Text)explicitBoosterBehavior.GetType().GetField("amountText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(explicitBoosterBehavior));

                    if (amountText != null)
                    {
                        amountText.text = string.Empty;
                    }
                }

            }
#endif
        }

        public void SetDefaultID()
        {
#if UNITY_EDITOR
            LevelsDatabase levelsDatabase = EditorUtils.GetAsset<LevelsDatabase>();

            if (IsWeapon())
            {
                WeaponID = levelsDatabase.GunsData[0].Id;
            }
            else if (IsCharacter())
            {
                CharacterID = levelsDatabase.CharactersData[0].Id;
            }
#endif
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SavableBooster);
        }

        public bool Equals(SavableBooster other)
        {
            return other is not null &&
                   base.Equals(other) &&
                   Id == other.Id &&
                   Position.Equals(other.Position) &&
                   BoosterType == other.BoosterType &&
                   OperationType == other.OperationType &&
                   NumericalValue == other.NumericalValue &&
                   CharacterID == other.CharacterID &&
                   WeaponID == other.WeaponID &&
                   CharactersAmount == other.CharactersAmount &&
                   IsNumerical == other.IsNumerical;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Id);
            hash.Add(Position);
            hash.Add(BoosterType);
            hash.Add(OperationType);
            hash.Add(NumericalValue);
            hash.Add(CharacterID);
            hash.Add(WeaponID);
            hash.Add(CharactersAmount);
            hash.Add(IsNumerical);
            return hash.ToHashCode();
        }
    }
}
