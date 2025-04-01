using UnityEngine;

namespace com.whatereyes.gametutorial
{
    [CreateAssetMenu(fileName = "NewPopUpData", menuName = "PopUp/PopUpData")]
    public class PopUpData : ScriptableObject
    {
        public string title;              // Título do pop-up
        public Sprite popUpImage;         // Imagem do pop-up

        [TextArea]                        // Caixa de texto com área para múltiplas linhas
        public string description;        // Descrição do pop-up

        public string category;           // Categoria do pop-up (se houver)
    }
}