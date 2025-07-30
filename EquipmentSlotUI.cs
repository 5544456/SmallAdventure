using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Item.ItemType slotType;
    
    private Item currentItem;
    
    public Item CurrentItem => currentItem;
    public Item.ItemType SlotType => slotType;
    
    public void SetItem(Item item)
    {
        if (item != null && item.itemType != slotType)
            return;
            
        currentItem = item;
        
        if (item != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
        }
        else
        {
            ClearSlot();
        }
    }
    
    public void ClearSlot()
    {
        currentItem = null;
        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }
    
    // Реализация интерфейса для перетаскивания
    public void OnDrop(PointerEventData eventData)
    {
        // Обработка перетаскивания будет происходить в InventoryUI
    }
} 