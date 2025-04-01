using UnityEngine;

namespace com.whatereyes.gametutorial
{
    public class PopUpTrigger : MonoBehaviour
    {
        public string popUpName;  // Nome do pop-up associado a este trigger

        // M�todo para acionar o pop-up via PopUpManager
        public void TriggerPopUp()
        {
            PopUpManager popUpManager = FindObjectOfType<PopUpManager>();
            if (popUpManager != null)
            {
                popUpManager.TriggerPopUp(popUpName);
            }
            else
            {
                Debug.LogError("PopUpManager n�o encontrado na cena.");
            }
        }
    }
}