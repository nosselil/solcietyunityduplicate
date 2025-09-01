using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.Upgrades;

namespace Watermelon
{
    public class UIUpgradeItem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text valueText;
        [SerializeField] TMP_Text costText;

        [Space]
        [SerializeField] Image upgradeIcon;
        [SerializeField] Image currencyIcon;
        [SerializeField] Image buttonImage;

        [Space]
        [SerializeField] Button upgradeButton;

        [Header("Button Sprites")]
        [SerializeField] Sprite interactableButtonSprite;
        [SerializeField] Sprite disabledButtonSprite;

        public FloatUpgrade Upgrade { get; private set; }

        public void Init(UpgradeType upgradeType)
        {
            Upgrade = UpgradesController.GetUpgrade<FloatUpgrade>(upgradeType);

            if (Upgrade.NextStage == null)
            {
                gameObject.SetActive(false);
                return;
            }

            RefreshVisuals();

            Upgrade.OnUpgraded += OnUpgradeUpgraded;
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            CurrencyController.SubscribeGlobalCallback(OnCurrencyChanged);
        }

        private void OnDestroy()
        {
            if(Upgrade != null)
            {
                Upgrade.OnUpgraded -= OnUpgradeUpgraded;
            }

            CurrencyController.UnsubscribeGlobalCallback(OnCurrencyChanged);
        }

        private void RefreshVisuals()
        {
            string title = Upgrade.Title;
            titleText.text = title;

            int price = Upgrade.NextStage.Price;
            costText.text = price.ToString();

            float value = Upgrade.GetCurrentStage().Value;
            float roundedValue = Mathf.RoundToInt(value * 100) / 100f;
            valueText.text = roundedValue.ToString();

            Sprite previewSprite = Upgrade.CurrentStage.PreviewSprite;
            upgradeIcon.sprite = previewSprite;

            CurrencyType currencyType = Upgrade.CurrentStage.CurrencyType;
            Sprite currencyicon = CurrencyController.GetCurrency(currencyType).Icon;
            currencyIcon.sprite = currencyicon;

            bool hasEnoughMoney = CurrencyController.HasAmount(currencyType, price);
            upgradeButton.enabled = hasEnoughMoney;

            Sprite buttonSprite = hasEnoughMoney ? interactableButtonSprite : disabledButtonSprite;
            buttonImage.sprite = buttonSprite;
        }

        private void OnUpgradeButtonClicked()
        {
            int price = Upgrade.NextStage.Price;
            CurrencyType currencyType = Upgrade.CurrentStage.CurrencyType;

            bool hasEnoughMoney = CurrencyController.HasAmount(currencyType, price);

            if (hasEnoughMoney)
            {
                CurrencyController.Substract(currencyType, price);

                Upgrade.UpgradeStage();
            }

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        private void OnUpgradeUpgraded()
        {
            if (Upgrade.NextStage == null)
            {
                gameObject.SetActive(false);

                Upgrade.OnUpgraded -= OnUpgradeUpgraded;
                upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
                CurrencyController.UnsubscribeGlobalCallback(OnCurrencyChanged);

                return;
            }

            RefreshVisuals();
        }

        private void OnCurrencyChanged(Currency currency, int difference) 
        {
            RefreshVisuals();
        }
    }
}
