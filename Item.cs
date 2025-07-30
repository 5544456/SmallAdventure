using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public enum ItemType
    {
        None,
        Head,
        Body,
        Hands,
        Legs,
        Boots,
        MainHand,
        OffHand
    }

    public string id;
    public string itemName;
    public string description;
    public Sprite icon;
    public float weight = 1f;
    public int maxStackSize = 1;
    public GameObject prefab;
    public ItemType itemType = ItemType.None;

    [SerializeField]
    private int count = 1;

    public int Count
    {
        get => count;
        set => count = Mathf.Clamp(value, 0, maxStackSize);
    }

    public bool IsStackable => maxStackSize > 1;
    public Sprite Icon => icon;

    public Item Clone()
    {
        Item clone = Instantiate(this);
        clone.count = this.count;
        return clone;
    }
} 