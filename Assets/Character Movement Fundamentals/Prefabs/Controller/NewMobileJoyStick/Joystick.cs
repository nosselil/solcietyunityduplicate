using UnityEngine;
using UnityEngine.EventSystems;
public class Joystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform joystickBase;  // The outer background
    public RectTransform joystickHandle; // The moving knob

    private Vector2 inputVector = Vector2.zero;

    public float deadZoneThreshold = 0.2f;  // Ignore tiny movements

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData); // Start dragging immediately
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 dragPosition = eventData.position - (Vector2)joystickBase.position;
        float radius = joystickBase.sizeDelta.x / 2f; // Get the joystick radius

        // Normalize the movement
        inputVector = dragPosition.magnitude > radius ? dragPosition.normalized : dragPosition / radius;

        // Apply dead zone
        if (inputVector.magnitude < deadZoneThreshold)
            inputVector = Vector2.zero;

        // Move the handle
        joystickHandle.anchoredPosition = inputVector * (radius * 0.6f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        joystickHandle.anchoredPosition = Vector2.zero; // Reset handle position
    }

    public float GetHorizontal()
    {
        return inputVector.x;
    }

    public float GetVertical()
    {
        return inputVector.y;
    }
}
