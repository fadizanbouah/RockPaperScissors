using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Upgrade Controllers")]
    [SerializeField] private UpgradeButtonController maxHealthUpgrade;
    [SerializeField] private UpgradeButtonController baseDamageUpgrade;
    [SerializeField] private UpgradeButtonController dodgeChanceUpgrade;
    [SerializeField] private UpgradeButtonController critChanceUpgrade;

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
}