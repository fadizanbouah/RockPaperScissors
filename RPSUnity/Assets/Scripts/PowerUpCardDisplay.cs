using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class PowerUpCardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private GameObject priceTagObject; // The price tag icon GameObject
    [SerializeField] private bool isPassiveCard = false;
    [SerializeField] private Animator floatingAnimator;
    [SerializeField] private GameObject shadowObject; // The shadow GameObject

    [Header("Sell Mode")]
    private bool isSellMode = false;
    private SellTabManager sellTabManager;

    [Header("Sold Out Animation")]
    [SerializeField] private GameObject soldOutObject; // The "SOLD OUT" GameObject with animation
    [SerializeField] private Animator soldOutAnimator; // Optional: if you need direct animator control

    [Header("Panel Floating Animation")]
    [SerializeField] private float floatingStartDelay = 1f; // Optional delay before starting

    [Header("Price Tag Dimming")]
    [SerializeField] private Image priceTagImage; // The Image component on the price tag
    [SerializeField] private float priceTagFadeDuration = 0.2f; // Match button's fade duration
    private Color priceTagOriginalColor;

    private PowerUpData data;
    private PowerUpPanelManager panelManager; // only needed in PowerUpPanel context
    private bool isGameplayCard = false;
    private int displayLevel = 0; // Track what level this card is showing

    private Vector3 originalLocalPosition; // Stores the fan layout position
    private Quaternion originalRotation;   // Stores the fan layout rotation

    public void SetData(PowerUpData newData, int currentFavor, PowerUpPanelManager manager = null, bool isGameplay = false)
    {
        data = newData;
        panelManager = manager;
        isGameplayCard = isGameplay;
        // Determine display level for upgradeable power-ups
        displayLevel = 0;
        if (data.isUpgradeable && RunProgressManager.Instance != null)
        {
            if (RunProgressManager.Instance.HasPowerUp(data))
            {
                displayLevel = RunProgressManager.Instance.GetPowerUpLevel(data) + 1;
                // Check if already at max level
                if (data.IsMaxLevel(displayLevel - 1))
                {
                    // Hide this card - it's maxed out
                    gameObject.SetActive(false);
                    return;
                }
            }
        }
        if (backgroundImage != null && data.icon != null)
            backgroundImage.sprite = data.icon;
        if (nameText != null)
        {
            nameText.text = data.powerUpName + data.GetLevelSuffix(displayLevel);
        }
        if (descriptionText != null)
        {
            descriptionText.text = data.GetDescriptionForLevel(displayLevel);
        }
        if (!isPassiveCard)
        {
            UpdateAffordability(currentFavor);
            // Hide price tag during gameplay
            if (priceTagObject != null)
            {
                priceTagObject.SetActive(!isGameplayCard);
            }
            // Hide shadow during gameplay
            if (shadowObject != null)
            {
                shadowObject.SetActive(!isGameplayCard);
            }
            // Store original price tag color
            if (priceTagImage != null && priceTagOriginalColor == default(Color))
            {
                priceTagOriginalColor = priceTagImage.color;
            }
        }
        else if (costText != null)
        {
            costText.text = "";
            costText.gameObject.SetActive(false);
            // Also hide price tag for passive cards
            if (priceTagObject != null)
            {
                priceTagObject.SetActive(false);
            }
            // Hide shadow for passive cards during gameplay
            if (shadowObject != null)
            {
                shadowObject.SetActive(!isGameplayCard);
            }
        }
        // Start floating animation when in panel (not gameplay)
        if (!isGameplay)
        {
            StartFloatingAnimation();
        }
    }

    public void UpdateAffordability(int currentFavor)
    {
        if (costText != null && data != null)
        {
            if (isGameplayCard)
            {
                costText.text = "";
                costText.gameObject.SetActive(false);
            }
            else
            {
                costText.gameObject.SetActive(true);

                if (data.favorCost > 0)
                {
                    costText.text = $"{data.favorCost}";
                    costText.color = currentFavor >= data.favorCost ? Color.white : Color.red;
                }
                else
                {
                    costText.text = "";
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Button button = GetComponent<Button>();
        if (button != null && !button.interactable) return; // Don't hover if not interactable

        if (isGameplayCard)
        {
            if (Mathf.Approximately(originalLocalPosition.y, 0f))
                originalLocalPosition = transform.localPosition;

            transform.localPosition = originalLocalPosition + Vector3.up * 20f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isGameplayCard)
        {
            transform.localPosition = originalLocalPosition;
        }
    }

    public void OnCardClicked()
    {
        if (isGameplayCard)
        {
            Debug.Log("[PowerUpCardDisplay] Card is in gameplay, ignoring click.");
            return;
        }
        if (isSellMode)
        {
            // Sell immediately on click
            if (sellTabManager != null && data != null)
            {
                sellTabManager.SellCard(data);
            }
            return;
        }
        if (data == null) return;
        int currentFavor = RunProgressManager.Instance.currentFavor;
        if (isPassiveCard || currentFavor >= data.favorCost)
        {
            if (!isPassiveCard)
                RunProgressManager.Instance.favor -= data.favorCost;
            Debug.Log($"[PowerUpCardDisplay] Purchased {data.powerUpName} for {data.favorCost} Favor!");
            if (data.isPassive)
            {
                RunProgressManager.Instance.ApplyPowerUpEffect(data);
                Debug.Log($"[PowerUpCardDisplay] Applied passive power-up: {data.powerUpName}");
                if (panelManager != null)
                {
                    panelManager.LockOutOtherPassiveChoices(this);
                    panelManager.DisableOtherPassiveCards(this);
                }
            }
            else
            {
                // Active power-up logic
                RunProgressManager.Instance.AddAcquiredPowerUp(data);

                // Instead of hiding the card, play the sold out animation
                PlaySoldOutAnimation();
                Debug.Log($"[PowerUpCardDisplay] Active power-up purchased, showing SOLD OUT: {data.powerUpName}");
            }
            if (panelManager != null)
            {
                panelManager.RefreshFavorDisplay();
                panelManager.RefreshCardAffordability();
            }
        }
        else
        {
            Debug.Log($"[PowerUpCardDisplay] Not enough favor to buy {data.powerUpName}!");
        }
    }

    public void ResetToFanPosition()
    {
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalRotation;
    }

    public void ResetHoverPosition()
    {
        transform.localPosition = originalLocalPosition;
    }

    public bool IsGameplayCard()
    {
        return isGameplayCard;
    }

    public void StoreFanLayoutState(Vector3 position, Quaternion rotation)
    {
        originalLocalPosition = position;
        originalRotation = rotation;
    }

    public PowerUpData GetPowerUpData()
    {
        return data;
    }

    public void PlayCheckmarkAnimation()
    {
        Transform container = transform.Find("AnimatedContainer");
        Transform checkmark = container?.Find("Checkmark");

        if (container != null && checkmark != null)
        {
            checkmark.gameObject.SetActive(true); // Enable the checkmark so it�s visible

            Animator anim = container.GetComponent<Animator>(); // Corrected: Animator is on AnimatedContainer

            if (anim != null)
            {
                anim.SetTrigger("CheckmarkPopIn"); // This triggers the Idle -> CheckmarkPopIn transition
            }
            else
            {
                Debug.LogWarning("[PowerUpCardDisplay] AnimatedContainer is missing an Animator!");
            }
        }
        else
        {
            Debug.LogWarning("[PowerUpCardDisplay] AnimatedContainer or Checkmark not found!");
        }
    }

    public void SetUsableState(bool canUse)
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            // Visual feedback for unusable state
            group.alpha = canUse ? 1.0f : 0.5f;
        }

        // Optional: Add a "Used this round" overlay
        Transform usedOverlay = transform.Find("UsedThisRoundOverlay");
        if (usedOverlay != null)
        {
            usedOverlay.gameObject.SetActive(!canUse);
        }
    }

    // Call this when updating card states
    public void UpdateCardState()
    {
        bool canUse = PowerUpUsageTracker.Instance == null || PowerUpUsageTracker.Instance.CanUsePowerUp();
        SetUsableState(canUse);

        // Also update draggability
        PowerUpCardDrag drag = GetComponent<PowerUpCardDrag>();
        if (drag != null && drag.isDraggable)
        {
            CanvasGroup group = GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.interactable = canUse;
                group.blocksRaycasts = canUse;
            }
        }
    }

    public Vector3 GetStoredFanPosition()
    {
        return originalLocalPosition;
    }

    public void PlaySoldOutAnimation()
    {
        if (soldOutObject != null)
        {
            soldOutObject.SetActive(true);

            if (soldOutAnimator == null && soldOutObject != null)
            {
                soldOutAnimator = soldOutObject.GetComponent<Animator>();
            }

            if (soldOutAnimator != null)
            {
                soldOutAnimator.SetTrigger("SoldOut");
            }
        }
        else
        {
            Debug.LogWarning("[PowerUpCardDisplay] SoldOut object not assigned!");
        }

        // Disable the button to apply disabled color
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.interactable = false;

            // Fade the price tag to match button's disabled color
            if (priceTagImage != null)
            {
                ColorBlock colors = button.colors;
                Color targetColor = priceTagOriginalColor * colors.disabledColor;
                StartCoroutine(FadePriceTag(targetColor));
            }
        }
    }

    private IEnumerator FadePriceTag(Color targetColor)
    {
        Color startColor = priceTagImage.color;
        float elapsed = 0f;

        while (elapsed < priceTagFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / priceTagFadeDuration;
            priceTagImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        priceTagImage.color = targetColor;
    }

    // Add this method to trigger the floating animation:
    private void StartFloatingAnimation()
    {
        // Only float in the power-up panel, not during gameplay
        if (isGameplayCard) return;

        if (floatingAnimator != null)
        {
            // Start the floating animation at a random offset
            float randomOffset = Random.Range(0f, floatingStartDelay); // 0-100% through the animation cycle
            floatingAnimator.Play("PowerUpCard_Floating", 0, randomOffset);
        }
    }

    private void OnEnable()
    {
        // Only restart animation if we're already set up and in panel mode
        // The isGameplayCard check alone isn't enough because OnEnable might be called
        // before SetData, so we also check if data is already assigned
        if (data != null && !isGameplayCard && floatingAnimator != null)
        {
            // Small delay to ensure everything is set up
            StartCoroutine(RestartFloatingAnimationDelayed());
        }
    }

    private IEnumerator RestartFloatingAnimationDelayed()
    {
        // Wait one frame to ensure the GameObject is fully active
        yield return null;

        /// Restart the floating animation with a new random offset
        float randomOffset = Random.Range(0f, 1f);
        floatingAnimator.Play("PowerUpCard_Floating", 0, randomOffset);
        floatingAnimator.speed = Random.Range(0.9f, 1.1f);
    }

    public void DisableAllInteractions()
    {
        // DON'T disable button.interactable to preserve visual appearance
        // Just prevent the button from receiving clicks
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners(); // Remove click functionality
        }

        // Disable EventTrigger to prevent hover effects
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            eventTrigger.enabled = false;
        }

        // Disable raycast blocking to prevent any interaction
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        // Ensure we're not in hover state
        if (isGameplayCard && originalLocalPosition != Vector3.zero)
        {
            transform.localPosition = originalLocalPosition;
        }
    }

    public void SetDataForSellMode(PowerUpData powerUpData, SellTabManager sellManager)
    {
        // Use regular SetData
        SetData(powerUpData, 0, null, false);

        isSellMode = true;
        sellTabManager = sellManager;

        // Show sell value instead of buy cost
        if (costText != null)
        {
            costText.gameObject.SetActive(true);
            costText.text = powerUpData.sellValue.ToString();
            costText.color = Color.green; // Green for sell value
        }

        // Show price tag
        if (priceTagObject != null)
        {
            priceTagObject.SetActive(true);
        }

        // Hide SOLD OUT if present
        if (soldOutObject != null)
        {
            soldOutObject.SetActive(false);
        }
    }
}
