using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private Slider experienceSlider;
    
    [Header("Настройки уведомления")]
    [SerializeField] private GameObject levelUpNotification;
    [SerializeField] private TextMeshProUGUI levelUpText;
    [SerializeField] private float notificationDuration = 3f;
    
    private PlayerLevelSystem levelSystem;
    
    private void Start()
    {
        // Находим систему уровней игрока
        levelSystem = FindFirstObjectByType<PlayerLevelSystem>();
        
        if (levelSystem == null)
        {
            Debug.LogError("Не найден компонент PlayerLevelSystem!");
            return;
        }
        
        // Скрываем уведомление о повышении уровня
        if (levelUpNotification != null)
        {
            levelUpNotification.SetActive(false);
        }
        
        // Подписываемся на события изменения уровня и опыта
        levelSystem.OnLevelUp.AddListener(OnLevelUp);
        levelSystem.OnExperienceChanged.AddListener(OnExperienceChanged);
        
        // Обновляем UI при старте
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта
        if (levelSystem != null)
        {
            levelSystem.OnLevelUp.RemoveListener(OnLevelUp);
            levelSystem.OnExperienceChanged.RemoveListener(OnExperienceChanged);
        }
    }
    
    // Обработчик события повышения уровня
    private void OnLevelUp(int newLevel)
    {
        // Показываем уведомление о повышении уровня
        ShowLevelUpNotification(newLevel);
        
        // Обновляем UI
        UpdateUI();
    }
    
    // Обработчик события изменения опыта
    private void OnExperienceChanged(int currentExp, int expToNextLevel, int currentLevel)
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (levelSystem == null) return;
        
        int currentLevel = levelSystem.GetCurrentLevel();
        int currentExperience = levelSystem.GetCurrentExperience();
        int experienceToNextLevel = levelSystem.GetExperienceToNextLevel();
        
        // Обновляем текст уровня (просто число без "Уровень: ")
        if (levelText != null)
        {
            levelText.text = currentLevel.ToString();
        }
        
        // Обновляем текст опыта
        if (experienceText != null)
        {
            experienceText.text = $"{currentExperience}/{experienceToNextLevel}";
        }
        
        // Обновляем слайдер опыта
        if (experienceSlider != null)
        {
            experienceSlider.value = (float)currentExperience / experienceToNextLevel;
        }
    }
    
    // Метод для отображения уведомления о повышении уровня
    public void ShowLevelUpNotification(int newLevel)
    {
        Debug.Log($"Показываем уведомление о повышении уровня до {newLevel}");
        
        if (levelUpNotification != null && levelUpText != null)
        {
            levelUpText.text = $"Уровень повышен до {newLevel}!";
            levelUpNotification.SetActive(true);
            
            // Скрываем уведомление через notificationDuration секунд
            Invoke("HideLevelUpNotification", notificationDuration);
        }
        else
        {
            Debug.LogWarning("Не настроено уведомление о повышении уровня!");
        }
    }
    
    // Метод для скрытия уведомления о повышении уровня
    private void HideLevelUpNotification()
    {
        if (levelUpNotification != null)
        {
            levelUpNotification.SetActive(false);
        }
    }
} 