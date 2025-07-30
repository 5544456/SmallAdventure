using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Animations; // Для доступа к AnimatorController
#endif

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTime = 2f;

    [Header("Ragdoll Components")]
    [Tooltip("Все Rigidbody, участвующие в ragdoll")] 
    public Rigidbody[] ragdollRigidbodies;
    [Tooltip("Все Collider, участвующие в ragdoll")] 
    public Collider[] ragdollColliders;
    
    [Header("Finisher Settings")]
    [SerializeField] private float finisherHealthThreshold = 20f; // Порог здоровья для добивания
    [SerializeField] private float finisherPositionSpeed = 10f; // Скорость перемещения при добивании
    
    [Header("Blood Effects")]
    [SerializeField] private GameObject bloodEffectPrefab; // Префаб эффекта крови
    [SerializeField] private Transform[] bloodSpawnPoints; // Точки для спавна эффектов крови
    [SerializeField] private GameObject[] bloodDecalPrefabs; // Префабы декалей крови
    [SerializeField] private int maxDecals = 5; // Максимальное количество декалей
    [SerializeField] private float bloodEffectDuration = 5f; // Длительность эффекта крови

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private float currentHealth;
    private float lastAttackTime;
    private int currentPatrolIndex;
    private bool isWaiting;
    private bool isDead;
    private bool isAttacking;
    private bool isBeingFinished = false;
    private bool isStunned = false; // Флаг для отслеживания состояния оглушения
    private float stunEndTime = 0f; // Время окончания оглушения
    private float hitAnimationDuration = 0.8f; // Длительность анимации получения урона
    private List<GameObject> activeDecals = new List<GameObject>();

    // Публичное свойство для доступа к текущему здоровью
    public float CurrentHealth => currentHealth;

    // Публичное свойство для проверки, мертв ли враг
    public bool IsDead => isDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Убедимся, что у врага правильный тег и слой
        EnsureEnemyTagAndLayer();
        
        currentHealth = maxHealth;
        agent.speed = patrolSpeed;
        
        if (patrolPoints.Length > 0)
        {
            StartCoroutine(Patrol());
        }
        SetRagdollActive(false);
        
        // Проверяем длительность анимации Hit в аниматоре
        if (animator != null)
        {
            CheckHitAnimationDuration();
        }
    }

    // Метод для установки правильного тега и слоя
    private void EnsureEnemyTagAndLayer()
    {
        // Проверяем и устанавливаем тег Enemy
        if (!gameObject.CompareTag("Enemy"))
        {
            gameObject.tag = "Enemy";
            Debug.Log($"[{name}] Установлен тег 'Enemy'");
        }
        
        // Проверяем и устанавливаем слой Enemy
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1 && gameObject.layer != enemyLayer)
        {
            gameObject.layer = enemyLayer;
            Debug.Log($"[{name}] Установлен слой 'Enemy'");
        }
        else if (enemyLayer == -1)
        {
            Debug.LogWarning($"[{name}] Слой 'Enemy' не существует в проекте!");
        }
    }

    private void Update()
    {
        if (isDead || player == null || isBeingFinished) return;
        
        // Проверяем, не оглушен ли враг
        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
                Debug.Log($"[{name}] Враг вышел из состояния оглушения");
            }
            else
            {
                // Враг оглушен, пропускаем остальную логику
                return;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
            if (distanceToPlayer <= attackRange && !isAttacking)
            {
                // В зоне атаки — обычная ходьба
                agent.speed = patrolSpeed;
                if (animator != null)
                    animator.SetBool("isSprinting", false);
                Attack();
            }
        }
        else if (!isWaiting && patrolPoints.Length > 0)
        {
            agent.speed = patrolSpeed;
            if (animator != null)
                animator.SetBool("isSprinting", false);
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                isWaiting = true;
                StartCoroutine(WaitAndGoToNextPatrolPoint());
            }
        }

        // --- Анимация ---
        if (animator != null)
        {
            bool moving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isMoving", moving);
        }
    }

    private void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
        
        // Поворачиваем врага к игроку
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        // Включаем анимацию бега
        if (animator != null)
            animator.SetBool("isSprinting", true);
    }

    private void Attack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("attack");
        }

        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(0.5f);

        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            Player playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.TakeDamage(attackDamage);
            }
        }

        isAttacking = false;
    }

    private IEnumerator Patrol()
    {
        while (!isDead)
        {
            if (patrolPoints.Length == 0) yield break;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            while (!agent.pathPending && agent.remainingDistance > 0.5f)
            {
                yield return null;
            }
            isWaiting = true;
            yield return new WaitForSeconds(waitTime);
            isWaiting = false;
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private IEnumerator WaitAndGoToNextPatrolPoint()
    {
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Уменьшаем здоровье
        currentHealth -= damage;
        Debug.Log($"[{name}] Получил {damage} урона. Текущее здоровье: {currentHealth}/{maxHealth}");

        // Оглушаем врага на время анимации получения урона
        isStunned = true;
        stunEndTime = Time.time + hitAnimationDuration;
        Debug.Log($"[{name}] Враг оглушен на {hitAnimationDuration} секунд");

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        // Добавляем декаль крови на модель при получении урона
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f) && hit.collider.gameObject == gameObject)
        {
            ApplyBloodDecal(hit.point, Quaternion.LookRotation(-hit.normal));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Проверка, можно ли добить врага
    public bool CanBeFinished()
    {
        // Проверяем здоровье и что враг не мертв
        bool healthCheck = !isDead && currentHealth <= finisherHealthThreshold;
        
        Debug.Log($"[{name}] CanBeFinished: isDead={isDead}, currentHealth={currentHealth}/{maxHealth}, " +
                  $"threshold={finisherHealthThreshold}, result={healthCheck}");
        
        // Дополнительная проверка для отладки
        if (!healthCheck)
        {
            if (isDead)
            {
                Debug.Log($"[{name}] Враг уже мертв, добивание невозможно");
            }
            else if (currentHealth > finisherHealthThreshold)
            {
                Debug.Log($"[{name}] Здоровье врага ({currentHealth}) выше порога добивания ({finisherHealthThreshold})");
            }
        }
        else
        {
            Debug.Log($"[{name}] Враг готов к добиванию!");
        }
        
        // Враг может быть добит, если у него мало здоровья и он не мертв
        return healthCheck;
    }
    
    // Подготовка врага к добиванию
    public void PrepareForFinisher(Vector3 targetPosition)
    {
        if (isDead) return;
        
        isBeingFinished = true;
        isStunned = true; // Оглушаем врага на время добивания
        stunEndTime = Time.time + 10f; // Долгое оглушение, чтобы враг не вышел из него во время добивания
        
        // Отключаем навигацию и физику
        if (agent != null && agent.enabled) agent.enabled = false;
        
        // Запускаем анимацию получения урона или специальную анимацию для добивания
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        // Перемещаем врага в позицию добивания
        StartCoroutine(MoveToFinisherPosition(targetPosition));
    }
    
    // Метод для настройки длительности анимации получения урона
    public void SetHitAnimationDuration(float duration)
    {
        if (duration > 0)
        {
            hitAnimationDuration = duration;
            Debug.Log($"[{name}] Длительность анимации получения урона установлена на {hitAnimationDuration} секунд");
        }
        else
        {
            Debug.LogWarning($"[{name}] Попытка установить некорректную длительность анимации получения урона: {duration}");
        }
    }
    
    private IEnumerator MoveToFinisherPosition(Vector3 targetPosition)
    {
        float startTime = Time.time;
        Vector3 startPosition = transform.position;
        
        // Плавно перемещаем врага в позицию добивания
        while (Time.time < startTime + 0.2f) // 0.2 секунды на перемещение
        {
            float t = (Time.time - startTime) / 0.2f;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            // Поворачиваем врага к игроку
            if (player != null)
            {
                Vector3 lookDir = player.position - transform.position;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, 
                        Quaternion.LookRotation(lookDir), t);
                }
            }
            
            yield return null;
        }
        
        // Фиксируем конечную позицию
        transform.position = targetPosition;
    }
    
    // Убийство врага при добивании
    public void FinisherKill()
    {
        if (isDead) return;
        
        // Спавним эффект крови в точке добивания
        if (bloodSpawnPoints != null && bloodSpawnPoints.Length > 0)
        {
            SpawnBloodEffectAtPoint(0); // Используем первую точку для эффекта
        }
        else
        {
            // Если точки не заданы, спавним эффект в центре врага
            SpawnBloodEffect(transform.position + Vector3.up);
        }
        
        // Добавляем декали крови на модель
        ApplyMultipleBloodDecals(3);
        
        // Убиваем врага
        Die();
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log($"[{name}] Враг убит!");
        
        // Отключаем NavMeshAgent
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        // Отключаем основной BoxCollider
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null) box.enabled = false;
        
        // Включаем ragdoll
        SetRagdollActive(true);
        
        // Отключаем аниматор
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Добавляем золото игроку
        var buildSystem = FindFirstObjectByType<BuildingSystem>();
        if (buildSystem != null) buildSystem.AddGold(20);
        
        // Уведомляем систему уровней игрока о смерти врага
        if (player != null)
        {
            PlayerLevelSystem levelSystem = player.GetComponent<PlayerLevelSystem>();
            if (levelSystem != null)
            {
                levelSystem.OnEnemyKilled();
                Debug.Log($"[{name}] Игрок получил опыт за убийство врага");
            }
        }
        
        // Уничтожаем объект через 10 секунд
        Destroy(gameObject, 10f);
    }

    private void SetRagdollActive(bool active)
    {
        if (ragdollRigidbodies != null)
        {
            foreach (var rb in ragdollRigidbodies)
            {
                rb.isKinematic = !active;
            }
        }
        if (ragdollColliders != null)
        {
            foreach (var col in ragdollColliders)
            {
                col.enabled = active;
            }
        }
        if (animator != null)
            animator.enabled = !active;
        if (agent != null)
            agent.enabled = !active;
    }
    
    // Спавн эффекта крови в указанной точке
    public void SpawnBloodEffect(Vector3 hitPoint)
    {
        if (bloodEffectPrefab != null)
        {
            // Спавним эффект крови в точке попадания
            GameObject bloodEffect = Instantiate(bloodEffectPrefab, hitPoint, Quaternion.identity);
            
            // Направляем частицы от точки удара
            ParticleSystem ps = bloodEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Направление от игрока к врагу
                Vector3 direction = (hitPoint - Camera.main.transform.position).normalized;
                bloodEffect.transform.rotation = Quaternion.LookRotation(direction);
            }
            
            // Автоматически уничтожаем эффект через некоторое время
            Destroy(bloodEffect, bloodEffectDuration);
        }
    }
    
    // Спавн эффекта крови в заданной точке (из массива точек)
    public void SpawnBloodEffectAtPoint(int pointIndex)
    {
        if (bloodEffectPrefab != null && bloodSpawnPoints != null && 
            pointIndex >= 0 && pointIndex < bloodSpawnPoints.Length)
        {
            GameObject bloodEffect = Instantiate(bloodEffectPrefab, 
                bloodSpawnPoints[pointIndex].position, 
                bloodSpawnPoints[pointIndex].rotation);
                
            Destroy(bloodEffect, bloodEffectDuration);
        }
    }
    
    // Добавление декаля крови на модель
    public void ApplyBloodDecal(Vector3 position, Quaternion rotation)
    {
        if (bloodDecalPrefabs == null || bloodDecalPrefabs.Length == 0) return;
        
        // Выбираем случайный декаль из массива
        GameObject decalPrefab = bloodDecalPrefabs[Random.Range(0, bloodDecalPrefabs.Length)];
        
        // Создаем декаль
        GameObject decal = Instantiate(decalPrefab, position, rotation);
        
        // Привязываем декаль к телу врага, чтобы он двигался вместе с ним
        decal.transform.SetParent(transform);
        
        // Добавляем в список активных декалей
        activeDecals.Add(decal);
        
        // Если декалей слишком много, удаляем самый старый
        if (activeDecals.Count > maxDecals)
        {
            GameObject oldestDecal = activeDecals[0];
            activeDecals.RemoveAt(0);
            Destroy(oldestDecal);
        }
    }
    
    // Добавление нескольких декалей крови на модель
    public void ApplyMultipleBloodDecals(int count)
    {
        if (bloodDecalPrefabs == null || bloodDecalPrefabs.Length == 0) return;
        
        for (int i = 0; i < count; i++)
        {
            // Выбираем случайную точку на поверхности врага
            Collider mainCollider = GetComponent<Collider>();
            if (mainCollider == null) return;
            
            // Получаем случайную точку на поверхности коллайдера
            Vector3 randomPoint = new Vector3(
                Random.Range(mainCollider.bounds.min.x, mainCollider.bounds.max.x),
                Random.Range(mainCollider.bounds.min.y, mainCollider.bounds.max.y),
                Random.Range(mainCollider.bounds.min.z, mainCollider.bounds.max.z)
            );
            
            // Находим ближайшую точку на поверхности коллайдера
            Vector3 surfacePoint = mainCollider.ClosestPoint(randomPoint);
            
            // Определяем нормаль в этой точке (примерно)
            Vector3 normal = (surfacePoint - transform.position).normalized;
            
            // Применяем декаль
            ApplyBloodDecal(surfacePoint, Quaternion.LookRotation(-normal));
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Отображаем радиус обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Отображаем радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Отображаем точки патрулирования
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.3f);
                }
            }
        }
        
        // Отображаем точки спавна крови
        if (bloodSpawnPoints != null && bloodSpawnPoints.Length > 0)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in bloodSpawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.1f);
                    Gizmos.DrawRay(point.position, point.forward * 0.3f);
                }
            }
        }
    }

    // Метод для проверки длительности анимации Hit в аниматоре
    private void CheckHitAnimationDuration()
    {
        if (animator == null) return;
        
#if UNITY_EDITOR
        // Проверяем наличие состояния Hit в аниматоре
        AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
        if (controller != null)
        {
            try
            {
                foreach (AnimatorControllerLayer layer in controller.layers)
                {
                    AnimatorStateMachine stateMachine = layer.stateMachine;
                    foreach (ChildAnimatorState state in stateMachine.states)
                    {
                        if (state.state.name.Contains("Hit") || state.state.name.Contains("Hurt") || state.state.name.Contains("Damage"))
                        {
                            // Получаем длительность анимации из клипа
                            AnimationClip clip = state.state.motion as AnimationClip;
                            if (clip != null)
                            {
                                hitAnimationDuration = clip.length;
                                Debug.Log($"[{name}] Найдена анимация получения урона '{state.state.name}' с длительностью {hitAnimationDuration} секунд");
                                return;
                            }
                        }
                    }
                }
                
                Debug.LogWarning($"[{name}] Не удалось найти анимацию получения урона в аниматоре. Используется значение по умолчанию: {hitAnimationDuration} секунд");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{name}] Ошибка при проверке анимации получения урона: {e.Message}");
            }
        }
        else
        {
            TryGetAnimationDurationFromRuntimeController();
        }
#else
        TryGetAnimationDurationFromRuntimeController();
#endif
    }
    
    // Метод для получения длительности анимации из RuntimeAnimatorController
    private void TryGetAnimationDurationFromRuntimeController()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        
        Debug.Log($"[{name}] Поиск анимации получения урона в RuntimeAnimatorController");
        
        // Получаем все клипы из контроллера
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        
        // Ищем анимацию получения урона по имени
        foreach (AnimationClip clip in clips)
        {
            if (clip != null && (clip.name.Contains("Hit") || clip.name.Contains("Hurt") || clip.name.Contains("Damage")))
            {
                hitAnimationDuration = clip.length;
                Debug.Log($"[{name}] Найдена анимация получения урона '{clip.name}' с длительностью {hitAnimationDuration} секунд");
                return;
            }
        }
        
        Debug.LogWarning($"[{name}] Не удалось найти анимацию получения урона. Используется значение по умолчанию: {hitAnimationDuration} секунд");
    }
} 