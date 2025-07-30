using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundMask;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float healthRegenRate = 1f;
    [SerializeField] private float healthRegenDelay = 5f;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 10f;
    [SerializeField] private float staminaRegenRate = 5f;
    [SerializeField] private float staminaRegenDelay = 2f;

    [Header("Combat Settings")]
    [SerializeField] private float parryWindow = 0.2f;
    [SerializeField] private float parryCooldown = 1f;
    [SerializeField] private float parryStaminaCost = 20f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 0.5f; // Задержка между атаками
    [SerializeField] private float attackDamage = 50f;
    
    [Header("Finisher Settings")]
    [SerializeField] private Transform finisherPosition; // Пустой объект, указывающий позицию врага при добивании
    [SerializeField] private float finisherRange = 2f; // Максимальное расстояние для добивания
    [SerializeField] private float finisherStaminaCost = 25f; // Затраты выносливости на добивание
    [SerializeField] private float finisherAnimationDuration = 2f; // Длительность анимации добивания

    private CharacterController controller;
    private Transform cameraTransform;
    private Vector3 velocity;
    private float currentSpeed;
    private float currentHealth;
    private float currentStamina;
    private float lastDamageTime;
    private float lastStaminaUseTime;
    private float lastParryTime;
    private float lastAttackTime; // Время последней атаки
    private bool isGrounded;
    private bool isSprinting;
    private bool isParrying;
    private bool isCrouching;
    private float verticalRotation = 0f;
    private Animator animator;
    private InventoryManager inventory;
    private Vector3 lastPosition;
    private bool isBlocking = false;
    private bool isBuildMenuOpen = false;
    private bool isFinishing = false;
    public Slider healthBar;
    public Slider staminaBar;
    public bool isInputBlocked = false;

    public float PlayerHealth => currentHealth;
    public float PlayerStamina => currentStamina;

    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<float> OnStaminaChanged;
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnParrySuccess;
    public UnityEvent OnParryFail;
    public UnityEvent OnAttack;
    public UnityEvent<bool> OnInventoryToggled;
    public UnityEvent OnFinisherStarted;
    public UnityEvent OnFinisherCompleted;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        animator = GetComponent<Animator>();
        inventory = FindFirstObjectByType<InventoryManager>();
        
        // Добавляем отладку аниматора
        if (animator != null)
        {
            Debug.Log($"[Player] Аниматор найден: {animator.name}");
            if (animator.runtimeAnimatorController != null)
            {
                Debug.Log($"[Player] RuntimeAnimatorController найден: {animator.runtimeAnimatorController.name}");
                
                // Проверяем параметры аниматора
                Debug.Log($"[Player] Параметры аниматора:");
                foreach (var param in animator.parameters)
                {
                    Debug.Log($"[Player] - {param.name} (тип: {param.type}, значение по умолчанию: {param.defaultBool})");
                }
                
                
            }
            else
            {
                Debug.LogError("[Player] RuntimeAnimatorController не найден!");
            }
        }
        else
        {
            Debug.LogError("[Player] Аниматор не найден!");
        }
        
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentSpeed = walkSpeed;
        lastParryTime = -parryCooldown;
        lastPosition = transform.position;

        // Проверяем наличие точки для добивания
        if (finisherPosition == null)
        {
            Debug.LogWarning("Точка для добивания (finisherPosition) не назначена! Создаем временную точку...");
            GameObject finisherPosObj = new GameObject("FinisherPosition");
            finisherPosObj.transform.parent = transform;
            finisherPosObj.transform.localPosition = new Vector3(0, 0, 1.5f); // 1.5 метра перед игроком
            finisherPosition = finisherPosObj.transform;
        }

        // Блокируем и скрываем курсор только если нет открытых UI
        var buildSystem = FindFirstObjectByType<BuildingSystem>();
        if (buildSystem == null || !buildSystem.IsAnyUIOpen())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            BlockInput(false);
        }
        
        // Инициализация событий, если они не были инициализированы
        if (OnHealthChanged == null)
            OnHealthChanged = new UnityEvent<float>();
        if (OnStaminaChanged == null)
            OnStaminaChanged = new UnityEvent<float>();
        if (OnPlayerDeath == null)
            OnPlayerDeath = new UnityEvent();
        if (OnParrySuccess == null)
            OnParrySuccess = new UnityEvent();
        if (OnParryFail == null)
            OnParryFail = new UnityEvent();
        if (OnAttack == null)
            OnAttack = new UnityEvent();
        if (OnInventoryToggled == null)
            OnInventoryToggled = new UnityEvent<bool>();
        if (OnFinisherStarted == null)
            OnFinisherStarted = new UnityEvent();
        if (OnFinisherCompleted == null)
            OnFinisherCompleted = new UnityEvent();
    }

    private void Update()
    {
        if (isInputBlocked) return;
        if (isBuildMenuOpen) return;
        
        // Проверяем, не активен ли диалог
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null && dialogueManager.IsDialogueActive())
        {
            return;
        }
        
        HandleMovement();
        HandleCameraRotation();
        HandleHealth();
        HandleStamina();
        HandleParry();
        HandleAttack();
        HandleInventory();
        HandleFinisher(); // Добавляем обработку добивания
        UpdateAnimations();
        UpdateUIBars();
        isBlocking = Input.GetMouseButton(1);
    }

    private void HandleCameraRotation()
    {
        if (isInputBlocked) return;
        // Горизонтальное вращение (поворот персонажа)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // Вертикальное вращение (наклон камеры)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        // Проверка на землю с отладкой
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f, groundMask);
        Debug.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * 0.3f, isGrounded ? Color.green : Color.red);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            if (animator != null)
                animator.SetBool("isJumping", false);
        }

        // Получаем входные данные
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Обработка движения
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + transform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching && currentStamina > 0;
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
            if (isCrouching) currentSpeed *= 0.5f;

            controller.Move(moveDir * currentSpeed * Time.deltaTime);

            if (isSprinting)
            {
                currentStamina = Mathf.Max(0f, currentStamina - staminaDrainRate * Time.deltaTime);
                lastStaminaUseTime = Time.time;
            }
        }
        else
        {
            isSprinting = false;
        }

        // Обработка прыжка
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            Debug.Log($"Прыжок! isGrounded={isGrounded}, velocity.y={velocity.y}");
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            if (animator != null)
                animator.SetBool("isJumping", true);
        }

        // Обработка приседания
        HandleCrouch();

        // Применяем гравитацию
        velocity.y += gravity * Time.deltaTime;

        // Применяем вертикальное движение
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))  // Меняем на клавишу C
        {
            isCrouching = !isCrouching;
            
            // Сохраняем текущую позицию
            Vector3 currentPos = transform.position;
            
            // Изменяем высоту контроллера
            controller.height = isCrouching ? 1f : 2f;
            controller.center = new Vector3(0, isCrouching ? 0.5f : 1f, 0);
            
            // Корректируем позицию, чтобы избежать проблем с коллизией
            if (!isCrouching)
            {
                // Проверяем, можем ли мы встать
                if (!Physics.Raycast(currentPos, Vector3.up, 2f, groundMask))
                {
                    transform.position = currentPos;
                }
                else
                {
                    // Если не можем встать, отменяем приседание
                    isCrouching = true;
                    controller.height = 1f;
                    controller.center = new Vector3(0, 0.5f, 0);
                }
            }
        }
    }

    private void HandleHealth()
    {
        if (Time.time - lastDamageTime >= healthRegenDelay && currentHealth < maxHealth)
        {
            currentHealth = Mathf.Min(currentHealth + healthRegenRate * Time.deltaTime, maxHealth);
            OnHealthChanged?.Invoke(currentHealth / maxHealth);
        }
    }

    private void HandleStamina()
    {
        if (!isSprinting && Time.time - lastStaminaUseTime >= staminaRegenDelay && currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
            OnStaminaChanged?.Invoke(currentStamina / maxStamina);
        }
    }

    private void HandleParry()
    {
        if (Input.GetMouseButtonDown(1)) // Правая кнопка мыши для парирования
        {
            if (Time.time - lastParryTime >= parryCooldown && currentStamina >= parryStaminaCost)
            {
                StartParry();
            }
        }
    }

    private void StartParry()
    {
        isParrying = true;
        lastParryTime = Time.time;
        currentStamina -= parryStaminaCost;
        OnStaminaChanged?.Invoke(currentStamina / maxStamina);
        
        // Отключаем парирование через parryWindow секунд
        StartCoroutine(EndParryAfterDelay());
    }

    private System.Collections.IEnumerator EndParryAfterDelay()
    {
        yield return new WaitForSeconds(parryWindow);
        isParrying = false;
    }

    public bool TryParry()
    {
        if (isParrying)
        {
            OnParrySuccess?.Invoke();
            isParrying = false;
            return true;
        }
        
        OnParryFail?.Invoke();
        return false;
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && !isBlocking && !isParrying && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            
            // Проверяем, не активен ли диалог
            DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
            if (dialogueManager != null && dialogueManager.IsDialogueActive())
            {
                return;
            }
            
            // Запускаем анимацию атаки
            if (animator != null)
            {
                Debug.Log("[Player] Установка триггера attack");
                animator.SetTrigger("attack");
            }
            else
            {
                Debug.LogWarning("[Player] Аниматор не найден в методе HandleAttack!");
            }
            
            // Вызываем событие атаки
            OnAttack?.Invoke();
            
            // Проверяем, есть ли враги в радиусе атаки
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, attackRange))
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Debug.Log($"[Player] Атака попала по врагу {enemy.name}");
                    enemy.TakeDamage(attackDamage);
                    
                    // Спавним эффект крови в точке попадания
                    enemy.SpawnBloodEffect(hit.point);
                }
            }
        }
    }
    
    private void HandleFinisher()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Нажата клавиша F для добивания");
            
            if (isFinishing)
            {
                Debug.Log("Добивание уже выполняется, пропускаем");
                return;
            }
            
            if (currentStamina < finisherStaminaCost)
            {
                Debug.Log($"Недостаточно выносливости для добивания: {currentStamina}/{finisherStaminaCost}");
                return;
            }
            
            // Ищем всех врагов в сцене
            Enemy[] allEnemies = FindObjectsOfType<Enemy>();
            Debug.Log($"Найдено врагов в сцене: {allEnemies.Length}");
            
            Enemy targetEnemy = null;
            float closestDistance = finisherRange;
            
            // Перебираем всех врагов
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy == null) continue;
                
                // Проверяем расстояние до врага
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                
                // Если враг в пределах дистанции добивания и его можно добить
                if (distance <= finisherRange && enemy.CanBeFinished())
                {
                    Debug.Log($"Найден подходящий враг для добивания: {enemy.name}, дистанция: {distance}");
                    
                    // Выбираем ближайшего врага
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetEnemy = enemy;
                    }
                }
                else
                {
                    Debug.Log($"Враг {enemy.name} не подходит для добивания: дистанция={distance}, можно добить={enemy.CanBeFinished()}");
                }
            }
            
            if (targetEnemy != null)
            {
                Debug.Log($"Начинаем добивание врага {targetEnemy.name}");
                StartCoroutine(PerformFinisher(targetEnemy));
            }
            else
            {
                Debug.Log("Подходящих врагов для добивания не найдено");
            }
        }
    }

    private IEnumerator PerformFinisher(Enemy enemy)
    {
        isFinishing = true;
        isInputBlocked = true;
        
        Debug.Log($"Начинаем добивание врага {enemy.name}, HP: {enemy.CurrentHealth}");
        
        // Расходуем выносливость
        currentStamina -= finisherStaminaCost;
        OnStaminaChanged?.Invoke(currentStamina / maxStamina);
        
        // Сохраняем начальные позиции
        Vector3 initialPlayerPos = transform.position;
        Vector3 initialEnemyPos = enemy.transform.position;
        
        // Разворачиваем игрока к врагу, учитывая только горизонтальное направление
        Vector3 dirToEnemy = enemy.transform.position - transform.position;
        dirToEnemy.y = 0; // Игнорируем вертикальную составляющую
        dirToEnemy = dirToEnemy.normalized;
        
        // Используем направление взгляда камеры для более точного поворота
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0; // Игнорируем вертикальную составляющую
        cameraForward = cameraForward.normalized;
        
        // Плавно поворачиваем игрока к врагу
        float rotationSpeed = 10f;
        float startTime = Time.time;
        float duration = 0.2f;
        
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(dirToEnemy);
        
        // Быстрый поворот к врагу
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }
        
        // Фиксируем окончательный поворот
        transform.rotation = targetRotation;
        
        Debug.Log($"Игрок повернулся к врагу: {transform.eulerAngles}");
        
        // Запускаем анимацию добивания
        if (animator != null)
        {
            Debug.Log("Запускаем анимацию добивания");
            
            // Проверяем наличие параметра "finisher" в аниматоре
            AnimatorControllerParameter[] parameters = animator.parameters;
            bool hasFinisherParam = false;
            foreach (var param in parameters)
            {
                if (param.name == "finisher")
                {
                    hasFinisherParam = true;
                    break;
                }
            }
            
            if (hasFinisherParam)
            {
                Debug.Log("[Player] Установка триггера finisher");
                animator.SetTrigger("finisher");
                Debug.Log("Триггер 'finisher' установлен в аниматоре");
            }
            else
            {
                Debug.LogError("Аниматор не содержит параметр 'finisher'! Добивание может работать некорректно.");
            }
        }
        else
        {
            Debug.LogWarning("Аниматор игрока не найден!");
        }
        
        OnFinisherStarted?.Invoke();
        
        // Ждем, пока анимация начнется
        yield return new WaitForSeconds(0.2f);
        
        // Перемещаем врага в нужную позицию относительно игрока
        Vector3 finisherPos;
        if (finisherPosition != null)
        {
            finisherPos = finisherPosition.position;
            Debug.Log($"Перемещаем врага в позицию добивания: {finisherPos}");
        }
        else
        {
            // Если позиция не задана, используем позицию перед игроком
            finisherPos = transform.position + transform.forward * 1.5f;
            Debug.Log($"Позиция для добивания не задана, используем позицию перед игроком: {finisherPos}");
        }
        
        enemy.PrepareForFinisher(finisherPos);
        
        // Ждем окончания анимации
        Debug.Log($"Ожидаем окончания анимации добивания ({finisherAnimationDuration} сек)");
        yield return new WaitForSeconds(finisherAnimationDuration);
        
        // Убиваем врага
        Debug.Log("Завершаем добивание, убиваем врага");
        enemy.FinisherKill();
        
        OnFinisherCompleted?.Invoke();
        
        isFinishing = false;
        isInputBlocked = false;
        Debug.Log("Добивание завершено");
    }

    private void HandleInventory()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // Проверяем, не активен ли диалог
            DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
            if (dialogueManager != null && dialogueManager.IsDialogueActive())
            {
                // Если диалог активен, не открываем инвентарь
                Debug.Log("[Player] Невозможно открыть инвентарь во время диалога");
                return;
            }
            
            if (inventory != null)
            {
                Debug.Log("[Player] Нажата клавиша I, переключаем инвентарь");
                bool wasOpen = inventory.IsInventoryOpen;
                
                // Проверяем, не открыто ли меню строительства
                var buildSystem = FindFirstObjectByType<BuildingSystem>();
                bool buildMenuOpen = (buildSystem != null && buildSystem.isBuildMenuOpen);
                
                // Если меню строительства открыто, сначала закрываем его
                if (buildMenuOpen && !wasOpen)
                {
                    buildSystem.CloseBuildMenu();
                }
                
                // Переключаем инвентарь
                inventory.ToggleInventory();
                bool isInventoryOpen = inventory.IsInventoryOpen;
                
                // Явно управляем курсором и блокировкой ввода здесь
                if (isInventoryOpen)
                {
                    Debug.Log("[Player] Инвентарь открыт, блокируем ввод");
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    isInputBlocked = true;
                    BlockInput(true);
                }
                else
                {
                    Debug.Log("[Player] Инвентарь закрыт");
                    // Проверяем, не открыто ли меню строительства
                    buildMenuOpen = (buildSystem != null && buildSystem.isBuildMenuOpen);
                    
                    if (!buildMenuOpen)
                    {
                        Debug.Log("[Player] Меню строительства не открыто, разблокируем ввод");
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        isInputBlocked = false;
                        BlockInput(false);
                    }
                }
                
                OnInventoryToggled?.Invoke(isInventoryOpen);
            }
            else
            {
                Debug.LogError("InventoryManager не найден!");
            }
        }
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            // Новый способ определения движения
            float moveDelta = (transform.position - lastPosition).magnitude / Time.deltaTime;
            bool isMoving = moveDelta > 0.05f;
            
            // Добавляем отладку
            Debug.Log($"[Player] Установка параметров аниматора: isMoving={isMoving}, isSprinting={isSprinting}");
            
            animator.SetBool("isMoving", isMoving);
            animator.SetBool("isSprinting", isSprinting);
            lastPosition = transform.position;
        }
        else
        {
            Debug.LogWarning("[Player] Аниматор не найден в методе UpdateAnimations!");
        }
    }

    private void UpdateUIBars()
    {
        if (healthBar != null && maxHealth > 0)
            healthBar.value = currentHealth / maxHealth;
        if (staminaBar != null && maxStamina > 0)
            staminaBar.value = currentStamina / maxStamina;
    }

    public void TakeDamage(float damage)
    {
        float finalDamage = damage;
        if (isBlocking)
        {
            finalDamage = Mathf.Max(0, damage - 20f);
        }
        currentHealth = Mathf.Max(0f, currentHealth - finalDamage);
        lastDamageTime = Time.time;
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0f)
        {
            OnPlayerDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    public void SetBuildMenuState(bool state)
    {
        isBuildMenuOpen = state;
    }

    public void BlockInput(bool block)
    {
        isInputBlocked = block;
        if (!block)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    // Метод для увеличения максимального здоровья
    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        Debug.Log($"Максимальное здоровье увеличено на {amount}. Новое значение: {maxHealth}");
        
        // Обновляем UI
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }
    
    // Метод для увеличения максимальной выносливости
    public void IncreaseMaxStamina(float amount)
    {
        maxStamina += amount;
        currentStamina += amount;
        Debug.Log($"Максимальная выносливость увеличена на {amount}. Новое значение: {maxStamina}");
        
        // Обновляем UI
        OnStaminaChanged?.Invoke(currentStamina / maxStamina);
    }
    
    // Для отображения радиуса добивания в редакторе
    private void OnDrawGizmosSelected()
    {
        // Отображаем сферу добивания
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, finisherRange);
        
        // Отображаем сферу атаки
        Gizmos.color = Color.blue;
        if (cameraTransform != null)
        {
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * attackRange);
            Gizmos.DrawWireSphere(cameraTransform.position + cameraTransform.forward * attackRange, 0.1f);
        }
        
        // Отображаем позицию для врага при добивании
        if (finisherPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(finisherPosition.position, 0.2f);
            Gizmos.DrawLine(transform.position, finisherPosition.position);
        }
    }

     
    
} 