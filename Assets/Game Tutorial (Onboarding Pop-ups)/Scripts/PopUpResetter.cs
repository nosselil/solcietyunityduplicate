using UnityEngine;
using UnityEngine.UI;

namespace com.whatereyes.gametutorial
{
    public class PopUpResetter : MonoBehaviour
    {
        public Button resetButton;         // Bot�o na UI para resetar os pop-ups
        public PopUpManager popUpManager;  // Refer�ncia ao PopUpManager

        void Start()
        {
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(ResetPopUps);  // Adiciona fun��o ao bot�o
            }
        }

        // M�todo para resetar apenas os dados dos pop-ups no PlayerPrefs
        public void ResetPopUps()
        {
            // Iterar sobre todos os pop-ups no PopUpManager
            foreach (PopUp popUp in popUpManager.popUps)
            {
                // Remover apenas os dados relacionados a cada pop-up
                if (PlayerPrefs.HasKey(popUp.popUpName))
                {
                    PlayerPrefs.DeleteKey(popUp.popUpName);
                }
            }

            PlayerPrefs.Save();  // Salva as altera��es no PlayerPrefs

            Debug.Log("Os dados dos pop-ups foram resetados.");
        }
    }
}