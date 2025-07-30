using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemPrefab : MonoBehaviour
{
    [Header("Настройки предмета")]
    public SpriteRenderer itemIcon;
    [SerializeField] private float pickupRadius = 1.5f; // Радиус, в котором можно подобрать предмет
    
    [Header("Физические свойства")]
    [SerializeField] private float floatHeight = 0.5f; // Высота над землей
    [SerializeField] private float rotationSpeed = 50f; // Скорость вращения
    [SerializeField] private float bobSpeed = 1f; // Скорость плавания вверх-вниз
    [SerializeField] private float bobHeight = 0.2f; // Амплитуда плавания
    
    // Внутренние переменные
    private Item containedItem;
    private bool isGrounded = false;
    private Vector3 startPosition;
    private bool isPickupable = false;
    private GameObject playerInRange = null;
    
    private void Start()
    {
        // Проверяем наличие необходимых компонентов
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = 1f;
            rb.linearDamping = 1f;
        }
        
        // Убеждаемся, что основной коллайдер не является триггером
        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null)
        {
            mainCollider.isTrigger = false;
        }
        else
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.5f, 0.5f, 0.5f);
            boxCollider.isTrigger = false;
        }
        
        // Добавляем отдельный триггер-коллайдер для подбора предмета
        SphereCollider pickupCollider = gameObject.AddComponent<SphereCollider>();
        pickupCollider.radius = pickupRadius;
        pickupCollider.isTrigger = true;
        
        // Начинаем проверку, на земле ли предмет
        StartCoroutine(CheckGroundedStatus());
    }
    
    private void Update()
    {
        if (isGrounded && isPickupable)
        {
            // Делаем предмет "плавающим" и вращающимся, когда он на земле
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        // Проверяем нажатие E для подбора
        if (playerInRange != null && isPickupable && Input.GetKeyDown(KeyCode.E))
        {
            var inventory = FindFirstObjectByType<InventoryManager>();
            if (inventory != null && containedItem != null)
            {
                if (inventory.AddItem(containedItem))
                {
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Не удалось добавить предмет в инвентарь (нет места или перегруз)");
                }
            }
        }
    }
    
    // Проверка, находится ли предмет на земле
    private IEnumerator CheckGroundedStatus()
    {
        // Ждем немного, чтобы физика успела отработать
        yield return new WaitForSeconds(1f);
        
        // Проверяем, находится ли предмет на земле
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f))
        {
            // Предмет на земле - поднимаем его на нужную высоту
            Vector3 newPosition = hit.point + Vector3.up * floatHeight;
            transform.position = newPosition;
            startPosition = newPosition;
            
            // Отключаем гравитацию и кинематику
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            
            isGrounded = true;
            isPickupable = true;
        }
        else
        {
            // Предмет еще падает - проверяем позже
            StartCoroutine(CheckGroundedStatus());
        }
    }
    
    public void SetItem(Item item)
    {
        containedItem = item;
        if (itemIcon != null && item != null)
        {
            itemIcon.sprite = item.icon;
            gameObject.name = $"DroppedItem_{item.itemName}";
        }
    }
    
    public Item GetItem()
    {
        return containedItem;
    }
    
    public Item PickUpItem()
    {
        if (!isPickupable) 
            return null;
            
        Item item = containedItem;
        Destroy(gameObject);
        return item;
    }
    
    // Для взаимодействия с игроком (триггер для обнаружения игрока рядом)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isPickupable)
        {
            playerInRange = other.gameObject;
            Debug.Log("Нажмите [E] чтобы подобрать: " + containedItem.itemName);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerInRange == other.gameObject)
                playerInRange = null;
            // Убрать сообщение UI
        }
    }
} 
