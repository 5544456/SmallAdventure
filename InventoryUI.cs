using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipNameText;
    [SerializeField] private TextMeshProUGUI tooltipDescriptionText;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private Slider weightSlider;
    [Header("Slot Settings")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int slotCount = 24;
    [SerializeField] private GameObject itemsContainer;
    [SerializeField] private GameObject hpSpCanvas; // Canvas для HP/SP

    private List<ItemSlotUI> slots = new List<ItemSlotUI>();
    private ItemSlotUI selectedSlot;
    private ItemSlotUI hoveredSlot;
    private bool isDragging = false;
    private Vector2 dragOffset;
    private InventoryManager inventoryManager;

    private void Start()
    {
        slots.Clear();
        var existingSlots = itemsContainer.GetComponentsInChildren<ItemSlotUI>(true);
        slots.AddRange(existingSlots);
        if (slots.Count == 0)
        {
            Debug.LogError("Слоты инвентаря не найдены! Убедитесь, что в ItemsContainer есть объекты с компонентом ItemSlotUI");
            return;
        }
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            slot.Initialize(i);
            int index = i;
            slot.OnPointerEnterEvent += () => ShowTooltip(slot, Input.mousePosition);
            slot.OnPointerExitEvent += HideTooltip;
            slot.OnBeginDragEvent += () => StartDragging(slot);
            slot.OnEndDragEvent += () => StopDragging(slot);
            slot.OnDropEvent += (sourceIndex) => HandleDrop(sourceIndex, index);
            slot.OnDoubleClickEvent += () => TryEquipItem(slot);
        }
        Debug.Log($"Найдено {slots.Count} слотов инвентаря");

        // Находим InventoryManager
        inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager не найден!");
            return;
        }

        // Подписываемся на события инвентаря
        inventoryManager.OnItemAdded += OnItemAdded;
        inventoryManager.OnItemRemoved += OnItemRemoved;
        inventoryManager.OnWeightChanged += UpdateWeightText;
        inventoryManager.OnItemEquipped += OnItemEquipped;
        inventoryManager.OnItemUnequipped += OnItemUnequipped;

        // Подписываемся на открытие/закрытие инвентаря
        var player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.OnInventoryToggled.AddListener((isOpen) => OnInventoryToggled(isOpen));
        }

        // Обновляем UI
        UpdateUI();
    }

    private void StartDragging(ItemSlotUI slot)
    {
        if (slot.CurrentItem != null)
        {
            selectedSlot = slot;
            isDragging = true;
        }
    }

    private void StopDragging(ItemSlotUI slot)
    {
        selectedSlot = null;
        isDragging = false;
    }

    private void HandleDrop(int fromIndex, int toIndex)
    {
        if (fromIndex != toIndex && inventoryManager != null)
        {
            inventoryManager.SwapItems(fromIndex, toIndex);
        }
    }

    public void UpdateUI()
    {
        if (inventoryManager == null) return;

        var items = inventoryManager.Items;
        Debug.Log($"[InventoryUI] Обновляем UI. Количество предметов: {items.Count}, количество слотов: {slots.Count}");
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Count)
            {
                slots[i].SetItem(items[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }

        UpdateWeightText(inventoryManager.CurrentWeight, inventoryManager.MaxWeight);
    }

    public void ShowTooltip(ItemSlotUI slot, Vector2 position)
    {
        if (slot.CurrentItem != null && tooltipPanel != null && tooltipNameText != null && tooltipDescriptionText != null)
        {
            tooltipNameText.text = slot.CurrentItem.itemName;
            tooltipDescriptionText.text = GetItemTooltipDescription(slot.CurrentItem);
            tooltipPanel.SetActive(true);
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private string GetItemTooltipDescription(Item item)
    {
        if (item == null) return "";
        string desc = item.description;
        desc += $"\nВес: {item.weight:F1}";
        return desc;
    }

    private void OnItemAdded(Item item)
    {
        UpdateUI();
    }

    private void OnItemRemoved(Item item)
    {
        UpdateUI();
    }

    private void OnItemEquipped(Item item)
    {
        UpdateUI();
    }

    private void OnItemUnequipped(Item item)
    {
        UpdateUI();
    }

    private void UpdateWeightText(float currentWeight, float maxWeight)
    {
        if (weightText != null)
        {
            weightText.text = $"Вес: {currentWeight:F1}/{maxWeight:F1}";
        }

        if (weightSlider != null)
        {
            weightSlider.value = currentWeight / maxWeight;
        }
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded -= OnItemAdded;
            inventoryManager.OnItemRemoved -= OnItemRemoved;
            inventoryManager.OnWeightChanged -= UpdateWeightText;
            inventoryManager.OnItemEquipped -= OnItemEquipped;
            inventoryManager.OnItemUnequipped -= OnItemUnequipped;
        }
    }

    private void TryEquipItem(ItemSlotUI slot)
    {
        // Ищем InventoryManager или Inventory
        var inventory = FindFirstObjectByType<InventoryManager>() as MonoBehaviour;
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>() as MonoBehaviour;
        if (inventory != null && slot.CurrentItem != null)
        {
            var method = inventory.GetType().GetMethod("EquipItem");
            if (method != null)
            {
                method.Invoke(inventory, new object[] { slot.CurrentItem });
            }
        }
    }

    // Метод для обработки открытия/закрытия инвентаря
    public void OnInventoryToggled(bool isOpen)
    {
        if (hpSpCanvas != null)
        {
            hpSpCanvas.SetActive(!isOpen);
        }
    }

    // Этот метод можно назначить через EventTrigger на слотах для drag&drop
    public void OnEndDrag(BaseEventData eventData)
    {
        var pointerEventData = eventData as PointerEventData;
        if (pointerEventData == null) return;

        if (selectedSlot != null && selectedSlot.CurrentItem != null)
        {
            if (pointerEventData.pointerCurrentRaycast.gameObject != null)
            {
                var equipmentSlot = pointerEventData.pointerCurrentRaycast.gameObject.GetComponent<EquipmentSlotUI>();
                if (equipmentSlot != null && equipmentSlot.SlotType == selectedSlot.CurrentItem.itemType)
                {
                    var inventory = FindFirstObjectByType<InventoryManager>() as MonoBehaviour;
                    if (inventory == null)
                        inventory = FindFirstObjectByType<Inventory>() as MonoBehaviour;
                    if (inventory != null)
                    {
                        var method = inventory.GetType().GetMethod("EquipItem");
                        if (method != null)
                        {
                            method.Invoke(inventory, new object[] { selectedSlot.CurrentItem });
                        }
                    }
                }
            }
        }
        selectedSlot = null;
        isDragging = false;
    }
}