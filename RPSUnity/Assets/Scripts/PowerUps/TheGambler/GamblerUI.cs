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

    [Header("Impulsive Gambler UI")]
    [SerializeField] private GameObject impulsiveTrackerPanel;
    [SerializeField] private TextMeshProUGUI progressText; // "High Bets: 2/3"
    [SerializeField] private Image damageReductionIcon; // Shows when active

    private IGamblerEffect gamblerEffect;
    private HandController player;
    private HealthBar playerHealthBar;

    public void Initialize(IGamblerEffect effect, HandController playerController)
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

            // Set initial max value
            int maxBet = gamblerEffect.GetMaxBet();
            betSlider.maxValue = maxBet;
        }

        // Update display immediately
        UpdateBetDisplay(0);
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

        int rawAmount = Mathf.RoundToInt(value);

        // Get snapped amount from gambler effect
        int snappedAmount = rawAmount;
        if (gamblerEffect != null)
        {
            snappedAmount = gamblerEffect.GetSnappedBetAmount(rawAmount);
        }

        // Snap the slider to valid increment
        if (betSlider != null && Mathf.Abs(betSlider.value - snappedAmount) > 0.01f)
        {
            betSlider.SetValueWithoutNotify(snappedAmount);
        }

        // Update the effect
        if (gamblerEffect != null)
        {
            gamblerEffect.SetBetAmount(snappedAmount);
        }

        // Update UI display
        UpdateBetDisplay(snappedAmount);

        // Update health bar preview
        UpdateHealthBarPreview(snappedAmount);
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

    public void UpdateBetDisplay(int betAmount)
    {
        if (betAmountText != null)
        {
            int maxBet = gamblerEffect != null ? gamblerEffect.GetMaxBet() : 0;
            betAmountText.text = $"Bet: {betAmount}/{maxBet} HP";
        }

        if (bonusDamageText != null)
        {
            int bonusDamage = gamblerEffect != null ? gamblerEffect.GetBonusDamage() : 0;
            bonusDamageText.text = $"+{bonusDamage} dmg";
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
            SetupForImpulsiveGambler(false);
        }

        // Reset display texts
        UpdateBetDisplay(0);

        // Hide the UI
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (gamblerEffect != null && player != null)
        {
            // Always refresh the display to show current max
            UpdateBetDisplay(gamblerEffect.GetCurrentBet());
        }
    }

    public void SetupForImpulsiveGambler(bool show)
    {
        if (impulsiveTrackerPanel != null)
        {
            impulsiveTrackerPanel.SetActive(show);
            Debug.Log($"[GamblerUI] Impulsive tracker panel set to: {show}");
        }
    }

    public void UpdateImpulsiveProgress(int current, int required, bool hasReduction)
    {
        if (progressText != null)
        {
            progressText.text = $"High Bets: {current}/{required}";
        }
    }
}