using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private int maxSlots = 24;
    [SerializeField] private float maxWeight = 50f;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private GameObject tooltipPanel;

    private List<Item> items = new List<Item>();
    private float currentWeight;
    private bool isInventoryOpen = false;

    public event Action OnInventoryChanged;
    public event Action<float, float> OnWeightChanged;
    public event Action<Item> OnItemAdded;
    public event Action<Item> OnItemRemoved;
    public event Action<Item> OnItemEquipped;
    public event Action<Item> OnItemUnequipped;

    public List<Item> Items => items;
    public int MaxSlots => maxSlots;
    public float MaxWeight => maxWeight;
    public float CurrentWeight => currentWeight;
    public bool IsInventoryOpen => isInventoryOpen;

    private InventoryUI inventoryUI;
    private Player cachedPlayer;

    private void Start()
    {
        // Находим UI инвентаря
        inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI == null)
        {
            Debug.LogError("InventoryUI не найден!");
            return;
        }
        
        // Скрываем UI инвентаря
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        isInventoryOpen = false;

        
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            // Инвертируем состояние
            isInventoryOpen = !isInventoryOpen;
            
            // Применяем новое состояние к UI элементам
            inventoryPanel.SetActive(isInventoryOpen);
            if (equipmentPanel != null)
                equipmentPanel.SetActive(isInventoryOpen);
            if (tooltipPanel != null)
                tooltipPanel.SetActive(isInventoryOpen);
                
            // Обрабатываем открытие/закрытие инвентаря
            if (isInventoryOpen)
            {
                if (inventoryUI != null)
            {
                inventoryUI.UpdateUI();
                Debug.Log($"[InventoryManager] Инвентарь открыт. Всего предметов: {items.Count}");
                }
                OpenInventory();
            }
            else
            {
                Debug.Log("[InventoryManager] Инвентарь закрыт");
                CloseInventory();
            }
        }
        else
        {
            Debug.LogError("InventoryPanel не найден!");
        }
    }

    public bool AddItem(Item item)
    {
        Debug.Log($"[InventoryManager] Пробуем добавить предмет: {item?.itemName}, тип: {item?.itemType}, вес: {item?.weight}");
        if (items.Count >= maxSlots)
        {
            Debug.Log("[InventoryManager] Инвентарь полон!");
            return false;
        }

        if (currentWeight + item.weight > maxWeight)
        {
            Debug.Log("[InventoryManager] Слишком тяжело!");
            return false;
        }

        items.Add(item);
        currentWeight += item.weight;
        Debug.Log($"[InventoryManager] Предмет добавлен: {item.itemName}. Всего предметов: {items.Count}");

        OnInventoryChanged?.Invoke();
        OnWeightChanged?.Invoke(currentWeight, maxWeight);
        OnItemAdded?.Invoke(item);

        return true;
    }

    public void RemoveItem(Item item)
    {
        if (items.Remove(item))
        {
            currentWeight -= item.weight;
            Debug.Log($"[InventoryManager] Предмет удалён: {item.itemName}. Осталось предметов: {items.Count}");
            OnInventoryChanged?.Invoke();
            OnWeightChanged?.Invoke(currentWeight, maxWeight);
            OnItemRemoved?.Invoke(item);
        }
    }

    public void RemoveItemAt(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            var item = items[index];
            currentWeight -= item.weight;
            items.RemoveAt(index);
            OnInventoryChanged?.Invoke();
            OnWeightChanged?.Invoke(currentWeight, maxWeight);
            OnItemRemoved?.Invoke(item);
        }
    }

    public void SwapItems(int fromIndex, int toIndex)
    {
        if (fromIndex >= 0 && fromIndex < items.Count && toIndex >= 0 && toIndex < items.Count)
        {
            Item temp = items[fromIndex];
            items[fromIndex] = items[toIndex];
            items[toIndex] = temp;

            OnInventoryChanged?.Invoke();
        }
    }

    public void Clear()
    {
        var oldItems = new List<Item>(items);
        items.Clear();
        currentWeight = 0f;
        OnInventoryChanged?.Invoke();
        OnWeightChanged?.Invoke(currentWeight, maxWeight);
        foreach (var item in oldItems)
        {
            OnItemRemoved?.Invoke(item);
        }
    }

    public void SaveInventory()
    {
        // Здесь будет реализация сохранения инвентаря
    }

    public void LoadInventory()
    {
        // Здесь будет реализация загрузки инвентаря
    }

    public void DropItem(Item item)
    {
        // Здесь можно добавить логику сброса предмета
    }

    public void UseItem(Item item)
    {
        Debug.Log($"Использование предмета: {item.itemName}");
        // Здесь можно добавить логику использования предмета
    }

    public void OpenInventory()
    {
        if (cachedPlayer == null) cachedPlayer = FindFirstObjectByType<Player>();
        if (cachedPlayer != null) cachedPlayer.BlockInput(true);
    }

    public void CloseInventory()
    {
        if (cachedPlayer == null) cachedPlayer = FindFirstObjectByType<Player>();
        var buildSystem = FindFirstObjectByType<BuildingSystem>();
        bool buildMenuOpen = (buildSystem != null && buildSystem.isBuildMenuOpen);
        
        if (!buildMenuOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (cachedPlayer != null) 
            {
                cachedPlayer.BlockInput(false);
                cachedPlayer.isInputBlocked = false;
            }
        }
    }
} 