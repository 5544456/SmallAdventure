// ВРЕМЕННО ОТКЛЮЧЕНО
/*
using UnityEngine;
using System.Collections.Generic;
using System;

public class PerkSystem : MonoBehaviour
{
    [Header("Настройки перков")]
    [SerializeField] private int perkPointsPerLevel = 1;
    [SerializeField] private List<Perk> availablePerks = new List<Perk>();
    [SerializeField] private int startingPerkPoints = 0;
    
    private Dictionary<PerkCategory, List<Perk>> perks;
    private int currentPerkPoints;
    private Dictionary<string, Perk> perksByName = new Dictionary<string, Perk>();

    // События
    public static event Action<Perk> OnPerkUnlocked;
    public static event Action<Perk> OnPerkLevelUp;
    public static event Action<int> OnPerkPointsChanged;

    public IReadOnlyList<Perk> AvailablePerks => availablePerks;
    public int PerkPoints => currentPerkPoints;

    private void Awake()
    {
        currentPerkPoints = startingPerkPoints;
        
        InitializePerks();
        
        foreach (Perk perk in availablePerks)
        {
            perksByName[perk.PerkName] = perk;
        }
        
        // Подписываемся на событие повышения уровня
        LevelSystem.OnLevelUp += OnLevelUp;
    }

    private void OnDestroy()
    {
        LevelSystem.OnLevelUp -= OnLevelUp;
    }

    private void InitializePerks()
    {
        perks = new Dictionary<PerkCategory, List<Perk>>();

        // Инициализация перков для стрельбы из лука
        perks[PerkCategory.Archery] = new List<Perk>();
        // Добавьте инициализацию перков здесь

        // Инициализация перков для мечей
        perks[PerkCategory.Swords] = new List<Perk>();
        // Добавьте инициализацию перков здесь

        // Инициализация перков для скрытности
        perks[PerkCategory.Stealth] = new List<Perk>();
        // Добавьте инициализацию перков здесь

        // Инициализация перков для щитов
        perks[PerkCategory.Shields] = new List<Perk>();
        // Добавьте инициализацию перков здесь
    }

    private void OnLevelUp(int newLevel)
    {
        AddPerkPoints(perkPointsPerLevel);
    }

    public void AddPerkPoints(int points)
    {
        currentPerkPoints += points;
        OnPerkPointsChanged?.Invoke(currentPerkPoints);
    }

    public bool UnlockPerk(string perkName)
    {
        if (!perksByName.ContainsKey(perkName))
            return false;

        Perk perk = perksByName[perkName];
        if (perk.IsUnlocked || currentPerkPoints <= 0)
            return false;

        currentPerkPoints--;
        OnPerkPointsChanged?.Invoke(currentPerkPoints);

        if (perk.Unlock())
        {
            OnPerkUnlocked?.Invoke(perk);
            return true;
        }

        return false;
    }

    public bool LevelUpPerk(string perkName)
    {
        if (!perksByName.ContainsKey(perkName))
            return false;

        Perk perk = perksByName[perkName];
        if (!perk.IsUnlocked || !perk.CanLevelUp(currentPerkPoints))
            return false;

        int requiredPoints = perk.GetRequiredPoints();
        currentPerkPoints -= requiredPoints;
        OnPerkPointsChanged?.Invoke(currentPerkPoints);

        if (perk.LevelUp())
        {
            OnPerkLevelUp?.Invoke(perk);
            return true;
        }

        return false;
    }

    public float GetPerkValue(string perkName)
    {
        if (!perksByName.ContainsKey(perkName))
            return 0f;

        return perksByName[perkName].GetCurrentValue();
    }

    public List<Perk> GetPerksInCategory(PerkCategory category)
    {
        if (perks.ContainsKey(category))
            return new List<Perk>(perks[category]);
        return new List<Perk>();
    }

    public void SavePerks()
    {
        // Реализация сохранения перков
    }

    public void LoadPerks()
    {
        // Реализация загрузки перков
    }

    public Perk GetPerk(string perkName)
    {
        if (perksByName.ContainsKey(perkName))
            return perksByName[perkName];
        return null;
    }

    public bool CanUnlockPerk(Perk perk)
    {
        return !perk.IsUnlocked && currentPerkPoints > 0;
    }
}
*/ 