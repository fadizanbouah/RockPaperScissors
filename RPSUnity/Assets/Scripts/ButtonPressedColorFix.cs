using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Fixes Unity's Button component staying in "Pressed" color when the pointer
// is held down, dragged outside the button, then released.
// Add this component alongside a Button component on any affected button.
[RequireComponent(typeof(Button))]
public class ButtonPressedColorFix : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private bool _isPointerDown;

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isPointerDown)
        {
            _isPointerDown = false;
            // Deselecting from the EventSystem forces the button's
            // state machine to re-evaluate and transition back to Normal
            EventSystem.current?.SetSelectedGameObject(null);
        }
    }
}
