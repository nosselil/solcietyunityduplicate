using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace com.whatereyes.gametutorial
{
    // Classe do PopUp para ativação e detecção
    [System.Serializable]
    public class PopUp
    {
        public string popUpName;            // Nome do pop-up (usado para colisão e identificação)
        public PopUpData popUpData;         // Dados do ScriptableObject para o pop-up
        public KeyCode activationKey;       // Tecla para ativar o pop-up (opcional)
        public bool activateOnCollision;    // Se o pop-up será ativado automaticamente ao colidir
        public bool activateAfterTime;      // Se o pop-up será ativado após um tempo
        public float timeToActivate;        // Tempo em segundos para ativar automaticamente
    }

    public class PopUpManager : MonoBehaviour
    {
        public List<PopUp> popUps;          // Lista de pop-ups
        public bool shouldPauseGame = true; // Se o jogo deve ser pausado ao exibir pop-ups
        public bool popUpsEnabled = true;   // Se os pop-ups estão habilitados

        public TextMeshProUGUI titleText;   // Referência ao texto do título na UI
        public Image popUpImage;            // Referência à imagem do pop-up na UI
        public TextMeshProUGUI descriptionText; // Referência ao texto da descrição na UI
        public TextMeshProUGUI categoryText;    // Referência ao texto da categoria na UI
        public GameObject popUpUI;          // Referência à janela de UI do pop-up

        private Dictionary<string, PopUp> popUpDict = new Dictionary<string, PopUp>();
        private Dictionary<string, float> popUpTimers = new Dictionary<string, float>();
        private PopUp currentPopUp;

        void Start()
        {
            // Preencher o dicionário para acesso rápido aos pop-ups pelo nome
            foreach (PopUp popUp in popUps)
            {
                popUpDict.Add(popUp.popUpName, popUp);
                popUpTimers.Add(popUp.popUpName, 0f);  // Inicializar o temporizador para cada pop-up
            }

            popUpUI.SetActive(false);  // Pop-ups começam desativados
        }

        void Update()
        {
            // Atualizar o temporizador para cada pop-up que deve ser ativado após um tempo
            foreach (var popUp in popUps)
            {
                if (popUp.activateAfterTime && !PlayerPrefs.HasKey(popUp.popUpName))
                {
                    popUpTimers[popUp.popUpName] += Time.deltaTime;

                    if (popUpTimers[popUp.popUpName] >= popUp.timeToActivate)
                    {
                        ShowPopUp(popUp);
                    }
                }
            }
        }

        // Função para ativar o pop-up via colisão ou tecla
        public void TriggerPopUp(string colliderName)
        {
            if (popUpsEnabled && popUpDict.ContainsKey(colliderName))
            {
                PopUp popUp = popUpDict[colliderName];

                // Verificar se o pop-up já foi mostrado
                if (PlayerPrefs.HasKey(popUp.popUpName))
                {
                    Debug.Log("Pop-up " + popUp.popUpName + " já foi exibido. Não exibindo novamente.");
                    return; // Se já foi exibido, não fazer nada
                }

                if (popUp.activateOnCollision)
                {
                    // Ativa automaticamente ao colidir
                    ShowPopUp(popUp);
                }
                else if (Input.GetKeyDown(popUp.activationKey))
                {
                    // Ativa ao pressionar a tecla correspondente
                    ShowPopUp(popUp);
                }
            }
        }

        // Função para exibir o pop-up com as informações dinâmicas
        void ShowPopUp(PopUp popUp)
        {
            // Preencher dinamicamente a janela do pop-up
            titleText.text = popUp.popUpData.title;              // Preenche o título
            popUpImage.sprite = popUp.popUpData.popUpImage;      // Preenche a imagem
            descriptionText.text = popUp.popUpData.description;  // Preenche a descrição
            categoryText.text = popUp.popUpData.category;        // Preenche a categoria

            popUpUI.SetActive(true);  // Ativar a UI do pop-up
            currentPopUp = popUp;     // Salva o pop-up atual
            PlayerPrefs.SetInt(popUp.popUpName, 1); // Marcar pop-up como exibido
            PlayerPrefs.Save();  // Certificar que os dados são salvos

            if (shouldPauseGame)
            {
                Time.timeScale = 0; // Pausar o jogo
            }
        }

        // Função para fechar o pop-up e continuar o jogo
        public void ClosePopUp()
        {
            if (currentPopUp != null)
            {
                popUpUI.SetActive(false);  // Desativar a janela de UI do pop-up

                if (shouldPauseGame)
                {
                    Time.timeScale = 1; // Continuar o jogo
                }

                currentPopUp = null;
            }
        }
    }
}