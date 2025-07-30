using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int maxSlots = 20;
    [SerializeField] private float maxWeight = 50f;
    private List<Item> items = new List<Item>();
    private float currentWeight = 0f;
    
    // Слоты экипировки
    [System.Serializable]
    public class EquipmentSlots
    {
        public Item head;
        public Item body;
        public Item hands;
        public Item legs;
        public Item boots;
        public Item mainHand;
        public Item offHand;
    }
    
    public EquipmentSlots equippedItems = new EquipmentSlots();
    
    // Ссылка на точку, где будут спавниться выброшенные предметы
    public Transform dropPoint;
    
    // Префаб для выброшенных на землю предметов
    public GameObject droppedItemPrefab;
    
    // События для оповещения UI
    public UnityEvent onInventoryChanged;
    public UnityEvent<float, float> onWeightChanged;
    
    // Переменная для хранения предмета в руках
    private GameObject currentItemInHands;
    public Transform weaponSocket; // Сокет для оружия
    public Transform shieldSocket; // Сокет для щита

    public List<Item> GetItems()
    {
        return new List<Item>(items);
    }

    public float GetCurrentWeight()
    {
        return currentWeight;
    }

    public float GetMaxWeight()
    {
        return maxWeight;
    }
    
    // Методы для управления инвентарем
    public bool AddItem(Item item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Инвентарь полон!");
            return false;
        }
        
        float newWeight = currentWeight + (item.weight * item.Count);
        if (newWeight > maxWeight)
        {
            Debug.Log("Слишком тяжело!");
            return false;
        }
        
        items.Add(item);
        currentWeight = newWeight;
        
        onInventoryChanged?.Invoke();
        onWeightChanged?.Invoke(currentWeight, maxWeight);
        
        return true;
    }
    
    public void RemoveItem(Item item)
    {
        if (items.Remove(item))
        {
            currentWeight -= (item.weight * item.Count);
            onInventoryChanged?.Invoke();
            onWeightChanged?.Invoke(currentWeight, maxWeight);
        }
    }
    
    // Метод для выбрасывания предмета
    public void DropItem(Item item)
    {
        if (item != null)
        {
            RemoveItem(item);
            
            if (droppedItemPrefab != null && dropPoint != null)
            {
                GameObject droppedItem = Instantiate(droppedItemPrefab, dropPoint.position, Quaternion.identity);
                DroppedItemPrefab itemPrefab = droppedItem.GetComponent<DroppedItemPrefab>();
                if (itemPrefab != null)
                {
                    itemPrefab.SetItem(item);
                }
            }
        }
    }
    
    // Метод для экипировки предмета
    public void EquipItem(Item itemToEquip)
    {
        if (itemToEquip == null) return;
        
        Item previousItem = null;
        
        // Выбор слота в зависимости от типа предмета
        switch (itemToEquip.itemType)
        {
            case Item.ItemType.Head:
                previousItem = equippedItems.head;
                equippedItems.head = itemToEquip;
                break;
            case Item.ItemType.Body:
                previousItem = equippedItems.body;
                equippedItems.body = itemToEquip;
                break;
            case Item.ItemType.Hands:
                previousItem = equippedItems.hands;
                equippedItems.hands = itemToEquip;
                break;
            case Item.ItemType.Legs:
                previousItem = equippedItems.legs;
                equippedItems.legs = itemToEquip;
                break;
            case Item.ItemType.Boots:
                previousItem = equippedItems.boots;
                equippedItems.boots = itemToEquip;
                break;
            case Item.ItemType.MainHand:
                previousItem = equippedItems.mainHand;
                equippedItems.mainHand = itemToEquip;
                break;
            case Item.ItemType.OffHand:
                previousItem = equippedItems.offHand;
                equippedItems.offHand = itemToEquip;
                break;
            default:
                Debug.LogWarning($"Предмет {itemToEquip.itemName} нельзя экипировать");
                return;
        }
        
        // Возвращаем предыдущий предмет в инвентарь, если он был
        if (previousItem != null)
        {
            AddItem(previousItem);
        }
        
        // Удаляем экипированный предмет из инвентаря
        RemoveItem(itemToEquip);
        
        // Если это оружие или щит, отображаем его в руках
        if (itemToEquip.itemType == Item.ItemType.MainHand || 
            itemToEquip.itemType == Item.ItemType.OffHand)
        {
            EquipItemInHands(itemToEquip);
        }
        
        onInventoryChanged?.Invoke();
    }
    
    // Метод для снятия экипированного предмета
    public void UnequipItem(Item itemToUnequip)
    {
        if (itemToUnequip == null) return;
        
        bool wasUnequipped = false;
        
        // Проверяем, в каком слоте находится предмет и снимаем его
        if (equippedItems.head == itemToUnequip)
        {
            equippedItems.head = null;
            wasUnequipped = true;
        }
        else if (equippedItems.body == itemToUnequip)
        {
            equippedItems.body = null;
            wasUnequipped = true;
        }
        else if (equippedItems.hands == itemToUnequip)
        {
            equippedItems.hands = null;
            wasUnequipped = true;
        }
        else if (equippedItems.legs == itemToUnequip)
        {
            equippedItems.legs = null;
            wasUnequipped = true;
        }
        else if (equippedItems.boots == itemToUnequip)
        {
            equippedItems.boots = null;
            wasUnequipped = true;
        }
        else if (equippedItems.mainHand == itemToUnequip)
        {
            equippedItems.mainHand = null;
            wasUnequipped = true;
            
            // Убираем предмет из рук
            if (currentItemInHands != null)
            {
                Destroy(currentItemInHands);
                currentItemInHands = null;
            }
        }
        else if (equippedItems.offHand == itemToUnequip)
        {
            equippedItems.offHand = null;
            wasUnequipped = true;
        }
        
        // Если предмет был снят, добавляем его в инвентарь
        if (wasUnequipped)
        {
            AddItem(itemToUnequip);
            onInventoryChanged?.Invoke();
        }
    }
    
    private void EquipItemInHands(Item item)
    {
        if (item == null || item.prefab == null) return;

        if (item.itemType == Item.ItemType.MainHand)
        {
            var socket = FindObjectOfType<WeaponSocket>();
            if (socket != null)
                socket.EquipWeapon(item.prefab);
        }
        else if (item.itemType == Item.ItemType.OffHand)
        {
            var socket = FindObjectOfType<ShieldSocket>();
            if (socket != null)
                socket.EquipShield(item.prefab);
        }
    }

    public void Clear()
    {
        items.Clear();
        currentWeight = 0f;
        onInventoryChanged?.Invoke();
        onWeightChanged?.Invoke(currentWeight, maxWeight);
    }
}

 