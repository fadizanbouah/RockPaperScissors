using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image healthBarFill; // Reference to the green fill image
    public TextMeshProUGUI playerHealthText;
    // Method to update the health bar
    public void SetHealth(float currentHealth, float maxHealth)
    {
        float fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        healthBarFill.fillAmount = fillAmount;
        playerHealthText.text = currentHealth + "/" + maxHealth;
    }
}
