using UnityEngine;
using UnityEngine.UI;
using Watermelon.IAPStore;

namespace Watermelon
{
    public class UIMainMenu : UIPage
    {
        public readonly float STORE_AD_RIGHT_OFFSET_X = 300F;

        [SerializeField] RectTransform safeAreaRectTransform;

        [Space]
        [SerializeField] Button playButton;
        [SerializeField] RectTransform tapToPlayRect;

        [Space]
        [SerializeField] UIScaleAnimation coinsLabelScalable;
        [SerializeField] CurrencyUIPanelSimple coinsPanel;

        [Space]
        [SerializeField] UIMainMenuButton iapStoreButton;
        [SerializeField] UIMainMenuButton noAdsButton;

        [Space]
        [SerializeField] UINoAdsPopUp noAdsPopUp;

        [Space]
        [SerializeField] UIUpgradesPanel upgradesPanel;
        
        private TweenCase tapToPlayPingPong;
        private TweenCase showHideStoreAdButtonDelayTweenCase;

        private void OnEnable()
        {
            AdsManager.ForcedAdDisabled += ForceAdPurchased;
        }

        private void OnDisable()
        {
            AdsManager.ForcedAdDisabled -= ForceAdPurchased;
        }

        public override void Init()
        {
            coinsPanel.Init();

            iapStoreButton.Init(STORE_AD_RIGHT_OFFSET_X);
            noAdsButton.Init(STORE_AD_RIGHT_OFFSET_X);

            iapStoreButton.Button.onClick.AddListener(IAPStoreButton);
            noAdsButton.Button.onClick.AddListener(NoAdButton);
            coinsPanel.AddButton.onClick.AddListener(AddCoinsButton);

            playButton.onClick.AddListener(TapToPlayButton);

            noAdsPopUp.Initialise();

            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);
        }

        // Letting UpgradesController get initialised
        private void Start()
        {
            upgradesPanel.Init();
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            showHideStoreAdButtonDelayTweenCase?.Kill();

            HideAdButton(true);
            iapStoreButton.Hide(true);
            ShowTapToPlay();

            coinsLabelScalable.Show();
            
            UILevelNumberText.Show();

            showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.12f, delegate
            {
                ShowAdButton();
                iapStoreButton.Show();
            });

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            showHideStoreAdButtonDelayTweenCase?.Kill();

            HideTapToPlayText(immediately: true);

            coinsLabelScalable.Hide(immediately: true);
            iapStoreButton.Hide(immediately: true);

            HideAdButton(immediately: true);

            UIController.OnPageClosed(this);
        }

        #endregion

        #region Tap To Play Label

        public void ShowTapToPlay(bool immediately = false)
        {
            if (tapToPlayPingPong != null && tapToPlayPingPong.IsActive)
                tapToPlayPingPong.Kill();

            if (immediately)
            {
                tapToPlayRect.localScale = Vector3.one;

                tapToPlayPingPong = tapToPlayRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

                return;
            }

            // RESET
            tapToPlayRect.localScale = Vector3.zero;

            tapToPlayRect.DOPushScale(Vector3.one * 1.2f, Vector3.one, 0.35f, 0.2f, Ease.Type.CubicOut, Ease.Type.CubicIn).OnComplete(delegate
            {

                tapToPlayPingPong = tapToPlayRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

            });

        }

        public void HideTapToPlayText(bool immediately = false)
        {
            if (tapToPlayPingPong != null && tapToPlayPingPong.IsActive)
                tapToPlayPingPong.Kill();

            if (immediately)
            {
                tapToPlayRect.localScale = Vector3.zero;

                return;
            }

            tapToPlayRect.DOPushScale(Vector3.one * 1.2f, Vector3.zero, 0.2f, 0.35f, Ease.Type.CubicOut, Ease.Type.CubicIn);
        }

        #endregion

        #region Ad Button Label

        private void ShowAdButton(bool immediately = false)
        {
            if (AdsManager.IsForcedAdEnabled())
            {
                noAdsButton.Show(immediately);
            }
            else
            {
                noAdsButton.Hide(immediately: true);
            }
        }

        private void HideAdButton(bool immediately = false)
        {
            if(AdsManager.IsForcedAdEnabled())
            {
                noAdsButton.Hide(immediately);
            }
        }

        private void ForceAdPurchased()
        {
            noAdsButton.Hide(true);
        }

        #endregion

        #region Buttons

        public void TapToPlayButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            GameController.StartLevel();
        }

        public void IAPStoreButton()
        {
            UIController.ShowPage<UIStore>();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        public void NoAdButton()
        {
            noAdsPopUp.Show();
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        public void AddCoinsButton()
        {
            UIController.ShowPage<UIStore>();
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        #endregion
    }


}
