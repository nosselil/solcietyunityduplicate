using UnityEngine;

namespace com.whatereyes.gametutorial
{
    public class PlayerCollisionDetector : MonoBehaviour
    {
        // Detecta colisão com um objeto (Collider sem a opção "Is Trigger" marcada)
        void OnCollisionEnter(Collision collision)
        {
            PopUpTrigger popUpTrigger = collision.gameObject.GetComponent<PopUpTrigger>();

            if (popUpTrigger != null)  // Verifica se o objeto colidido tem o script PopUpTrigger
            {
                popUpTrigger.TriggerPopUp();  // Chama o método TriggerPopUp do PopUpTrigger
            }
        }

        // Detecta entrada em um trigger (Collider com "Is Trigger" marcado)
        void OnTriggerEnter(Collider other)
        {
            PopUpTrigger popUpTrigger = other.GetComponent<PopUpTrigger>();

            if (popUpTrigger != null)  // Verifica se o objeto de trigger tem o script PopUpTrigger
            {
                popUpTrigger.TriggerPopUp();  // Chama o método TriggerPopUp do PopUpTrigger
            }
        }
    }
}