using UnityEngine;

namespace com.whatereyes.gametutorial
{
    [CreateAssetMenu(fileName = "NewPopUpData", menuName = "PopUp/PopUpData")]
    public class PopUpData : ScriptableObject
    {
        public string title;              // T�tulo do pop-up
        public Sprite popUpImage;         // Imagem do pop-up

        [TextArea]                        // Caixa de texto com �rea para m�ltiplas linhas
        public string description;        // Descri��o do pop-up

        public string category;           // Categoria do pop-up (se houver)
    }
}