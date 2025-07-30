using UnityEngine;
using UnityEngine.Events;

public class QuestObjectInteraction : MonoBehaviour
{
    [SerializeField] private int questId; // ID квеста
    [SerializeField] private int objectiveIndex; // Индекс задачи в квесте
    [SerializeField] private int progressAmount = 1; // Количество прогресса, которое добавляется при взаимодействии
    [SerializeField] private bool interactOnTrigger = false; // Взаимодействовать автоматически при входе в триггер
    [SerializeField] private bool interactOnce = true; // Взаимодействовать только один раз
    [SerializeField] private string playerTag = "Player"; // Тег игрока для проверки
    [SerializeField] private KeyCode interactionKey = KeyCode.E; // Клавиша для взаимодействия
    [SerializeField] private float interactionDistance = 3f; // Расстояние для взаимодействия
    
    [Header("Визуальные элементы")]
    [SerializeField] private GameObject interactionPrompt; // Подсказка для взаимодействия
    [SerializeField] private GameObject questMarker; // Маркер квеста
    
    [Header("События")]
    [HideInInspector] public UnityEvent OnInteractionComplete = new UnityEvent(); // Событие при завершении взаимодействия
    
    private bool hasInteracted = false; // Флаг, было ли уже взаимодействие
    private Transform playerTransform; // Трансформ игрока для проверки дистанции
    private bool playerInRange = false; // Флаг, находится ли игрок в зоне взаимодействия
    
    private void Awake()
    {
        // Инициализируем событие, если оно null
        if (OnInteractionComplete == null)
            OnInteractionComplete = new UnityEvent();
    }
    
    private void Start()
    {
        // Скрываем подсказку взаимодействия в начале
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Если уже взаимодействовали и можно взаимодействовать только один раз, выходим
        if (hasInteracted && interactOnce) return;
        
        // Если игрок в зоне взаимодействия и нажата клавиша взаимодействия
        if (playerInRange && Input.GetKeyDown(interactionKey) && !interactOnTrigger)
        {
            Interact();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что это игрок
        if (other.CompareTag(playerTag))
        {
            // Сохраняем трансформ игрока
            playerTransform = other.transform;
            
            // Отмечаем, что игрок в зоне взаимодействия
            playerInRange = true;
            
            // Показываем подсказку взаимодействия
            if (interactionPrompt != null && !hasInteracted)
            {
                interactionPrompt.SetActive(true);
            }
            
            // Если настроено автоматическое взаимодействие при входе в триггер
            if (interactOnTrigger && (!interactOnce || !hasInteracted))
            {
                Interact();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Проверяем, что это игрок
        if (other.CompareTag(playerTag))
        {
            // Отмечаем, что игрок вышел из зоны взаимодействия
            playerInRange = false;
            
            // Скрываем подсказку взаимодействия
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    // Метод для взаимодействия с объектом квеста
    public void Interact()
    {
        // Если уже взаимодействовали и можно взаимодействовать только один раз, выходим
        if (hasInteracted && interactOnce) return;
        
        // Находим систему квестов
        SimpleQuestSystem questSystem = FindFirstObjectByType<SimpleQuestSystem>();
        
        // Если система квестов найдена, обновляем прогресс задачи
        if (questSystem != null)
        {
            questSystem.UpdateQuestProgress(questId, objectiveIndex, progressAmount);
            
            // Отмечаем, что взаимодействие произошло
            hasInteracted = true;
            
            // Скрываем подсказку взаимодействия
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            
            // Скрываем маркер квеста, если он есть
            if (questMarker != null)
            {
                questMarker.SetActive(false);
            }
            
            // Вызываем событие завершения взаимодействия
            OnInteractionComplete?.Invoke();
            
            // Если нужно взаимодействовать только один раз, можно отключить компонент
            if (interactOnce)
            {
                // Отключаем коллайдер
                Collider interactionCollider = GetComponent<Collider>();
                if (interactionCollider != null)
                {
                    interactionCollider.enabled = false;
                }
            }
        }
        else
        {
            Debug.LogWarning("QuestObjectInteraction: SimpleQuestSystem не найден в сцене!");
        }
    }
} 