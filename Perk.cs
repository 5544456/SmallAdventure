// ВРЕМЕННО ОТКЛЮЧЕНО
/*
using UnityEngine;

[System.Serializable]
public class PerkLevel
{
    public float value;
    public int requiredPoints;
}

public enum PerkCategory
{
    Archery,
    Swords,
    Stealth,
    Shields
}

[System.Serializable]
public class Perk
{
    [SerializeField] private string perkName;
    [SerializeField] private string description;
    [SerializeField] private PerkCategory category;
    [SerializeField] private PerkLevel[] levels;
    [SerializeField] private int maxLevel = 1;
    
    private int currentLevel = 0;
    private bool isUnlocked = false;

    public string PerkName => perkName;
    public string Description => description;
    public int MaxLevel => maxLevel;
    public int CurrentLevel => currentLevel;
    public bool IsUnlocked => isUnlocked;
    public PerkCategory Category => category;
    public PerkLevel[] Levels => levels;

    public bool CanLevelUp(int availablePoints)
    {
        if (currentLevel >= maxLevel) return false;
        return availablePoints >= GetRequiredPoints();
    }

    public bool Unlock()
    {
        if (isUnlocked) return false;
        isUnlocked = true;
        currentLevel = 1;
        return true;
    }

    public bool LevelUp()
    {
        if (currentLevel >= maxLevel) return false;
        currentLevel++;
        return true;
    }

    public string GetNextLevelDescription()
    {
        if (currentLevel >= maxLevel) return "Максимальный уровень достигнут";
        return levels[currentLevel].value.ToString();
    }

    public int GetRequiredPoints()
    {
        if (currentLevel >= maxLevel) return 0;
        return levels[currentLevel].requiredPoints;
    }

    public float GetCurrentValue()
    {
        if (!isUnlocked || currentLevel <= 0 || currentLevel > levels.Length)
            return 0f;
        return levels[currentLevel - 1].value;
    }
}
*/ 