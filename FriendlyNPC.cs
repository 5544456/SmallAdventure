using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class FriendlyNPC : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private string npcName = "Дружелюбный NPC";
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private GameObject interactionPrompt;

    [Header("Диалоги")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Патрулирование")]
    [SerializeField] private bool canPatrol = false;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 3f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask enemyLayer;

    // Компоненты
    private Animator animator;
    private NavMeshAgent agent;
    private DialogueManager dialogueManager;
    private Player player;

    // Состояние NPC
    private bool isInteracting = false;
    private bool isPatrolling = false;
    private int currentPatrolPoint = 0;
    private bool isChasing = false;
    private GameObject targetEnemy;

    private void Start()
    {
        // Получаем компоненты
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        player = FindFirstObjectByType<Player>();

        // Настраиваем подсказку взаимодействия
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Корректируем позицию модели, чтобы она была на уровне земли
        FixModelPosition();

        // Если NPC может патрулировать и есть точки патрулирования, начинаем патрулирование
        if (canPatrol && patrolPoints != null && patrolPoints.Length > 0)
        {
            StartCoroutine(Patrol());
        }
    }

    private void Update()
    {
        // Проверяем дистанцию до игрока для взаимодействия
        CheckPlayerDistance();

        // Если NPC может патрулировать, проверяем наличие врагов
        if (canPatrol && !isInteracting)
        {
            CheckForEnemies();
        }
    }

    private void CheckPlayerDistance()
    {
        if (player == null || isInteracting) return;

        float distance = Vector3.Distance(
            interactionPoint != null ? interactionPoint.position : transform.position, 
            player.transform.position
        );

        // Если игрок в зоне взаимодействия, показываем подсказку
        if (distance <= interactionDistance)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[{name}] InteractionPrompt не назначен для этого NPC!");
            }

            // Если игрок нажал E, начинаем диалог
            if (Input.GetKeyDown(KeyCode.E) && !isInteracting)
            {
                StartInteraction();
            }
        }
        else
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void StartInteraction()
    {
        Debug.Log($"[{name}] Начинаем взаимодействие");
        
        // Находим DialogueManager, если он еще не был найден
        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogError($"[{name}] DialogueManager не найден в сцене!");
                return; // Прерываем взаимодействие, если DialogueManager не найден
            }
        }
        
        // Проверяем наличие данных диалога
        if (dialogueData == null)
        {
            Debug.LogError($"[{name}] DialogueData не назначен для этого NPC!");
            return; // Прерываем взаимодействие, если DialogueData не назначен
        }

        isInteracting = true;
        
        // Останавливаем патрулирование и поворачиваемся к игроку
        if (agent != null)
        {
            agent.isStopped = true;
        }
        
        // Поворачиваем NPC к игроку
        if (player != null)
        {
            transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
            Debug.Log($"[{name}] Повернулся к игроку для диалога");
        }
        
        // Запускаем анимацию разговора, если она есть
        if (animator != null)
        {
            try 
            {
                // Проверяем наличие параметра в аниматоре
                if (HasParameter("isTalking"))
                {
                    animator.SetBool("isTalking", true);
                }
                else
                {
                    Debug.LogWarning($"[{name}] Аниматор не содержит параметр 'isTalking'");
                    // Пробуем использовать другой параметр, который может быть в аниматоре
                    if (HasParameter("Talk"))
                        animator.SetBool("Talk", true);
                    else if (HasParameter("Talking"))
                        animator.SetBool("Talking", true);
                    else if (HasParameter("talk"))
                        animator.SetTrigger("talk");
                    else
                        Debug.LogWarning($"[{name}] Не найдены подходящие параметры для анимации разговора");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{name}] Ошибка при установке параметра аниматора: {e.Message}");
            }
        }
        
        // Проверяем, связан ли этот NPC с квестами
        QuestNPC questNPC = GetComponent<QuestNPC>();
        if (questNPC != null)
        {
            // Используем метод взаимодействия из QuestNPC
            questNPC.Interact();
        }
        else
        {
            // Используем стандартный диалог
            Debug.Log($"[{name}] Запускаем диалог через DialogueManager");
            // Убеждаемся, что диалог не null перед вызовом
            if (dialogueData != null && dialogueManager != null)
            {
                dialogueManager.StartDialogue(dialogueData, this);
            }
            else
            {
                Debug.LogError($"[{name}] Не удалось запустить диалог: dialogueData={dialogueData != null}, dialogueManager={dialogueManager != null}");
                isInteracting = false; // Сбрасываем флаг взаимодействия, если диалог не удалось запустить
            }
        }
    }

    public void EndInteraction()
    {
        isInteracting = false;
        
        // Возобновляем патрулирование
        if (agent != null)
        {
            agent.isStopped = false;
        }
        
        // Останавливаем анимацию разговора
        if (animator != null)
        {
            try
            {
                if (HasParameter("isTalking"))
                    animator.SetBool("isTalking", false);
                else if (HasParameter("Talk"))
                    animator.SetBool("Talk", false);
                else if (HasParameter("Talking"))
                    animator.SetBool("Talking", false);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{name}] Ошибка при сбросе параметра аниматора: {e.Message}");
            }
        }
        
        // Если NPC патрулирует, возобновляем патрулирование
        if (canPatrol && !isChasing)
        {
            StartCoroutine(Patrol());
        }
    }

    private IEnumerator Patrol()
    {
        if (patrolPoints.Length == 0 || agent == null) yield break;
        
        isPatrolling = true;
        
        while (isPatrolling && !isInteracting && !isChasing)
        {
            // Устанавливаем точку назначения
            agent.SetDestination(patrolPoints[currentPatrolPoint].position);
            
            // Ждем, пока NPC достигнет точки
            while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance)
            {
                if (isInteracting || isChasing)
                {
                    isPatrolling = false;
                    yield break;
                }
                yield return null;
            }
            
            // Ждем в точке патрулирования
            yield return new WaitForSeconds(patrolWaitTime);
            
            // Переходим к следующей точке
            currentPatrolPoint = (currentPatrolPoint + 1) % patrolPoints.Length;
        }
        
        isPatrolling = false;
    }

    private void CheckForEnemies()
    {
        if (detectionRadius <= 0 || !canPatrol) return;
        
        // Ищем врагов в радиусе обнаружения
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        
        if (hitColliders.Length > 0)
        {
            // Берем первого найденного врага
            targetEnemy = hitColliders[0].gameObject;
            
            // Прекращаем патрулирование и начинаем преследование
            StopCoroutine(Patrol());
            isPatrolling = false;
            isChasing = true;
            StartCoroutine(ChaseEnemy());
        }
    }

    private IEnumerator ChaseEnemy()
    {
        if (agent == null || targetEnemy == null)
        {
            isChasing = false;
            yield break;
        }
        
        while (isChasing && targetEnemy != null && !isInteracting)
        {
            // Преследуем врага
            agent.SetDestination(targetEnemy.transform.position);
            
            // Проверяем дистанцию до врага
            float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.transform.position);
            
            // Если враг слишком далеко, прекращаем преследование
            if (distanceToEnemy > detectionRadius * 1.5f)
            {
                isChasing = false;
                targetEnemy = null;
                
                // Возобновляем патрулирование
                StartCoroutine(Patrol());
                yield break;
            }
            
            // Если враг близко, атакуем
            if (distanceToEnemy <= agent.stoppingDistance + 0.5f)
            {
                // Поворачиваемся к врагу
                transform.LookAt(new Vector3(targetEnemy.transform.position.x, transform.position.y, targetEnemy.transform.position.z));
                
                // Запускаем анимацию атаки
                if (animator != null)
                {
                    animator.SetTrigger("attack");
                }
                
                // Наносим урон врагу
                Enemy enemy = targetEnemy.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(20f); // Урон от дружественного NPC
                }
                
                // Ждем перед следующей атакой
                yield return new WaitForSeconds(1.5f);
            }
            
            yield return null;
        }
        
        isChasing = false;
    }

    public string GetNPCName()
    {
        return npcName;
    }

    // Добавляем метод для скрытия подсказки взаимодействия
    public void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    // Метод для корректировки позиции модели NPC
    private void FixModelPosition()
    {
        // Проверяем, есть ли у NPC дочерние объекты с моделью
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (renderers.Length > 0)
        {
            // Находим все меши модели
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                // Перемещаем модель вниз, чтобы она была на уровне земли
                renderer.transform.localPosition = new Vector3(
                    renderer.transform.localPosition.x,
                    0f, // Сбрасываем Y в 0 для локальной позиции
                    renderer.transform.localPosition.z
                );
            }
        }
        
        // Также проверяем обычные меши
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers.Length > 0)
        {
            foreach (MeshRenderer renderer in meshRenderers)
            {
                // Если это не часть подсказки взаимодействия
                if (interactionPrompt == null || !renderer.transform.IsChildOf(interactionPrompt.transform))
                {
                    renderer.transform.localPosition = new Vector3(
                        renderer.transform.localPosition.x,
                        0f,
                        renderer.transform.localPosition.z
                    );
                }
            }
        }
    }

    // Вспомогательный метод для проверки наличия параметра в аниматоре
    private bool HasParameter(string paramName)
    {
        if (animator == null)
            return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
} 