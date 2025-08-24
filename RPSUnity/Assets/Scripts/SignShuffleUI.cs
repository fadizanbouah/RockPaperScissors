using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignShuffleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject counterPanel; // Parent panel with icon and text
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private Image shuffleIcon; // Optional icon/asset

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow; // When 1 round left
    [SerializeField] private Color imminentColor = Color.red; // When shuffling this round

    private HandController currentEnemy;
    private bool isInitialized = false;

    private void OnEnable()
    {
        isInitialized = false;
    }

    private void Update()
    {
        // Wait until we're in gameplay and enemy exists
        if (!isInitialized)
        {
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.currentState == GameStateManager.GameState.Gameplay &&
                RoomManager.Instance != null)
            {
                HandController enemy = RoomManager.Instance.GetCurrentEnemy();
                if (enemy != null)
                {
                    UpdateEnemyReference(enemy);
                    isInitialized = true;
                }
            }
        }

        // Update counter display
        if (isInitialized && currentEnemy != null)
        {
            UpdateCounterDisplay();
        }
    }

    public void UpdateEnemyReference(HandController enemy)
    {
        currentEnemy = enemy;

        // Check if this enemy uses sign shuffle
        if (enemy != null && enemy.UsesSignShuffle())
        {
            ShowCounter();
            UpdateCounterDisplay();
        }
        else
        {
            HideCounter();
        }
    }

    private void UpdateCounterDisplay()
    {
        if (currentEnemy == null || !currentEnemy.UsesSignShuffle())
        {
            HideCounter();
            return;
        }

        int roundsLeft = currentEnemy.GetRoundsUntilShuffle();

        if (roundsLeft < 0)
        {
            HideCounter();
            return;
        }

        // Update text
        if (counterText != null)
        {
            counterText.text = roundsLeft.ToString();
        }
    }

    private void ShowCounter()
    {
        if (counterPanel != null)
            counterPanel.SetActive(true);
    }

    private void HideCounter()
    {
        if (counterPanel != null)
            counterPanel.SetActive(false);
    }

    public void OnRoomStart()
    {
        // Reset when entering new room
        isInitialized = false;
        HideCounter();

        HandController currentEnemy = RoomManager.Instance?.GetCurrentEnemy();
        if (currentEnemy != null)
        {
            UpdateEnemyReference(currentEnemy);
        }
    }
}