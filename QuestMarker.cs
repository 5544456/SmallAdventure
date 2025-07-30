using UnityEngine;

public class QuestMarker : MonoBehaviour
{
    [SerializeField] private float bobSpeed = 1.0f; // Скорость движения вверх-вниз
    [SerializeField] private float bobHeight = 0.2f; // Высота движения вверх-вниз
    [SerializeField] private float rotationSpeed = 50.0f; // Скорость вращения
    
    private Vector3 startPosition; // Начальная позиция маркера
    private float bobTimer = 0f; // Таймер для движения вверх-вниз
    private Transform mainCamera; // Ссылка на камеру для билборда
    
    void Start()
    {
        // Сохраняем начальную позицию
        startPosition = transform.localPosition;
        
        // Находим главную камеру
        mainCamera = Camera.main.transform;
    }
    
    void Update()
    {
        // Плавное движение вверх-вниз
        bobTimer += Time.deltaTime;
        float newY = startPosition.y + Mathf.Sin(bobTimer * bobSpeed) * bobHeight;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
        
        // Вращение маркера
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Поворот к камере (билборд)
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.rotation * Vector3.forward,
                            mainCamera.rotation * Vector3.up);
        }
    }
} 