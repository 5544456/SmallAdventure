using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Настройки здоровья")]
    [SerializeField] private int maxHealth = 100;           // Максимальное здоровье
    [SerializeField] private int currentHealth;             // Текущее здоровье
    [SerializeField] private float invulnerabilityTime = 1f; // Время неуязвимости после получения урона

    [Header("UI элементы")]
    [SerializeField] private Image healthBar;                // Ссылка на полоску здоровья (если есть)
    [SerializeField] private Text healthText;                // Текстовое отображение здоровья (если есть)

    [Header("Эффекты")]
    [SerializeField] private GameObject damageEffect;        // Эффект получения урона
    [SerializeField] private GameObject deathEffect;         // Эффект смерти

    private bool isInvulnerable = false;                     // Флаг неуязвимости
    private Animator animator;                               // Компонент аниматора

    // События
    public delegate void PlayerDeathHandler();
    public event PlayerDeathHandler OnPlayerDeath;

    public delegate void PlayerDamageHandler(int damage);
    public event PlayerDamageHandler OnPlayerDamage;
    public GameObject Death;

    // Свойство для доступа к текущему здоровью
    public int Health
    {
        get { return currentHealth; }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Инициализация здоровья
        currentHealth = maxHealth;

        // Обновление UI
        UpdateHealthUI();
    }

    // Метод получения урона (публичный метод)
    public void TakeDamage(int damage)
    {
        // Проверка на неуязвимость
        if (isInvulnerable)
            return;

        // Применение урона
        currentHealth -= damage;

        // Вызов события получения урона
        if (OnPlayerDamage != null)
            OnPlayerDamage(damage);

        // Проверка на смерть
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            // Запуск неуязвимости
            StartCoroutine(InvulnerabilityTimer());

            // Воспроизведение анимации получения урона
            if (animator != null)
                animator.SetTrigger("Hit");

            // Отображение эффекта получения урона
            if (damageEffect != null)
                Instantiate(damageEffect, transform.position, Quaternion.identity);
        }

        // Обновление UI
        UpdateHealthUI();
    }

    // Метод лечения (публичный метод)
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        // Обновление UI
        UpdateHealthUI();
    }

    // Таймер неуязвимости
    private IEnumerator InvulnerabilityTimer()
    {
        isInvulnerable = true;

        // Здесь можете добавить визуальное отображение неуязвимости
        // Например, мигание спрайта

        yield return new WaitForSeconds(invulnerabilityTime);

        isInvulnerable = false;
    }

    // Обработка смерти
    private void Die()
    {
        // Вызов события смерти
        if (OnPlayerDeath != null) 
            OnPlayerDeath();

        // Воспроизведение анимации смерти
        if (animator != null)
            animator.SetTrigger("Death");

        // Отображение эффекта смерти
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Отключение управления игроком
        // Например, отключение компонента передвижения
        GetComponent<Player>().enabled = false;

        // Здесь можно добавить дополнительную логику смерти
        // Например, показ экрана окончания игры после некоторой задержки
    }

    // Обновление UI элементов
    private void UpdateHealthUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = (float)currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = $"Здоровье: {currentHealth}/{maxHealth}";
    }

    // Метод для проверки, жив ли игрок
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    // ����� ������ �������� (��������, ��� �����������)
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }
}