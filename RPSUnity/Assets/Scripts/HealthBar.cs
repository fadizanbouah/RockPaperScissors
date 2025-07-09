using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    public Image healthBarFill; // Reference to the green fill image
    public TextMeshProUGUI playerHealthText;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f; // How long the animation takes

    private Coroutine animationCoroutine;
    private float targetFillAmount;

    // Method to update the health bar
    public void SetHealth(float currentHealth, float maxHealth)
    {
        float fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        targetFillAmount = fillAmount;

        // Update text immediately
        playerHealthText.text = currentHealth + "/" + maxHealth;

        // Stop any ongoing animation
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        // Start smooth animation
        animationCoroutine = StartCoroutine(AnimateHealthBar());
    }

    private IEnumerator AnimateHealthBar()
    {
        float startFill = healthBarFill.fillAmount;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // Use smooth curve for more natural animation
            t = Mathf.SmoothStep(0, 1, t);

            healthBarFill.fillAmount = Mathf.Lerp(startFill, targetFillAmount, t);
            yield return null;
        }

        // Ensure we end at exact target
        healthBarFill.fillAmount = targetFillAmount;
        animationCoroutine = null;
    }
}