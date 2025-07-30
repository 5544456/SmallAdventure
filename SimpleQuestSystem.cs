using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

// Класс для определения задачи квеста
[System.Serializable]
public class QuestObjective
{
    public string description;
    public int targetAmount = 1;
    public int currentAmount;
    public bool isCompleted;
}

// Класс для определения квеста
[System.Serializable]
public class Quest
{
    public int id;
    public string title;
    public string description;
    public QuestObjective[] objectives;
    public int experienceReward;
    public int goldReward;
    public bool isActive;
    public bool isCompleted;
}

public class SimpleQuestSystem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject questDisplayPanel; // Панель отображения активного квеста
    [SerializeField] private TextMeshProUGUI questTitleText; // Текст названия квеста
    [SerializeField] private TextMeshProUGUI questObjectiveText; // Текст задачи квеста
    [SerializeField] private TextMeshProUGUI questRewardText; // Текст награды за квест
    
    [Header("Quest Notification")]
    [SerializeField] private GameObject questNotificationPanel; // Панель уведомления о новом квесте
    [SerializeField] private TextMeshProUGUI questNotificationText; // Текст уведомления
    [SerializeField] private float notificationDisplayTime = 5f; // Время отображения уведомления
    
    [Header("Quests")]
    [SerializeField] private Quest[] availableQuests; // Массив доступных квестов
    
    private List<Quest> activeQuests = new List<Quest>(); // Список активных квестов
    private Quest currentDisplayedQuest; // Текущий отображаемый квест
    
    // События для интеграции с другими системами
    [HideInInspector] public UnityEvent<Quest> OnQuestActivated = new UnityEvent<Quest>(); // Событие при активации квеста
    [HideInInspector] public UnityEvent<Quest> OnQuestCompleted = new UnityEvent<Quest>(); // Событие при завершении квеста
    [HideInInspector] public UnityEvent<Quest, int> OnQuestObjectiveUpdated = new UnityEvent<Quest, int>(); // Событие при обновлении задачи квеста
    
    private void Awake()
    {
        // Инициализируем события, если они null
        if (OnQuestActivated == null)
            OnQuestActivated = new UnityEvent<Quest>();
            
        if (OnQuestCompleted == null)
            OnQuestCompleted = new UnityEvent<Quest>();
            
        if (OnQuestObjectiveUpdated == null)
            OnQuestObjectiveUpdated = new UnityEvent<Quest, int>();
    }
    
    private void Start()
    {
        // Скрываем панели в начале
        if (questDisplayPanel != null) questDisplayPanel.SetActive(false);
        if (questNotificationPanel != null) questNotificationPanel.SetActive(false);
        
        // Инициализируем начальные квесты, если они есть
        InitializeStartingQuests();
    }
    
    // Метод для инициализации начальных квестов
    private void InitializeStartingQuests()
    {
        // Пример автоматической активации первого квеста
        if (availableQuests != null && availableQuests.Length > 0)
        {
            ActivateQuest(availableQuests[0].id);
        }
    }
    
    // Метод для активации квеста по ID
    public void ActivateQuest(int questId)
    {
        // Находим квест по ID
        Quest quest = System.Array.Find(availableQuests, q => q.id == questId);
        
        // Проверяем, найден ли квест и не активен ли он уже
        if (quest != null && !quest.isActive && !quest.isCompleted)
        {
            // Активируем квест
            quest.isActive = true;
            activeQuests.Add(quest);
            
            // Отображаем уведомление о новом квесте
            ShowQuestNotification($"Новый квест: {quest.title}");
            
            // Обновляем UI
            UpdateQuestDisplay();
            
            // Вызываем событие активации квеста
            OnQuestActivated?.Invoke(quest);
        }
    }
    
    // Метод для обновления прогресса квеста
    public void UpdateQuestProgress(int questId, int objectiveIndex, int amount)
    {
        // Находим активный квест по ID
        Quest quest = activeQuests.Find(q => q.id == questId);
        
        // Проверяем, найден ли квест
        if (quest != null && objectiveIndex >= 0 && objectiveIndex < quest.objectives.Length)
        {
            // Получаем задачу
            QuestObjective objective = quest.objectives[objectiveIndex];
            
            // Обновляем прогресс
            objective.currentAmount += amount;
            
            // Проверяем, выполнена ли задача
            if (objective.currentAmount >= objective.targetAmount)
            {
                objective.isCompleted = true;
                objective.currentAmount = objective.targetAmount; // Ограничиваем максимальным значением
            }
            
            // Проверяем, выполнены ли все задачи квеста
            CheckQuestCompletion(quest);
            
            // Обновляем UI
            UpdateQuestDisplay();
            
            // Вызываем событие обновления задачи
            OnQuestObjectiveUpdated?.Invoke(quest, objectiveIndex);
        }
    }
    
    // Метод для проверки завершения квеста
    private void CheckQuestCompletion(Quest quest)
    {
        // Проверяем, выполнены ли все задачи
        bool allCompleted = true;
        foreach (QuestObjective objective in quest.objectives)
        {
            if (!objective.isCompleted)
            {
                allCompleted = false;
                break;
            }
        }
        
        // Если все задачи выполнены, завершаем квест
        if (allCompleted && !quest.isCompleted)
        {
            CompleteQuest(quest);
        }
    }
    
    // Метод для завершения квеста
    private void CompleteQuest(Quest quest)
    {
        // Отмечаем квест как завершенный
        quest.isCompleted = true;
        
        // Выдаем награду через BuildingSystem
        BuildingSystem buildingSystem = FindFirstObjectByType<BuildingSystem>();
        if (buildingSystem != null)
        {
            // Добавляем золото в качестве награды
            buildingSystem.AddGold(quest.goldReward);
            
            // Показываем уведомление об этом
            ShowQuestNotification($"Квест завершен: {quest.title}\nПолучено: {quest.experienceReward} опыта, {quest.goldReward} золота");
        }
        else
        {
            // Если BuildingSystem не найден, просто показываем уведомление
            ShowQuestNotification($"Квест завершен: {quest.title}\nПолучено: {quest.experienceReward} опыта, {quest.goldReward} золота");
        }
        
        // Удаляем квест из списка активных
        activeQuests.Remove(quest);
        
        // Обновляем UI
        UpdateQuestDisplay();
        
        // Вызываем событие завершения квеста
        OnQuestCompleted?.Invoke(quest);
    }
    
    // Метод для обновления отображения квеста
    private void UpdateQuestDisplay()
    {
        // Если нет активных квестов, скрываем панель
        if (activeQuests.Count == 0)
        {
            if (questDisplayPanel != null) questDisplayPanel.SetActive(false);
            currentDisplayedQuest = null;
            return;
        }
        
        // Берем первый активный квест для отображения
        currentDisplayedQuest = activeQuests[0];
        
        // Показываем панель
        if (questDisplayPanel != null) questDisplayPanel.SetActive(true);
        
        // Обновляем текст названия квеста
        if (questTitleText != null)
        {
            questTitleText.text = currentDisplayedQuest.title;
        }
        
        // Обновляем текст задачи квеста
        if (questObjectiveText != null)
        {
            string objectives = "";
            foreach (QuestObjective objective in currentDisplayedQuest.objectives)
            {
                string status = objective.isCompleted ? "✓" : "";
                objectives += $"{status} {objective.description} ({objective.currentAmount}/{objective.targetAmount})\n";
            }
            questObjectiveText.text = objectives.TrimEnd();
        }
        
        // Обновляем текст награды
        if (questRewardText != null)
        {
            questRewardText.text = $"Награда: {currentDisplayedQuest.experienceReward} опыта, {currentDisplayedQuest.goldReward} золота";
        }
    }
    
    // Метод для отображения уведомления о квесте с постепенным появлением текста
    private void ShowQuestNotification(string message)
    {
        if (questNotificationPanel == null || questNotificationText == null) return;
        
        // Показываем панель уведомления
        questNotificationPanel.SetActive(true);
        
        // Запускаем корутину для постепенного появления текста
        StartCoroutine(TypeNotificationText(message));
        
        // Запускаем корутину для скрытия уведомления через некоторое время
        StartCoroutine(HideNotificationAfterDelay());
    }
    
    // Корутина для постепенного появления текста
    private System.Collections.IEnumerator TypeNotificationText(string message)
    {
        questNotificationText.text = "";
        
        // Скорость печатания (в секундах на символ)
        float typingSpeed = 0.03f;
        
        // Постепенно добавляем каждый символ
        foreach (char letter in message.ToCharArray())
        {
            questNotificationText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    
    // Корутина для скрытия уведомления
    private System.Collections.IEnumerator HideNotificationAfterDelay()
    {
        // Ждем, пока текст полностью напечатается (примерное время)
        float typingTime = questNotificationText.text.Length * 0.03f;
        yield return new WaitForSeconds(typingTime + notificationDisplayTime);
        
        if (questNotificationPanel != null)
        {
            questNotificationPanel.SetActive(false);
        }
    }
} 