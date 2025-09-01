using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [SerializeField] RectTransform safeAreaRectTransform;
        [SerializeField] CurrencyUIPanelSimple coinsPanel;

        [Space]
        [SerializeField] InputHandler inputHandler;

        public InputHandler InputHandler => inputHandler;

        public override void Init()
        {
            coinsPanel.Init();

            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);
        }

        public override void PlayHideAnimation()
        {
            coinsPanel.Disable();

            UILevelNumberText.Hide();

            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            coinsPanel.Activate();

            UILevelNumberText.Show();

            UIController.OnPageOpened(this);
        }
    }
}
