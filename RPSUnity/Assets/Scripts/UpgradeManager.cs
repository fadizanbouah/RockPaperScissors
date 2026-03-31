using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Upgrade Controllers")]
    [SerializeField] private UpgradeButtonController maxHealthUpgrade;
    [SerializeField] private UpgradeButtonController baseDamageUpgrade;
    [SerializeField] private UpgradeButtonController dodgeChanceUpgrade;
    [SerializeField] private UpgradeButtonController critChanceUpgrade;
    [SerializeField] private UpgradeButtonController startingCardsUpgrade;

    [Header("Starting Card Pool")]
    [SerializeField] private PowerUpData[] startingCardPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public int GetMaxHealthBonus()
    {
        if (maxHealthUpgrade != null)
            return maxHealthUpgrade.GetTotalStatIncrease();

        // Fallback to old system if upgrade controller not found
        return PlayerProgressData.Instance.maxHealthLevel * 5;
    }

    public int GetBaseDamageBonus()
    {
        if (baseDamageUpgrade != null)
            return baseDamageUpgrade.GetTotalStatIncrease();

        // Fallback to old system if upgrade controller not found
        return PlayerProgressData.Instance.baseDamageLevel * 2;
    }

    public float GetDodgeChanceBonus()
    {
        if (dodgeChanceUpgrade != null)
            return dodgeChanceUpgrade.GetTotalStatIncrease();

        // Fallback: 5% per level
        return PlayerProgressData.Instance.dodgeChanceLevel * 5f;
    }

    public float GetCritChanceBonus()
    {
        if (critChanceUpgrade != null)
            return critChanceUpgrade.GetTotalStatIncrease();

        // Fallback: 5% per level
        return PlayerProgressData.Instance.critChanceLevel * 5f;
    }

    public int GetStartingCardsCount()
    {
        if (startingCardsUpgrade != null)
            return startingCardsUpgrade.GetTotalStatIncrease();

        // Fallback: 1 card per level
        return PlayerProgressData.Instance.startingCardsLevel;
    }

    public void GrantStartingCards()
    {
        int count = GetStartingCardsCount();
        if (count <= 0 || startingCardPool == null || startingCardPool.Length == 0) return;

        // Build a copy of the pool and pick without duplicates using Fisher-Yates partial shuffle
        List<PowerUpData> pool = new List<PowerUpData>(startingCardPool);
        int toGrant = Mathf.Min(count, pool.Count);

        for (int i = 0; i < toGrant; i++)
        {
            int randomIndex = Random.Range(i, pool.Count);
            PowerUpData chosen = pool[randomIndex];
            pool[randomIndex] = pool[i];
            pool[i] = chosen;

            RunProgressManager.Instance.AddAcquiredPowerUp(chosen);
            Debug.Log($"[UpgradeManager] Granted starting card: {chosen.powerUpName}");
        }
    }
}