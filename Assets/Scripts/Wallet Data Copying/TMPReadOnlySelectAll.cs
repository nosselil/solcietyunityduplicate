using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class TMPReadOnlySelectAll : MonoBehaviour, IPointerClickHandler
{
    TMP_InputField inputField;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.readOnly = true;
        inputField.interactable = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        inputField.ActivateInputField();
        inputField.selectionAnchorPosition = 0;
        inputField.selectionFocusPosition = inputField.text.Length;
    }
}
