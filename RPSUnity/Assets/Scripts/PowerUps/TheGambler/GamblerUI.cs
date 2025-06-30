using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamblerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider betSlider;
    [SerializeField] private TextMeshProUGUI betAmountText;
    [SerializeField] private TextMeshProUGUI bonusDamageText;
    [SerializeField] private CanvasGroup canvasGroup;

    private GamblerEffect gamblerEffect;
    private HandController player;
    private HealthBar playerHealthBar;

    public void Initialize(GamblerEffect effect, HandController playerController)
    {
        gamblerEffect = effect;
        player = playerController;

        // Find the player's health bar
        if (player != null)
        {
            playerHealthBar = player.healthBar;
        }

        // Set up slider
        if (betSlider != null)
        {
            betSlider.minValue = 0;
            betSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void Update()
    {
        if (gamblerEffect == null || player == null) return;

        // Don't update the slider during combat - the bet is locked in
        bool isInCombat = RockPaperScissorsGame.Instance != null &&
                          RockPaperScissorsGame.Instance.IsInCombat();

        if (!isInCombat)
        {
            // Only update slider max value when NOT in combat
            int maxBet = gamblerEffect.GetMaxBet();
            if (betSlider != null && betSlider.maxValue != maxBet)
            {
                betSlider.maxValue = maxBet;
                // Don't clamp or change the current value - let the player's choice stand
            }
        }

        // Disable interaction during combat
        SetInteractable(!isInCombat);
    }

    private void OnSliderValueChanged(float value)
    {
        // Don't process changes during combat
        bool isInCombat = RockPaperScissorsGame.Instance != null &&
                          RockPaperScissorsGame.Instance.IsInCombat();
        if (isInCombat) return;

        int betAmount = Mathf.RoundToInt(value);

        // Update the effect
        if (gamblerEffect != null)
        {
            gamblerEffect.SetBetAmount(betAmount);
        }

        // Update UI display
        UpdateBetDisplay(betAmount);

        // Update health bar preview
        UpdateHealthBarPreview(betAmount);
    }

    private void UpdateHealthBarPreview(int betAmount)
    {
        if (playerHealthBar == null || player == null) return;

        // Create a preview of what the health would look like
        int currentHealth = player.CurrentHealth;
        int previewHealth = currentHealth - betAmount;

        // You might want to add a preview overlay to the health bar
        // For now, we'll use the fill amount preview
        float currentFillAmount = (float)currentHealth / player.maxHealth;
        float previewFillAmount = (float)previewHealth / player.maxHealth;

        // This is a simple approach - you might want to add a semi-transparent overlay
        // to show the "at risk" HP instead
        if (betAmount > 0)
        {
            // Show some visual feedback that HP is at risk
            // This could be a different color overlay or a dotted line
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetInteractable(bool interactable)
    {
        if (betSlider != null)
        {
            betSlider.interactable = interactable;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = interactable ? 1f : 0.5f;
        }
    }

    public void ResetSlider()
    {
        if (betSlider != null)
        {
            // Temporarily remove the listener to avoid any recursion
            betSlider.onValueChanged.RemoveListener(OnSliderValueChanged);

            // Set slider to 0
            betSlider.value = 0;

            // Re-add the listener
            betSlider.onValueChanged.AddListener(OnSliderValueChanged);

            // Manually update the UI to ensure texts are synchronized
            UpdateBetDisplay(0);
        }
    }

    private void UpdateBetDisplay(int betAmount)
    {
        if (betAmountText != null)
        {
            betAmountText.text = $"Bet: {betAmount} HP";
        }

        if (bonusDamageText != null)
        {
            int bonusDamage = betAmount / 2;
            bonusDamageText.text = $"+{bonusDamage} Damage";
        }
    }

    public void FullReset()
    {
        // Reset slider
        if (betSlider != null)
        {
            betSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            betSlider.value = 0;
            betSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        // Reset display texts
        UpdateBetDisplay(0);

        // Hide the UI
        gameObject.SetActive(false);
    }
}