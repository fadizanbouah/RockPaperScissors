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
        // Override this or use events to handle tab-specific initialization
        switch (tabIndex)
        {
            case 0: // Power-Up Selection Tab
                Debug.Log("[TabManager] Power-up selection tab activated");
                // The existing power-up spawning logic will handle this
                break;

            case 1: // Sell Cards Tab
                Debug.Log("[TabManager] Sell cards tab activated");
                // TODO: Populate the sell interface with player's cards
                PopulateSellTab();
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
}