using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class NumericalBoosterBehavior : BoosterBehavior
    {
        [Header("Text")]
        [SerializeField] protected TMP_Text valueText;

        [Header("Visuals")]
        [SerializeField] protected GameObject positiveVisuals;
        [SerializeField] protected GameObject negativeVisuals;

        public OperationType OperationType { get; protected set; }
        public float Value { get; protected set; }

        public bool IsPositive => OperationType == OperationType.Plus || OperationType == OperationType.Multiply;

        public override void Init(AbstractLevelData data)
        {
            BoosterLevelData boosterData = (BoosterLevelData) data;

            if(boosterData == null)
            {
                Debug.LogError("You are trying to init BoosterBehavior with the wrong data!");

                return;
            }

            OperationType = boosterData.OperationType;
            Value = boosterData.NumericalValue;

            base.Init(data);
        }

        protected override void Init()
        {
            base.Init();

            UpdateVisuals();

            UpdateText();
        }

        protected virtual void UpdateVisuals()
        {
            if(positiveVisuals != null) positiveVisuals.SetActive(IsPositive);
            if(negativeVisuals != null) negativeVisuals.SetActive(!IsPositive);
        }

        protected void RevertOperation()
        {
            OperationType = BoosterHelper.RevertOperation(OperationType);
        }

        public virtual string GetValueText()
        {
            return BoosterHelper.GetSymbol(OperationType) + (Mathf.RoundToInt(Value * 10f) / 10f).ToString();
        }

        protected virtual void UpdateText()
        {
            if(valueText != null) valueText.text = GetValueText();
        }

        public float ApplyBoosterToValue(float oldValue)
        {
            return BoosterHelper.ApplyBoosterToValue(OperationType, oldValue, Value);
        }

        protected override void Apply(PlayerBehavior player)
        {
            var modifier = new StatModifier(OperationType, Value);

            switch (BoosterData.BoosterType)
            {
                case BoosterType.FireRate:
                    player.RegisterFireRateModifier(modifier);
                    break;
                case BoosterType.Damage:
                    player.RegisterDamageModifier(modifier);
                    break;
                case BoosterType.Range:
                    player.RegisterRangeModifier(modifier);
                    break;
                case BoosterType.Health:
                    player.AddHealth(modifier);
                    break;
            }
        }
    }
}