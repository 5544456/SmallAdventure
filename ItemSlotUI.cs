using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image backgroundImage;
    
    private Item currentItem;
    private int slotIndex;
    private bool isEquipped;

    public Item CurrentItem => currentItem;
    public int SlotIndex => slotIndex;
    public bool IsEquipped => isEquipped;

    public event Action OnPointerEnterEvent;
    public event Action OnPointerExitEvent;
    public event Action OnBeginDragEvent;
    public event Action OnEndDragEvent;
    public event Action<int> OnDropEvent;
    public event Action OnDoubleClickEvent;

    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;

    public void Initialize(int index)
    {
        slotIndex = index;
        ClearSlot();
    }

    public void SetItem(Item item)
    {
        currentItem = item;
        UpdateVisual();
    }

    public void ClearSlot()
    {
        currentItem = null;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (itemIcon != null)
        {
            itemIcon.enabled = currentItem != null;
            if (currentItem != null)
            {
                itemIcon.sprite = currentItem.Icon;
                Debug.Log($"[ItemSlotUI] Слот {slotIndex}: Назначен спрайт {currentItem.Icon?.name} для предмета {currentItem.itemName}");
            }
            else
            {
                Debug.Log($"[ItemSlotUI] Слот {slotIndex}: Очищен");
            }
        }
    }

    public void SetEquipped(bool equipped)
    {
        isEquipped = equipped;
        if (backgroundImage != null)
        {
            backgroundImage.color = equipped ? new Color(0.8f, 0.8f, 1f) : Color.white;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            OnBeginDragEvent?.Invoke();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragEvent?.Invoke();
    }

    public void OnDrop(PointerEventData eventData)
    {
        ItemSlotUI sourceSlot = eventData.pointerDrag.GetComponent<ItemSlotUI>();
        if (sourceSlot != null)
        {
            OnDropEvent?.Invoke(sourceSlot.SlotIndex);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            float time = Time.unscaledTime;
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (time - lastClickTime < doubleClickThreshold)
                {
                    OnDoubleClickEvent?.Invoke();
                }
                lastClickTime = time;
            }
            // ... обработка правого клика, если нужно ...
        }
    }
}