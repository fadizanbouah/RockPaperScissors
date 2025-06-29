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

        // Update slider max value based on current HP
        int maxBet = gamblerEffect.GetMaxBet();
        if (betSlider != null && betSlider.maxValue != maxBet)
        {
            betSlider.maxValue = maxBet;
            // Clamp current value if it exceeds new max
            if (betSlider.value > maxBet)
            {
                betSlider.value = maxBet;
            }
        }

        // Disable during combat
        bool canBet = RockPaperScissorsGame.Instance == null ||
                      !RockPaperScissorsGame.Instance.IsInCombat();
        SetInteractable(canBet);
    }

    private void OnSliderValueChanged(float value)
    {
        int betAmount = Mathf.RoundToInt(value);

        // Update the effect
        if (gamblerEffect != null)
        {
            gamblerEffect.SetBetAmount(betAmount);
        }

        // Update UI text
        if (betAmountText != null)
        {
            betAmountText.text = $"Bet: {betAmount} HP";
        }

        if (bonusDamageText != null)
        {
            int bonusDamage = betAmount / 2;
            bonusDamageText.text = $"+{bonusDamage} Damage";
        }

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
            betSlider.value = 0;
            OnSliderValueChanged(0); // This will update the text displays as well
        }
    }
}