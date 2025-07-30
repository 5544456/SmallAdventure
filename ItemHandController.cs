using UnityEngine;

public class ItemHandController : MonoBehaviour
{
    [SerializeField] private Transform handPoint;
    private GameObject currentItem;
    private Item equippedItem;
    
    public void EquipItem(Item item)
    {
        if (item == null)
        {
            UnequipItem();
            return;
        }
        
        if (item.prefab != null)
        {
            // Если уже есть предмет в руках, убираем его
            if (currentItem != null)
            {
                UnequipItem();
            }
            
            // Создаем новый предмет
            currentItem = Instantiate(item.prefab, handPoint.position, handPoint.rotation, handPoint);
            equippedItem = item;
            
            // Настраиваем позицию и поворот
            currentItem.transform.localPosition = Vector3.zero;
            currentItem.transform.localRotation = Quaternion.identity;
        }
    }
    
    public void UnequipItem()
    {
        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
            equippedItem = null;
        }
    }
    
    public Item GetEquippedItem()
    {
        return equippedItem;
    }
} 