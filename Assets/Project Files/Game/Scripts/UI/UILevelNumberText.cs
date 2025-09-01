using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UILevelNumberText : MonoBehaviour
    {
        private static readonly string LEVEL_NUMBER_SAVE_NAME = "Level Number Save";

        private const string LEVEL_LABEL = "LEVEL {0}";
        private static UILevelNumberText instance;

        [SerializeField] UIScaleAnimation uIScalableObject;

        private static UIScaleAnimation UIScalableObject => instance.uIScalableObject;
        private static TextMeshProUGUI levelNumberText;

        private static bool IsDisplayed = false;

        private static LevelNumberSave save;

        private void Awake()
        {
            instance = this;
            levelNumberText = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            save = SaveController.GetSaveObject<LevelNumberSave>(LEVEL_NUMBER_SAVE_NAME);
            UpdateLevelNumber();
        }

        private void OnEnable()
        {
            GameController.OnLevelChangedEvent += UpdateLevelNumber;
        }

        private void OnDisable()
        {
            GameController.OnLevelChangedEvent -= UpdateLevelNumber;
        }

        public static void Show(bool immediately = false)
        {
            if (IsDisplayed)
                return;

            IsDisplayed = true;

            levelNumberText.enabled = true;
            UIScalableObject.Show(scaleMultiplier: 1.05f, immediately: immediately);
        }

        public static void Hide(bool immediately = false)
        {
            if (!IsDisplayed)
                return;

            if (immediately)
                IsDisplayed = false;

            UIScalableObject.Hide(scaleMultiplier: 1.05f, immediately: immediately, onCompleted: delegate
            {
                IsDisplayed = false;
                levelNumberText.enabled = false;
            });
        }

        private void UpdateLevelNumber()
        {
            levelNumberText.text = string.Format(LEVEL_LABEL, save.LevelNumber + 1);
        }

    }
}
