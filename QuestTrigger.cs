using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    [SerializeField] private int questId; // ID квеста, который нужно активировать
    [SerializeField] private bool triggerOnce = true; // Активировать триггер только один раз
    [SerializeField] private string playerTag = "Player"; // Тег игрока для проверки
    
    private bool hasTriggered = false; // Флаг, был ли уже активирован триггер
    
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что это игрок и триггер еще не был активирован (если triggerOnce = true)
        if (other.CompareTag(playerTag) && (!triggerOnce || !hasTriggered))
        {
            // Находим систему квестов
            SimpleQuestSystem questSystem = FindFirstObjectByType<SimpleQuestSystem>();
            
            // Если система квестов найдена, активируем квест
            if (questSystem != null)
            {
                questSystem.ActivateQuest(questId);
                
                // Отмечаем, что триггер был активирован
                hasTriggered = true;
                
                // Если триггер должен сработать только один раз, отключаем его
                if (triggerOnce)
                {
                    // Отключаем коллайдер
                    Collider triggerCollider = GetComponent<Collider>();
                    if (triggerCollider != null)
                    {
                        triggerCollider.enabled = false;
                    }
                    
                    // Можно также скрыть визуальное представление триггера, если оно есть
                    Renderer triggerRenderer = GetComponent<Renderer>();
                    if (triggerRenderer != null)
                    {
                        triggerRenderer.enabled = false;
                    }
                }
            }
            else
            {
                Debug.LogWarning("QuestTrigger: SimpleQuestSystem не найден в сцене!");
            }
        }
    }
} 