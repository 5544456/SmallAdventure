// ВРЕМЕННО ОТКЛЮЧЕНО
/*
using UnityEngine;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class UpgradeCost
{
    public int wood;
    public int stone;
    public int gold;
}

[System.Serializable]
public class BuildingUpgrade
{
    public string buildingName;
    public GameObject[] upgradeLevels; // Префабы для каждого уровня улучшения
    public UpgradeCost[] costs; // Стоимость каждого уровня улучшения
}

[System.Serializable]
public class ItemUpgrade
{
    public string itemName;
    public GameObject[] upgradeLevels; // Префабы для каждого уровня улучшения
    public UpgradeCost[] costs; // Стоимость каждого уровня улучшения
    public float[] damageMultipliers; // Множители урона для каждого уровня
    public float[] armorMultipliers; // Множители защиты для каждого уровня
}

public class UpgradeSystem : MonoBehaviour
{
    [Header("Улучшения")]
    [SerializeField] private BuildingUpgrade[] buildingUpgrades;
    [SerializeField] private ItemUpgrade[] itemUpgrades;
    
    [Header("UI")]
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Transform upgradeButtonsContainer;
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private TextMeshProUGUI resourcesText;
    
    private BuildingSystem buildingSystem;
    private Dictionary<string, BuildingUpgrade> buildingUpgradeDict = new Dictionary<string, BuildingUpgrade>();
    private Dictionary<string, ItemUpgrade> itemUpgradeDict = new Dictionary<string, ItemUpgrade>();
    private Dictionary<GameObject, int> currentLevels = new Dictionary<GameObject, int>();
    private Dictionary<string, Dictionary<GameObject, int>> buildingLevels = new Dictionary<string, Dictionary<GameObject, int>>();

    private void Start()
    {
        buildingSystem = UnityEngine.Object.FindFirstObjectByType<BuildingSystem>();
        if (buildingSystem == null)
        {
            Debug.LogError("BuildingSystem not found!");
            return;
        }
        
        // Инициализируем словари для быстрого доступа
        foreach (BuildingUpgrade upgrade in buildingUpgrades)
        {
            buildingUpgradeDict[upgrade.buildingName] = upgrade;
        }
        
        foreach (ItemUpgrade upgrade in itemUpgrades)
        {
            itemUpgradeDict[upgrade.itemName] = upgrade;
        }
        
        if (upgradeUI != null)
            upgradeUI.SetActive(false);

        // Инициализация словаря уровней зданий
        foreach (BuildingUpgrade upgrade in buildingUpgrades)
        {
            buildingLevels[upgrade.buildingName] = new Dictionary<GameObject, int>();
        }
    }

    private void Update()
    {
        // Открытие/закрытие UI улучшений по кнопке U
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleUpgradeUI();
        }
    }

    private void ToggleUpgradeUI()
    {
        if (upgradeUI != null)
        {
            bool isActive = !upgradeUI.activeSelf;
            upgradeUI.SetActive(isActive);
            
            if (isActive)
            {
                UpdateUpgradeUI();
            }
        }
    }

    private void UpdateUpgradeUI()
    {
        if (upgradeButtonsContainer == null)
            return;

        // Очищаем существующие кнопки
        foreach (Transform child in upgradeButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        // Создаем кнопки для доступных улучшений
        foreach (var building in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (building.CompareTag("Building"))
            {
                CreateUpgradeButton(building);
            }
        }

        // Добавляем кнопки для предметов в инвентаре
        // Здесь нужно добавить интеграцию с вашей системой инвентаря
    }

    private void CreateUpgradeButton(GameObject target)
    {
        if (upgradeButtonPrefab == null)
            return;

        string targetName = target.name.Replace("(Clone)", "").Trim();
        BuildingUpgrade buildingUpgrade = null;
        ItemUpgrade itemUpgrade = null;
        
        bool isBuilding = buildingUpgradeDict.TryGetValue(targetName, out buildingUpgrade);
        bool isItem = !isBuilding && itemUpgradeDict.TryGetValue(targetName, out itemUpgrade);

        if (!isBuilding && !isItem)
            return;

        int currentLevel = GetCurrentLevel(target);
        if (currentLevel >= (isBuilding ? buildingUpgrade.upgradeLevels.Length : itemUpgrade.upgradeLevels.Length))
            return;

        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonsContainer);
        UnityEngine.UI.Button button = buttonObj.GetComponent<UnityEngine.UI.Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

        UpgradeCost cost = isBuilding ? 
            buildingUpgrade.costs[currentLevel] : 
            itemUpgrade.costs[currentLevel];

        if (buttonText != null)
        {
            buttonText.text = $"Улучшить {targetName}\nУровень {currentLevel + 1} -> {currentLevel + 2}\n" +
                            $"Стоимость:\nДерево: {cost.wood}\nКамень: {cost.stone}\nЗолото: {cost.gold}";
        }

        if (button != null)
        {
            button.onClick.AddListener(() => TryUpgrade(target));
        }
    }

    public int GetCurrentLevel(GameObject target)
    {
        if (!currentLevels.TryGetValue(target, out int level))
        {
            currentLevels[target] = 0;
            return 0;
        }
        return level;
    }

    public void TryUpgrade(GameObject target)
    {
        if (buildingSystem == null)
            return;

        string targetName = target.name.Replace("(Clone)", "").Trim();
        int currentLevel = GetCurrentLevel(target);

        if (buildingUpgradeDict.TryGetValue(targetName, out BuildingUpgrade buildingUpgrade))
        {
            if (currentLevel >= buildingUpgrade.upgradeLevels.Length - 1)
                return;

            UpgradeCost cost = buildingUpgrade.costs[currentLevel];
            if (HasEnoughResources(cost))
            {
                // Списываем ресурсы
                SpendResources(cost);

                // Заменяем модель здания
                Vector3 position = target.transform.position;
                Quaternion rotation = target.transform.rotation;
                Destroy(target);

                GameObject newBuilding = Instantiate(buildingUpgrade.upgradeLevels[currentLevel + 1], 
                    position, rotation);
                newBuilding.name = targetName;
                newBuilding.tag = "Building";

                currentLevels[newBuilding] = currentLevel + 1;
                UpdateUpgradeUI();
            }
        }
        else if (itemUpgradeDict.TryGetValue(targetName, out ItemUpgrade itemUpgrade))
        {
            if (currentLevel >= itemUpgrade.upgradeLevels.Length - 1)
                return;

            UpgradeCost cost = itemUpgrade.costs[currentLevel];
            if (HasEnoughResources(cost))
            {
                // Списываем ресурсы
                SpendResources(cost);

                // Заменяем модель предмета
                Vector3 position = target.transform.position;
                Quaternion rotation = target.transform.rotation;
                Destroy(target);

                GameObject newItem = Instantiate(itemUpgrade.upgradeLevels[currentLevel + 1], 
                    position, rotation);
                newItem.name = targetName;

                // Обновляем характеристики предмета
                // Здесь нужно добавить интеграцию с вашей системой предметов
                
                currentLevels[newItem] = currentLevel + 1;
                UpdateUpgradeUI();
            }
        }
    }

    private bool HasEnoughResources(UpgradeCost cost)
    {
        // Проверяем наличие ресурсов через BuildingSystem
        return true; // Временная заглушка
    }

    private void SpendResources(UpgradeCost cost)
    {
        // Списываем ресурсы через BuildingSystem
        // Здесь нужно добавить интеграцию с BuildingSystem
    }

    // Методы для получения множителей улучшений
    public float GetDamageMultiplier(string itemName, GameObject item)
    {
        if (itemUpgradeDict.TryGetValue(itemName, out ItemUpgrade upgrade))
        {
            int level = GetCurrentLevel(item);
            if (level < upgrade.damageMultipliers.Length)
            {
                return upgrade.damageMultipliers[level];
            }
        }
        return 1f;
    }

    public float GetArmorMultiplier(string itemName, GameObject item)
    {
        if (itemUpgradeDict.TryGetValue(itemName, out ItemUpgrade upgrade))
        {
            int level = GetCurrentLevel(item);
            if (level < upgrade.armorMultipliers.Length)
            {
                return upgrade.armorMultipliers[level];
            }
        }
        return 1f;
    }
}
*/ 