using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    [SerializeField] private Item[] itemsToSpawn; // Предметы для спавна
    [SerializeField] private GameObject droppedItemPrefab; // Префаб выброшенного предмета
    [SerializeField] private float spawnRadius = 5f; // Радиус спавна
    [SerializeField] private bool spawnOnStart = true; // Спавнить при старте?
    
    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnItems();
        }
    }
    
    public void SpawnItems()
    {
        if (droppedItemPrefab == null)
        {
            Debug.LogError("Не указан префаб для выброшенных предметов!");
            return;
        }
        
        foreach (Item item in itemsToSpawn)
        {
            SpawnItem(item);
        }
    }
    
    public void SpawnItem(Item item)
    {
        if (item == null || droppedItemPrefab == null)
            return;
            
        // Получаем случайную позицию в радиусе
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        
        // Создаем предмет
        GameObject droppedItem = Instantiate(droppedItemPrefab, spawnPosition, Quaternion.identity);
        DroppedItemPrefab itemPrefab = droppedItem.GetComponent<DroppedItemPrefab>();
        
        if (itemPrefab != null)
        {
            itemPrefab.SetItem(item);
        }
    }
    
    // Отображение в редакторе для наглядности
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
} 