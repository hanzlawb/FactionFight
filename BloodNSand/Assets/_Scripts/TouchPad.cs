using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform touchpadArea; // Reference to the RectTransform of the touchpad area
    public float sensitivity = 1.0f;

    private bool isTouching = false;
    private Vector2 lastTouchPosition;
    private Vector2 touchDeltaPosition;

    public Vector2 GetTouchDeltaPosition()
    {
        return touchDeltaPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Check if the pointer is over the touchpad area
        if (RectTransformUtility.RectangleContainsScreenPoint(touchpadArea, eventData.position, Camera.main))
        {
            isTouching = true;
            lastTouchPosition = eventData.position;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouching = false;
        touchDeltaPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isTouching)
        {
            Vector2 currentTouchPosition = eventData.position;

            // Calculate the touch delta only if the pointer is over the touchpad area
            if (RectTransformUtility.RectangleContainsScreenPoint(touchpadArea, currentTouchPosition, Camera.main))
            {
                touchDeltaPosition = (currentTouchPosition - lastTouchPosition) * sensitivity;
                lastTouchPosition = currentTouchPosition;
            }
        }
    }
}
