using UnityEngine;

/// <summary>
/// Скрипт для декалей крови, которые можно размещать на поверхностях
/// </summary>
public class BloodDecal : MonoBehaviour
{
    [SerializeField] private float fadeTime = 30f; // Время до исчезновения декаля
    [SerializeField] private float fadeDelay = 20f; // Задержка перед началом исчезновения
    [SerializeField] private bool randomizeRotation = true; // Случайный поворот декаля
    [SerializeField] private bool randomizeScale = true; // Случайный размер декаля
    [SerializeField] private float minScale = 0.8f; // Минимальный размер
    [SerializeField] private float maxScale = 1.2f; // Максимальный размер
    
    private Renderer decalRenderer;
    private float startTime;
    private float alpha = 1f;
    private Color originalColor;
    private Material decalMaterial;
    
    private void Awake()
    {
        decalRenderer = GetComponent<Renderer>();
        if (decalRenderer != null && decalRenderer.material != null)
        {
            // Создаем экземпляр материала, чтобы не влиять на другие декали
            decalMaterial = new Material(decalRenderer.material);
            decalRenderer.material = decalMaterial;
            
            // Запоминаем оригинальный цвет
            originalColor = decalMaterial.color;
        }
        
        startTime = Time.time;
    }
    
    private void OnEnable()
    {
        // Сбрасываем время и прозрачность при активации
        startTime = Time.time;
        alpha = 1f;
        
        if (decalRenderer != null && decalMaterial != null)
        {
            decalMaterial.color = originalColor;
        }
        
        // Случайный поворот
        if (randomizeRotation)
        {
            transform.Rotate(0, 0, Random.Range(0f, 360f), Space.Self);
        }
        
        // Случайный размер
        if (randomizeScale)
        {
            float scale = Random.Range(minScale, maxScale);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
    
    private void Update()
    {
        // Если прошло достаточно времени, начинаем затухание
        if (Time.time > startTime + fadeDelay && decalMaterial != null)
        {
            // Вычисляем прогресс затухания
            float fadeProgress = (Time.time - (startTime + fadeDelay)) / fadeTime;
            
            // Обновляем прозрачность
            alpha = Mathf.Clamp01(1f - fadeProgress);
            Color newColor = originalColor;
            newColor.a = alpha;
            decalMaterial.color = newColor;
            
            // Если полностью исчезли, деактивируем объект
            if (alpha <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Устанавливает размер декаля
    /// </summary>
    public void SetSize(float size)
    {
        transform.localScale = new Vector3(size, size, size);
    }
    
    /// <summary>
    /// Устанавливает цвет декаля
    /// </summary>
    public void SetColor(Color color)
    {
        if (decalMaterial != null)
        {
            originalColor = color;
            decalMaterial.color = color;
        }
    }
    
    /// <summary>
    /// Устанавливает время затухания декаля
    /// </summary>
    public void SetFadeTime(float time, float delay = -1)
    {
        fadeTime = time;
        if (delay >= 0)
        {
            fadeDelay = delay;
        }
    }
} 