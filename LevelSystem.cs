// ВРЕМЕННО ОТКЛЮЧЕНО
/*
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class LevelSystem : MonoBehaviour
{
    [Header("Настройки опыта")]
    [SerializeField] private int maxLevel = 5;
    [SerializeField] private int experiencePerKill = 50;
    [SerializeField] private int[] experienceRequirements;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private GameObject perkSelectionPanel;

    private int currentLevel = 1;
    private int currentExperience = 0;
    private bool canSelectPerk = false;

    public static event Action<int> OnLevelUp;
    public static event Action<int> OnExperienceGained;

    private void Start()
    {
        // Инициализация требований опыта для каждого уровня
        experienceRequirements = new int[maxLevel];
        for (int i = 0; i < maxLevel; i++)
        {
            experienceRequirements[i] = (i + 1) * 100; // 100, 200, 300, 400, 500
        }

        UpdateUI();
        
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
            
        if (perkSelectionPanel != null)
            perkSelectionPanel.SetActive(false);
    }

    private void Update()
    {
        // Открытие/закрытие панели перков по кнопке P
        if (Input.GetKeyDown(KeyCode.P) && canSelectPerk)
        {
            TogglePerkPanel();
        }
    }

    public void AddExperience(int amount)
    {
        if (currentLevel >= maxLevel)
            return;

        currentExperience += amount;
        OnExperienceGained?.Invoke(amount);

        while (currentLevel < maxLevel && currentExperience >= experienceRequirements[currentLevel - 1])
        {
            currentExperience -= experienceRequirements[currentLevel - 1];
            LevelUp();
        }

        UpdateUI();
    }

    public void AddKillExperience()
    {
        AddExperience(experiencePerKill);
    }

    private void LevelUp()
    {
        currentLevel++;
        canSelectPerk = true;
        
        OnLevelUp?.Invoke(currentLevel);

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            Invoke(nameof(HideLevelUpPanel), 3f);
        }

        if (perkSelectionPanel != null)
            perkSelectionPanel.SetActive(true);
    }

    private void HideLevelUpPanel()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }

    private void TogglePerkPanel()
    {
        if (perkSelectionPanel != null)
            perkSelectionPanel.SetActive(!perkSelectionPanel.activeSelf);
    }

    private void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Уровень: {currentLevel}";

        if (experienceText != null)
        {
            if (currentLevel < maxLevel)
            {
                experienceText.text = $"Опыт: {currentExperience}/{experienceRequirements[currentLevel - 1]}";
            }
            else
            {
                experienceText.text = "Максимальный уровень достигнут!";
            }
        }

        if (experienceSlider != null && currentLevel < maxLevel)
        {
            experienceSlider.maxValue = experienceRequirements[currentLevel - 1];
            experienceSlider.value = currentExperience;
        }
    }

    public int CurrentLevel => currentLevel;
    public bool CanSelectPerk => canSelectPerk;
    public void PerkSelected() => canSelectPerk = false;
}
*/ 