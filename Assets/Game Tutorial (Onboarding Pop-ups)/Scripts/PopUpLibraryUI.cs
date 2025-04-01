using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace com.whatereyes.gametutorial
{
    public class PopUpLibraryUI : MonoBehaviour
    {
        public ScrollRect scrollView; // ScrollView para a lista de pop-ups
        public GameObject buttonPrefab; // Prefab para os bot�es da lista de pop-ups
        public Transform content; // O conte�do da ScrollView

        public Image popupImage; // Exibe a imagem do pop-up
        public TextMeshProUGUI popupTitle; // Exibe o t�tulo do pop-up
        public TextMeshProUGUI popupDescription; // Exibe a descri��o do pop-up
        public TextMeshProUGUI popupCategory; // Exibe a categoria do pop-up

        // Lista de todos os pop-ups via ScriptableObject
        public List<PopUpData> popUpList; // Preenchida no Inspector com assets do tipo PopUpData

        void Start()
        {
            LoadPopUps();
        }

        // Carrega todos os pop-ups e cria a lista na UI
        void LoadPopUps()
        {
            foreach (PopUpData popUp in popUpList)
            {
                GameObject newButton = Instantiate(buttonPrefab, content);
                newButton.GetComponentInChildren<TextMeshProUGUI>().text = popUp.title;

                // Adiciona uma fun��o ao bot�o para exibir os detalhes do pop-up
                newButton.GetComponent<Button>().onClick.AddListener(() => DisplayPopUpDetails(popUp));
            }
        }

        // Exibe os detalhes do pop-up selecionado
        void DisplayPopUpDetails(PopUpData popUp)
        {
            popupImage.sprite = popUp.popUpImage;
            popupTitle.text = popUp.title;
            popupDescription.text = popUp.description;
            popupCategory.text = popUp.category;
        }
    }
}