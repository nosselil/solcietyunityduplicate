using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class NumericalGateBehavior : GateBehavior
    {
        [Header("Text")]
        [SerializeField] protected TMP_Text valueText;

        [Header("Images")]
        [SerializeField] Image backImage;
        [SerializeField] Image titleBackImage;

        [Header("Visuals")]
        [SerializeField] protected GameObject positiveVisuals;
        [SerializeField] protected GameObject negativeVisuals;

        [Header("Colors")]
        [SerializeField] Color positiveBackColor;
        [SerializeField] Color negativeBackColor;
        [SerializeField] Color positiveTitleColor;
        [SerializeField] Color negativeTitleColor;

        public OperationType OperationType { get; protected set; }
        public float Value { get; protected set; }

        public bool UpdateOnHit { get; private set; }
        public float Step { get; private set; }

        public bool IsPositive => OperationType == OperationType.Plus || OperationType == OperationType.Multiply;

        public override void Init(AbstractLevelData data)
        {
            base.Init(data);

            //StatType = gateData.StatType;
            OperationType = Data.OperationType;
            Value = Data.NumericalValue;

            UpdateOnHit = Data.UpdateOnHit;
            Step = Data.Step;

            UpdateVisuals();
            UpdateText();
            UpdateValue(0);
        }

        protected override void PlayParticle()
        {
            if (IsPositive)
            {
                base.PlayParticle();
            }
        }

        protected override void Apply(PlayerBehavior player)
        {
            base.Apply(player);

            StatModifier modifier = new StatModifier(OperationType, Value);

            switch (Data.GateType)
            {
                case GateType.FireRate:
                    player.RegisterFireRateModifier(modifier);
                    break;

                case GateType.Money:
                    CurrencyController.Add(CurrencyType.Money, Mathf.RoundToInt(Value));
                    break;

                case GateType.Range:
                    player.RegisterRangeModifier(modifier);
                    break;

                case GateType.Damage:
                    player.RegisterDamageModifier(modifier);
                    break;

                case GateType.Health:
                    player.AddHealth(modifier);
                    break;
            }

            PlayGateSound(Data.GateType == GateType.GiveWeapon || Data.GateType == GateType.GiveCharacter || Value >= 0);
        }

        protected void UpdateVisuals()
        {
            positiveVisuals.SetActive(IsPositive);
            negativeVisuals.SetActive(!IsPositive);

            backImage.color = IsPositive ? positiveBackColor : negativeBackColor;
            if(titleBackImage != null) titleBackImage.color = IsPositive ? positiveTitleColor : negativeTitleColor;
        }

        public virtual string GetValueText()
        {
            return BoosterHelper.GetSymbol(OperationType) + (Mathf.RoundToInt(Value * 10f) / 10f).ToString();
        }

        protected virtual void UpdateText()
        {
            if (valueText != null) valueText.text = GetValueText();
        }

        public void UpdateValue(float step)
        {
            if (OperationType == OperationType.Divide || OperationType == OperationType.Minus)
            {
                Value -= step;
            }
            else
            {
                Value += step;
            }

            if (OperationType == OperationType.Minus && Value <= 0 || OperationType == OperationType.Divide && Value < 1)
            {
                Value = step;

                OperationType = BoosterHelper.RevertOperation(OperationType);
                UpdateVisuals();
            }

            if ((OperationType == OperationType.Divide || OperationType == OperationType.Multiply) && Value < 1)
            {
                Value = 1;
            }

            UpdateText();
        }

        public override void GetHit(float damage, Vector3 hitPoint, float gunModifier)
        {
            base.GetHit(damage, hitPoint, gunModifier);
            if (!UpdateOnHit) return;
            UpdateValue(Step * gunModifier);
        }
    }
}
