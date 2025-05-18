using UnityEngine;
using UnityEngine.EventSystems;

public class CardActivationZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private Transform activationAnimationTarget; // e.g. center of screen

    public void OnDrop(PointerEventData eventData)
    {
        PowerUpCardDrag card = eventData.pointerDrag?.GetComponent<PowerUpCardDrag>();

        if (card != null)
        {
            Debug.Log("[CardActivationZone] Card dropped on activation zone!");

            // Disable further dragging
            card.DisableInteraction();

            // Begin activation animation and logic
            card.BeginActivationSequence(activationAnimationTarget.position);
        }
    }
}
