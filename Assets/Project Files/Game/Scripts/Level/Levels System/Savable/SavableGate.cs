using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace Watermelon
{
    [ExecuteInEditMode]
    public class SavableGate : AbstractSavable, IEquatable<SavableGate>
    {
        [SerializeField, Hide] GateType gateType;
        [SerializeField, ShowIf("isNumerical"), OnValueChanged("UpdateGateVisuals")] OperationType operationType;
        [SerializeField, ShowIf("isNumerical"), Min(0), OnValueChanged("UpdateGateVisuals")] float numericalValue = 1;
        [SerializeField, ShowIf("isNumerical"), OnValueChanged("UpdateGateVisuals")] bool updateOnHit = true;
        [SerializeField, ShowIf("isNumerical"), Min(0), OnValueChanged("UpdateGateVisuals")] float step = 0.1f;
        [SerializeField, ShowIf("IsWeapon"), LevelDataPicker(LevelDataType.Weapon), OnValueChanged("UpdateGateVisuals")] string weaponId = "default_gun";
        [SerializeField, ShowIf("IsCharacter"), LevelDataPicker(LevelDataType.Character), OnValueChanged("UpdateGateVisuals")] string characterId = "default_character";
        [SerializeField, ShowIf("IsCharacter"), OnValueChanged("UpdateGateVisuals")] int charactersAmount = 1;

        [SerializeField,HideInInspector] private bool isNumerical;//SerializeField so we can duplicate object
        [HideInInspector] public bool needToUpdateVisuals; // tells EditorSceneController that visuals needs to be updated on next frame. Important for loading level

        public GateType GateType { get => gateType; set => gateType = value; }

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

        public bool UpdateOnHit
        {
            get => updateOnHit; set
            {
                updateOnHit = value;
                needToUpdateVisuals = true;
            }
        }

        public float Step
        {
            get => step; set
            {
                step = value;
                needToUpdateVisuals = true;
            }
        }
       
        public string ExplicitId
        {
            get
            {
                if (IsWeapon())
                {
                    return weaponId;
                }
                else
                {
                    return characterId;
                }
            }

            set
            {
                weaponId = value;
                characterId = value;
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
            return (gateType == GateType.GiveCharacter);
        }

        private bool IsWeapon()
        {
            return (gateType == GateType.GiveWeapon);
        }

        private void Awake() //this method exist to catch duplicates
        {
            needToUpdateVisuals = true; 
        }


        public void UpdateGateVisuals()
        {
#if UNITY_EDITOR
            LevelsDatabase levelsDatabase = EditorUtils.GetAsset<LevelsDatabase>();
            needToUpdateVisuals = false;

            if (isNumerical)
            {
                NumericalGateBehavior numericalGateBehavior = GetComponent<NumericalGateBehavior>();
                numericalGateBehavior.GetType().GetProperty("OperationType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).SetValue(numericalGateBehavior, operationType);
                numericalGateBehavior.GetType().GetProperty("Value", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).SetValue(numericalGateBehavior, numericalValue);
                numericalGateBehavior.GetType().GetProperty("UpdateOnHit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).SetValue(numericalGateBehavior, updateOnHit);
                numericalGateBehavior.GetType().GetProperty("Step", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).SetValue(numericalGateBehavior, step);
                numericalGateBehavior.GetType().GetMethod("UpdateVisuals", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(numericalGateBehavior, new object[] { });
                numericalGateBehavior.GetType().GetMethod("UpdateText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(numericalGateBehavior, new object[] { });

            }
            else
            {
                SpecificGateBehavior specificGateBehavior = GetComponent<SpecificGateBehavior>();

                switch (gateType)
                {
                    case GateType.GiveCharacter:

                        CharacterData characterData = levelsDatabase.GetCharacterData(characterId);

                        if (characterData == null)
                        {
                            return;
                        }

                        ((Image)specificGateBehavior.GetType().GetField("iconImage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(specificGateBehavior)).sprite = characterData.PreviewSprite;
                        ((TMP_Text)specificGateBehavior.GetType().GetField("amountText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(specificGateBehavior)).text = "x" + charactersAmount.ToString();
                        ((TMP_Text)specificGateBehavior.GetType().GetField("displayNameText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(specificGateBehavior)).text = characterData.DisplayName;

                        break;

                    case GateType.GiveWeapon:

                        GunData gunData = levelsDatabase.GetGunData(weaponId);

                        if (gunData == null)
                        {
                            return;
                        }

                        ((Image)specificGateBehavior.GetType().GetField("iconImage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(specificGateBehavior)).sprite = gunData.PreviewSprite;
                        ((TMP_Text)specificGateBehavior.GetType().GetField("displayNameText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(specificGateBehavior)).text = gunData.DisplayName;
                        ((TMP_Text)specificGateBehavior.GetType().GetField("amountText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(specificGateBehavior)).text = string.Empty;

                        break;
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
                ExplicitId =  levelsDatabase.GunsData[0].Id;
            }
            else if (IsCharacter())
            {
                ExplicitId = levelsDatabase.CharactersData[0].Id;
            }
#endif
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SavableGate);
        }

        public bool Equals(SavableGate other)
        {
            return other is not null &&
                   base.Equals(other) &&
                   Id == other.Id &&
                   Position.Equals(other.Position) &&
                   GateType == other.GateType &&
                   OperationType == other.OperationType &&
                   NumericalValue == other.NumericalValue &&
                   UpdateOnHit == other.UpdateOnHit &&
                   Step == other.Step &&
                   ExplicitId == other.ExplicitId &&
                   CharactersAmount == other.CharactersAmount &&
                   IsNumerical == other.IsNumerical;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Id);
            hash.Add(Position);
            hash.Add(GateType);
            hash.Add(OperationType);
            hash.Add(NumericalValue);
            hash.Add(UpdateOnHit);
            hash.Add(Step);
            hash.Add(ExplicitId);
            hash.Add(CharactersAmount);
            hash.Add(IsNumerical);
            return hash.ToHashCode();
        }
    }
}
