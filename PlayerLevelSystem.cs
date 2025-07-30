using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class PlayerLevelSystem : MonoBehaviour
{
    [Header("Настройки уровней")]
    [SerializeField] private int experiencePerKill = 40;
    [SerializeField] private int maxLevel = 10;
    [SerializeField] private int healthIncreasePerLevel = 10;
    [SerializeField] private int staminaIncreasePerLevel = 10;
    
    [Header("UI элементы")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private GameObject levelUpNotification;
    [SerializeField] private TextMeshProUGUI levelUpText;
    [SerializeField] private float notificationDuration = 3f;
    
    // События для уведомления других скриптов
    public UnityEvent<int> OnLevelUp;
    public UnityEvent<int, int, int> OnExperienceChanged; // текущий опыт, опыт для следующего уровня, текущий уровень
    
    private Player player;
    private LevelUI levelUI;
    private int currentLevel = 1;
    private int currentExperience = 0;
    private int experienceToNextLevel = 100;
    
    private void Awake()
    {
        // Инициализируем события
        if (OnLevelUp == null)
            OnLevelUp = new UnityEvent<int>();
        if (OnExperienceChanged == null)
            OnExperienceChanged = new UnityEvent<int, int, int>();
    }
    
    private void Start()
    {
        // Находим игрока
        player = GetComponent<Player>();
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
            if (player == null)
            {
                Debug.LogError("Не найден компонент Player!");
            }
        }
        
        // Находим UI для уровней
        levelUI = FindFirstObjectByType<LevelUI>();
        
        // Скрываем уведомление о повышении уровня
        if (levelUpNotification != null)
        {
            levelUpNotification.SetActive(false);
        }
        
        // Обновляем UI
        UpdateUI();
        
        // Уведомляем о начальных значениях
        OnExperienceChanged?.Invoke(currentExperience, experienceToNextLevel, currentLevel);
    }
    
    private void UpdateUI()
    {
        if (levelText != null)
        {
            // Отображаем только число без "Уровень: "
            levelText.text = currentLevel.ToString();
        }
        
        if (experienceText != null)
        {
            // Отображаем просто числа опыта через слэш
            experienceText.text = $"{currentExperience}/{experienceToNextLevel}";
        }
    }
    
    // Метод для добавления опыта
    public void AddExperience(int amount)
    {
        // Если достигнут максимальный уровень, не добавляем опыт
        if (currentLevel >= maxLevel)
        {
            return;
        }
        
        currentExperience += amount;
        Debug.Log($"Получено {amount} опыта. Текущий опыт: {currentExperience}/{experienceToNextLevel}");
        
        // Уведомляем об изменении опыта
        OnExperienceChanged?.Invoke(currentExperience, experienceToNextLevel, currentLevel);
        
        // Проверяем, достаточно ли опыта для повышения уровня
        CheckLevelUp();
        
        // Обновляем UI
        UpdateUI();
    }
    
    // Метод для проверки повышения уровня
    private void CheckLevelUp()
    {
        if (currentExperience >= experienceToNextLevel)
        {
            // Повышаем уровень
            currentLevel++;
            
            // Вычитаем опыт, необходимый для повышения уровня
            currentExperience -= experienceToNextLevel;
            
            // Рассчитываем опыт для следующего уровня (100, 200, 300, ...)
            experienceToNextLevel = currentLevel * 100;
            
            // Увеличиваем здоровье и выносливость игрока
            IncreasePlayerStats();
            
            // Показываем уведомление о повышении уровня
            ShowLevelUpNotification();
            
            // Уведомляем о повышении уровня
            OnLevelUp?.Invoke(currentLevel);
            OnExperienceChanged?.Invoke(currentExperience, experienceToNextLevel, currentLevel);
            
            Debug.Log($"Уровень повышен до {currentLevel}! Следующий уровень: {experienceToNextLevel} опыта");
            
            // Проверяем, не достигнут ли максимальный уровень
            if (currentLevel >= maxLevel)
            {
                Debug.Log("Достигнут максимальный уровень!");
                currentExperience = 0;
            }
            else
            {
                // Рекурсивно проверяем, хватает ли опыта для следующего уровня
                CheckLevelUp();
            }
        }
    }
    
    // Метод для увеличения характеристик игрока
    private void IncreasePlayerStats()
    {
        if (player != null)
        {
            // Увеличиваем максимальное здоровье
            player.IncreaseMaxHealth(healthIncreasePerLevel);
            
            // Увеличиваем максимальную выносливость
            player.IncreaseMaxStamina(staminaIncreasePerLevel);
        }
    }
    
    // Метод для отображения уведомления о повышении уровня
    private void ShowLevelUpNotification()
    {
        Debug.Log($"PlayerLevelSystem: Вызван метод ShowLevelUpNotification, текущий уровень: {currentLevel}");
        
        // Используем встроенное уведомление, если оно есть
        if (levelUpNotification != null && levelUpText != null)
        {
            levelUpText.text = $"Уровень повышен до {currentLevel}!";
            levelUpNotification.SetActive(true);
            
            Debug.Log($"PlayerLevelSystem: Активировано встроенное уведомление о повышении уровня");
            
            // Скрываем уведомление через notificationDuration секунд
            Invoke("HideLevelUpNotification", notificationDuration);
        }
        else
        {
            Debug.LogWarning("PlayerLevelSystem: Не настроено встроенное уведомление о повышении уровня!");
        }
        
        // Используем внешний UI, если он есть
        if (levelUI != null)
        {
            Debug.Log($"PlayerLevelSystem: Найден LevelUI, вызываем ShowLevelUpNotification({currentLevel})");
            levelUI.ShowLevelUpNotification(currentLevel);
        }
        else
        {
            Debug.LogWarning("PlayerLevelSystem: Не найден компонент LevelUI для отображения уведомления!");
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
    
    // Метод для получения опыта за убийство врага
    public void OnEnemyKilled()
    {
        AddExperience(experiencePerKill);
    }
    
    // Геттеры для доступа к текущему уровню и опыту
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    public int GetCurrentExperience()
    {
        return currentExperience;
    }
    
    public int GetExperienceToNextLevel()
    {
        return experienceToNextLevel;
    }
} 