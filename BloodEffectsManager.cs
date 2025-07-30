using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Менеджер эффектов крови, который предоставляет общие настройки и пул объектов для эффектов
/// </summary>
public class BloodEffectsManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject bloodSplatterPrefab; // Префаб брызг крови
    [SerializeField] private GameObject[] bloodDecalPrefabs; // Префабы декалей крови
    
    [Header("Settings")]
    [SerializeField] private int poolSize = 20; // Размер пула объектов
    [SerializeField] private float bloodLifetime = 5f; // Время жизни эффектов крови
    [SerializeField] private float decalLifetime = 30f; // Время жизни декалей
    [SerializeField] private float minDecalSize = 0.2f; // Минимальный размер декаля
    [SerializeField] private float maxDecalSize = 0.5f; // Максимальный размер декаля
    
    // Пулы объектов для оптимизации
    private Queue<GameObject> bloodSplatterPool;
    private Queue<GameObject> bloodDecalPool;
    
    // Синглтон
    private static BloodEffectsManager _instance;
    public static BloodEffectsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<BloodEffectsManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("BloodEffectsManager");
                    _instance = go.AddComponent<BloodEffectsManager>();
                }
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Инициализируем пулы объектов
        InitializePools();
    }
    
    private void InitializePools()
    {
        // Создаем пул для брызг крови
        bloodSplatterPool = new Queue<GameObject>();
        if (bloodSplatterPrefab != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(bloodSplatterPrefab, transform);
                obj.SetActive(false);
                bloodSplatterPool.Enqueue(obj);
            }
        }
        
        // Создаем пул для декалей крови
        bloodDecalPool = new Queue<GameObject>();
        if (bloodDecalPrefabs != null && bloodDecalPrefabs.Length > 0)
        {
            for (int i = 0; i < poolSize; i++)
            {
                // Выбираем случайный префаб из массива
                GameObject prefab = bloodDecalPrefabs[Random.Range(0, bloodDecalPrefabs.Length)];
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                bloodDecalPool.Enqueue(obj);
            }
        }
    }
    
    /// <summary>
    /// Создает эффект брызг крови в указанной точке
    /// </summary>
    public GameObject SpawnBloodSplatter(Vector3 position, Vector3 normal, float intensity = 1f)
    {
        if (bloodSplatterPool.Count == 0 || bloodSplatterPrefab == null)
            return null;
            
        // Получаем объект из пула
        GameObject bloodEffect = bloodSplatterPool.Dequeue();
        
        // Настраиваем позицию и поворот
        bloodEffect.transform.position = position;
        bloodEffect.transform.rotation = Quaternion.LookRotation(normal);
        
        // Настраиваем систему частиц
        ParticleSystem ps = bloodEffect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startSizeMultiplier *= intensity;
            
            // Запускаем систему частиц
            ps.Stop();
            ps.Clear();
            ps.Play();
        }
        
        // Активируем объект
        bloodEffect.SetActive(true);
        
        // Возвращаем объект в пул через некоторое время
        StartCoroutine(ReturnToPoolAfterDelay(bloodEffect, bloodSplatterPool, bloodLifetime));
        
        return bloodEffect;
    }
    
    /// <summary>
    /// Создает декаль крови на поверхности
    /// </summary>
    public GameObject SpawnBloodDecal(Vector3 position, Quaternion rotation, Transform parent = null, float size = 1f)
    {
        if (bloodDecalPool.Count == 0 || bloodDecalPrefabs.Length == 0)
            return null;
            
        // Получаем объект из пула
        GameObject decal = bloodDecalPool.Dequeue();
        
        // Настраиваем позицию и поворот
        decal.transform.position = position;
        decal.transform.rotation = rotation;
        
        // Настраиваем размер декаля
        float actualSize = Mathf.Lerp(minDecalSize, maxDecalSize, size);
        decal.transform.localScale = new Vector3(actualSize, actualSize, actualSize);
        
        // Если указан родитель, привязываем декаль к нему
        if (parent != null)
        {
            decal.transform.SetParent(parent);
        }
        else
        {
            decal.transform.SetParent(transform);
        }
        
        // Активируем объект
        decal.SetActive(true);
        
        // Возвращаем объект в пул через некоторое время
        StartCoroutine(ReturnToPoolAfterDelay(decal, bloodDecalPool, decalLifetime));
        
        return decal;
    }
    
    /// <summary>
    /// Создает эффекты крови при ударе по поверхности
    /// </summary>
    public void CreateBloodEffectOnHit(Vector3 hitPoint, Vector3 hitNormal, float intensity = 1f, Transform parent = null)
    {
        // Создаем брызги крови
        SpawnBloodSplatter(hitPoint + hitNormal * 0.05f, hitNormal, intensity);
        
        // Создаем декаль крови на поверхности
        SpawnBloodDecal(hitPoint + hitNormal * 0.01f, Quaternion.LookRotation(-hitNormal), parent, intensity);
    }
    
    /// <summary>
    /// Создает множественные декали крови на объекте
    /// </summary>
    public void CreateMultipleBloodDecals(Collider targetCollider, int count, float intensity = 1f)
    {
        if (targetCollider == null) return;
        
        for (int i = 0; i < count; i++)
        {
            // Выбираем случайную точку на поверхности коллайдера
            Vector3 randomPoint = new Vector3(
                Random.Range(targetCollider.bounds.min.x, targetCollider.bounds.max.x),
                Random.Range(targetCollider.bounds.min.y, targetCollider.bounds.max.y),
                Random.Range(targetCollider.bounds.min.z, targetCollider.bounds.max.z)
            );
            
            // Находим ближайшую точку на поверхности коллайдера
            Vector3 surfacePoint = targetCollider.ClosestPoint(randomPoint);
            
            // Определяем нормаль в этой точке (примерно)
            Vector3 normal = (surfacePoint - targetCollider.transform.position).normalized;
            
            // Создаем декаль
            SpawnBloodDecal(
                surfacePoint + normal * 0.01f, 
                Quaternion.LookRotation(-normal), 
                targetCollider.transform, 
                Random.Range(0.5f, 1f) * intensity
            );
        }
    }
    
    /// <summary>
    /// Возвращает объект в пул через указанное время
    /// </summary>
    private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject obj, Queue<GameObject> pool, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (obj != null)
        {
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            pool.Enqueue(obj);
        }
    }
} 