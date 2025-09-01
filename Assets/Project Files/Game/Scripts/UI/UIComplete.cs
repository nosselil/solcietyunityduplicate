using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;
using Watermelon.IAPStore;

namespace Watermelon
{
    public class UIComplete : UIPage
    {
        [SerializeField] RectTransform safeAreaRectTransform;

        [Space]
        [SerializeField] UIFadeAnimation backgroundFade;
        [SerializeField] UIScaleAnimation levelCompleteLabel;

        [Space]
        [SerializeField] UIScaleAnimation rewardLabel;
        [SerializeField] Image rewardCurrencyIcon;
        [SerializeField] TextMeshProUGUI rewardAmountText;

        [Header("Coins Label")]
        [SerializeField] UIScaleAnimation coinsPanelScalable;
        [SerializeField] CurrencyUIPanelSimple coinsPanelUI;

        [Header("Buttons")]
        [SerializeField] UIFadeAnimation multiplyRewardButtonFade;
        [SerializeField] Button multiplyRewardButton;
        [SerializeField] UIFadeAnimation noThanksButtonFade;
        [SerializeField] Button noThanksButton;
        [SerializeField] TMP_Text noThanksButtonText;

        private TweenCase noThanksAppearTween;
        private static int coinsHash = "Money".GetHashCode();

        private readonly string NO_THANKS_TEXT = "CONTINUE";
        private readonly string CONTINUE_TEXT = "CONTINUE";

        private int currentReward;

        public override void Init()
        {
            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            multiplyRewardButton.onClick.AddListener(MultiplyRewardButton);
            noThanksButton.onClick.AddListener(NoThanksButton);

            coinsPanelUI.Init();
        }

        #region Show/Hide
        public override void PlayShowAnimation()
        {
            if (isPageDisplayed)
                return;

            isPageDisplayed = true;
            canvas.enabled = true;

            rewardLabel.Hide(immediately: true);
            multiplyRewardButtonFade.Hide(immediately: true);
            multiplyRewardButton.interactable = false;
            noThanksButtonFade.Hide(immediately: true);
            noThanksButton.interactable = false;
            coinsPanelScalable.Hide(immediately: true);

            noThanksButtonText.text = NO_THANKS_TEXT;

            backgroundFade.Show(duration: 0.3f);
            levelCompleteLabel.Show();

            coinsPanelScalable.Show();

            rewardCurrencyIcon.sprite = CurrencyController.GetCurrency(LevelController.LevelData.RewardCurrency).Icon;
            currentReward = LevelController.LevelData.RewardAmount;

            ShowRewardLabel(currentReward, false, 0.3f, delegate 
            {
                rewardLabel.Transform.DOPushScale(Vector3.one * 1.1f, Vector3.one, 0.2f, 0.2f).OnComplete(delegate
                {
                    FloatingCloud.SpawnCurrency(coinsHash, (RectTransform)rewardLabel.Transform, (RectTransform)coinsPanelScalable.Transform, 10, "", () =>
                    {
                        CurrencyController.Add(CurrencyType.Money, currentReward);

                        multiplyRewardButtonFade.Show();
                        multiplyRewardButton.interactable = true;

                        noThanksAppearTween = Tween.DelayedCall(1.5f, delegate
                        {
                            noThanksButtonFade.Show();
                            noThanksButton.interactable = true;
                        });
                    });
                });
            });
        }

        public override void PlayHideAnimation()
        {
            if (!isPageDisplayed)
                return;

            backgroundFade.Hide(0.25f);
            coinsPanelScalable.Hide();

            Tween.DelayedCall(0.25f, delegate
            {
                canvas.enabled = false;
                isPageDisplayed = false;

                UIController.OnPageClosed(this);
            });
        }


        #endregion

        #region RewardLabel

        public void ShowRewardLabel(float rewardAmounts, bool immediately = false, float duration = 0.3f, Action onComplted = null)
        {
            rewardLabel.Show(immediately: immediately);

            if (immediately)
            {
                rewardAmountText.text = "+" + rewardAmounts;
                onComplted?.Invoke();

                return;
            }

            rewardAmountText.text = "+" + 0;

            Tween.DoFloat(0, rewardAmounts, duration, (float value) =>
            {

                rewardAmountText.text = "+" + (int)value;
            }).OnComplete(delegate
            {

                onComplted?.Invoke();
            });
        }

        #endregion

        #region Buttons

        public void MultiplyRewardButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            if (noThanksAppearTween != null && noThanksAppearTween.IsActive)
            {
                noThanksAppearTween.Kill();
            }

            noThanksButton.interactable = false;

            AdsManager.ShowRewardBasedVideo((bool success) =>
            {
                if (success)
                {
                    int rewardMult = 3;

                    noThanksButtonFade.Hide(immediately: true);
                    multiplyRewardButtonFade.Hide(immediately: true);
                    multiplyRewardButton.interactable = false;

                    ShowRewardLabel(currentReward * rewardMult, false, 0.3f, delegate
                    {
                        FloatingCloud.SpawnCurrency(coinsHash, (RectTransform)rewardLabel.Transform, (RectTransform)coinsPanelScalable.Transform, 10, "", () =>
                        {
                            CurrencyController.Add(CurrencyType.Money, currentReward * rewardMult);

                            noThanksButtonText.text = CONTINUE_TEXT;

                            noThanksButton.interactable = true;
                            noThanksButton.gameObject.SetActive(true);
                            noThanksButtonFade.Show();
                        });
                    });
                }
                else
                {
                    NoThanksButton();
                }
            });
        }

        public void NoThanksButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage<UIComplete>(GameController.ReturnToMainMenu);
        }

        public void HomeButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
        #endregion
    }
}
