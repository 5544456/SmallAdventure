using UnityEngine;

public class BuildingInstance : MonoBehaviour
{
    public BuildingData data;
    public BuildingSystem system;
    public int level = 1; // Добавляем поле для хранения уровня здания
    
    private void Start()
    {
        // Если система не назначена, пытаемся найти её
        if (system == null)
        {
            system = FindFirstObjectByType<BuildingSystem>();
        }
        
        // Регистрируем здание в системе строительства
        if (system != null)
        {
            system.RegisterCurrentBuilding(gameObject);
        }
    }
} 