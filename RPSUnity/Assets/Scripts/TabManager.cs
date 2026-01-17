using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabManager : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public string tabName;
        public Button tabButton;
        public GameObject contentPanel;
        public Image buttonBackground; // For changing color/sprite when active
    }

    [Header("Tab Configuration")]
    [SerializeField] private List<Tab> tabs = new List<Tab>();
    [SerializeField] private int defaultTabIndex = 0;

    [Header("Visual Settings")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Sprite activeTabSprite; // Optional: different sprite for active tab
    [SerializeField] private Sprite inactiveTabSprite; // Optional: different sprite for inactive tab

    private int currentTabIndex = -1;

    private void Start()
    {
        // Set up button listeners
        for (int i = 0; i < tabs.Count; i++)
        {
            int tabIndex = i; // Capture for closure
            if (tabs[i].tabButton != null)
            {
                tabs[i].tabButton.onClick.AddListener(() => SwitchToTab(tabIndex));
            }
        }

        // Show default tab
        SwitchToTab(defaultTabIndex);
    }

    public void SwitchToTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabs.Count) return;
        if (tabIndex == currentTabIndex) return; // Already on this tab

        Debug.Log($"[TabManager] Switching to tab {tabIndex}: {tabs[tabIndex].tabName}");

        // Hide all content panels and update button visuals
        for (int i = 0; i < tabs.Count; i++)
        {
            Tab tab = tabs[i];
            bool isActive = (i == tabIndex);

            // Show/hide content
            if (tab.contentPanel != null)
            {
                tab.contentPanel.SetActive(isActive);
            }

            // Update button visual state
            UpdateTabButtonVisual(tab, isActive);
        }

        currentTabIndex = tabIndex;

        // Optional: Call specific initialization for the tab
        OnTabActivated(tabIndex);
    }

    private void UpdateTabButtonVisual(Tab tab, bool isActive)
    {
        if (tab.buttonBackground != null)
        {
            // Update color
            tab.buttonBackground.color = isActive ? activeTabColor : inactiveTabColor;

            // Update sprite if provided
            if (activeTabSprite != null && inactiveTabSprite != null)
            {
                tab.buttonBackground.sprite = isActive ? activeTabSprite : inactiveTabSprite;
            }
        }

        // Optional: Update button interactability
        if (tab.tabButton != null)
        {
            // You might want to disable the active tab's button
            // tab.tabButton.interactable = !isActive;
        }
    }

    private void OnTabActivated(int tabIndex)
    {
        switch (tabIndex)
        {
            case 0: // Passive Power-Ups Tab
                Debug.Log("[TabManager] Passive power-ups tab activated");
                RestartFloatingAnimations();
                break;

            case 1: // Active Power-Ups Tab
                Debug.Log("[TabManager] Active power-ups tab activated");
                RestartFloatingAnimations(); // This will start the animations when tab is shown
                break;

            case 2: // Sell Tab
                Debug.Log("[TabManager] Sell tab activated");
                SellTabManager sellManager = tabs[2].contentPanel.GetComponent<SellTabManager>();
                if (sellManager != null)
                {
                    sellManager.PopulateSellTab();
                }
                RestartFloatingAnimations();
                break;
        }
    }

    private void PopulateSellTab()
    {
        // This will be implemented when we create the sell functionality
        // For now, just a placeholder
        Debug.Log("[TabManager] PopulateSellTab - TODO: Show player's acquired power-ups");
    }

    public int GetCurrentTabIndex()
    {
        return currentTabIndex;
    }

    public void SetTabInteractable(int tabIndex, bool interactable)
    {
        if (tabIndex >= 0 && tabIndex < tabs.Count && tabs[tabIndex].tabButton != null)
        {
            tabs[tabIndex].tabButton.interactable = interactable;
        }
    }

    // Add this method to your TabManager class:

    private void RestartFloatingAnimations()
    {
        // Find all PowerUpCardDisplay components in the current tab
        if (currentTabIndex >= 0 && currentTabIndex < tabs.Count && tabs[currentTabIndex].contentPanel != null)
        {
            PowerUpCardDisplay[] cards = tabs[currentTabIndex].contentPanel.GetComponentsInChildren<PowerUpCardDisplay>();

            foreach (PowerUpCardDisplay card in cards)
            {
                // Get the animator and restart the floating animation
                Animator animator = card.GetComponent<Animator>();
                if (animator != null && animator.enabled)
                {
                    // Generate a new random offset for variety
                    float randomOffset = Random.Range(0f, 1f);
                    animator.Play("PowerUpCard_Floating", 0, randomOffset);

                    // Optional: Vary the speed again
                    animator.speed = Random.Range(0.9f, 1.1f);

                    Debug.Log($"[TabManager] Restarted floating animation for {card.name}");
                }
            }
        }
    }

    private void OnEnable()
    {
        // Always start with the default tab when panel opens
        SwitchToTab(defaultTabIndex);
    }

    // Add this method to reset to default tab when panel opens:
    public void ResetToDefaultTab()
    {
        SwitchToTab(defaultTabIndex);
    }
}