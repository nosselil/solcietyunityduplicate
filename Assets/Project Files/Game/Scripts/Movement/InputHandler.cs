using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    public class InputHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] float dragSensitivity = 1f;

        private Vector2 pointerPosition;

        public bool IsPointerDown { get; private set; }
        public Vector2 DragDirection { get; private set; }

        public event SimpleVector2Callback OnPointerDragged;

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPointerDown = true;

            pointerPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Normalize drag distance based on screen dimensions
            float normalizedDragX = eventData.delta.x / Screen.width;
            float normalizedDragY = eventData.delta.y / Screen.height;

            // Calculate drag direction
            DragDirection = new Vector2(normalizedDragX, normalizedDragY) * dragSensitivity;

            OnPointerDragged?.Invoke(DragDirection);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPointerDown = false;
        }
    }
}