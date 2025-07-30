using UnityEngine;
using UnityEngine.Events;

public class QuestNPC : MonoBehaviour
{
    [Header("Настройки квеста")]
    [SerializeField] private int questToGive = -1; // ID квеста, который выдает NPC (-1 означает, что квест не выдается)
    [SerializeField] private int questToComplete = -1; // ID квеста, который завершает NPC (-1 означает, что квест не завершается)
    [SerializeField] private int objectiveIndex = 0; // Индекс задачи в квесте для обновления прогресса
    
    [Header("Диалоги")]
    [SerializeField] private DialogueData defaultDialogue; // Стандартный диалог
    [SerializeField] private DialogueData questActiveDialogue; // Диалог, когда квест активен
    [SerializeField] private DialogueData questCompletedDialogue; // Диалог после завершения квеста
    
    [Header("События")]
    [HideInInspector] public UnityEvent OnQuestGiven = new UnityEvent(); // Событие при выдаче квеста
    [HideInInspector] public UnityEvent OnQuestCompleted = new UnityEvent(); // Событие при завершении квеста
    
    private SimpleQuestSystem questSystem; // Ссылка на систему квестов
    private DialogueManager dialogueManager; // Ссылка на менеджер диалогов
    private bool questGiven = false; // Флаг, был ли выдан квест
    private bool questCompleted = false; // Флаг, был ли завершен квест
    
    private void Awake()
    {
        // Инициализируем события, если они null
        if (OnQuestGiven == null)
            OnQuestGiven = new UnityEvent();
            
        if (OnQuestCompleted == null)
            OnQuestCompleted = new UnityEvent();
    }
    
    private void Start()
    {
        // Находим систему квестов и менеджер диалогов
        questSystem = FindFirstObjectByType<SimpleQuestSystem>();
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        
        // Подписываемся на события системы квестов
        if (questSystem != null)
        {
            questSystem.OnQuestCompleted.AddListener(OnQuestCompletedHandler);
        }
    }
    
    // Метод, вызываемый при взаимодействии с NPC
    public void Interact()
    {
        if (dialogueManager == null) return;
        
        // Выбираем подходящий диалог в зависимости от состояния квеста
        DialogueData dialogueToShow = defaultDialogue;
        
        // Если квест завершен, показываем диалог завершения
        if (questCompleted && questCompletedDialogue != null)
        {
            dialogueToShow = questCompletedDialogue;
        }
        // Если квест активен, но не завершен, показываем диалог активного квеста
        else if (questGiven && !questCompleted && questActiveDialogue != null)
        {
            dialogueToShow = questActiveDialogue;
        }
        
        // Показываем выбранный диалог
        if (dialogueToShow != null)
        {
            dialogueManager.StartDialogue(dialogueToShow, GetComponent<FriendlyNPC>());
        }
        
        // Если квест еще не выдан и есть квест для выдачи, выдаем его
        if (!questGiven && questToGive != -1 && questSystem != null)
        {
            questSystem.ActivateQuest(questToGive);
            questGiven = true;
            OnQuestGiven?.Invoke();
        }
        
        // Если есть квест для завершения, обновляем его прогресс
        if (!questCompleted && questToComplete != -1 && questSystem != null)
        {
            questSystem.UpdateQuestProgress(questToComplete, objectiveIndex, 1);
        }
    }
    
    // Обработчик события завершения квеста
    private void OnQuestCompletedHandler(Quest quest)
    {
        // Проверяем, что это наш квест
        if (quest.id == questToComplete)
        {
            questCompleted = true;
            OnQuestCompleted?.Invoke();
        }
    }
} 