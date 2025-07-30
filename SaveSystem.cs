// ВРЕМЕННО ОТКЛЮЧЕНО
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Добавляем для работы с TextMeshProUGUI
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class GameSaveData
{
    public PlayerSaveData player;
    public List<BuildingSaveData> buildings;
    public ResourcesSaveData resources;
    public List<QuestSaveData> quests;
    public List<NPCSaveData> npcs;
    public List<EnemySaveData> enemies;
    public DateTime saveTime;
    public string saveName;
}

[System.Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public float health;
    public Dictionary<string, int> inventory;
}

[System.Serializable]
public class BuildingSaveData
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public int level;
    public bool canBeUpgraded;
}

[System.Serializable]
public class ResourcesSaveData
{
    public int wood;
    public int stone;
    public int gold;
}

[System.Serializable]
public class QuestSaveData
{
    public int id;
    public bool isActive;
    public bool isCompleted;
    public List<QuestObjectiveSaveData> objectives;
}

[System.Serializable]
public class QuestObjectiveSaveData
{
    public int currentAmount;
    public bool isCompleted;
}

[System.Serializable]
public class NPCSaveData
{
    public string npcId;
    public Vector3 position;
    public Quaternion rotation;
    public bool isAlive;
}

[System.Serializable]
public class EnemySaveData
{
    public string enemyId;
    public Vector3 position;
    public Quaternion rotation;
    public float health;
    public bool isAlive;
}

public class SaveSystem : MonoBehaviour
{
    // Статический экземпляр для доступа из любого места
    public static SaveSystem Instance { get; private set; }

    private static string SavePath => Path.Combine(Application.persistentDataPath, "saves");
    private static string QuickSavePath => Path.Combine(SavePath, "quicksave.json");
    private const int MaxManualSaves = 10;

    [Header("UI элементы")]
    public GameObject pauseMenuCanvas;
    public GameObject saveLoadMenuCanvas;
    public Transform saveSlotContainer;
    public TextMeshProUGUI saveLoadTitleText; // Добавляем ссылку на текст заголовка

    private Player player;
    private BuildingSystem buildingSystem;
    private SimpleQuestSystem questSystem;
    private bool isPaused = false;
    
    private void Awake()
    {
        // Синглтон
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
        // Создаем директорию для сохранений, если её нет
        if (!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
        }

            // Проверяем наличие префаба слота сохранения
            // if (saveSlotPrefab == null) // Удалено
            // {
            //     // Пытаемся загрузить префаб из ресурсов
            //     saveSlotPrefab = Resources.Load<GameObject>("UI/SaveSlotPrefab");
                
            //     if (saveSlotPrefab == null)
            //     {
            //         Debug.LogWarning("SaveSlotPrefab не найден в ресурсах. Будет создан временный префаб при необходимости.");
                    
            //         // Создаем временный префаб и используем его
            //         CreateTemporarySaveSlotPrefab();
                    
            //         if (saveSlotPrefab != null)
            //         {
            //             Debug.Log("Временный префаб SaveSlotPrefab успешно создан.");
            //         }
            //     }
            //     else
            //     {
            //         Debug.Log("SaveSlotPrefab успешно загружен из ресурсов.");
            //     }
            // }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Получаем ссылки на необходимые системы
        player = FindFirstObjectByType<Player>();
        buildingSystem = FindFirstObjectByType<BuildingSystem>();
        questSystem = FindFirstObjectByType<SimpleQuestSystem>();
        
        // Проверяем наличие префаба слота сохранения
        // if (saveSlotPrefab == null) // Удалено
        // {
        //     Debug.Log("Пытаемся найти префаб слота сохранения...");
            
        //     // Проверяем наличие префаба в папке префабов
        //     saveSlotPrefab = Resources.Load<GameObject>("UI/SaveSlotPrefab");
            
        //     if (saveSlotPrefab == null)
        //     {
        //         Debug.LogWarning("Префаб SaveSlotPrefab не найден в Resources/UI, ищем в основной папке Resources...");
        //         saveSlotPrefab = Resources.Load<GameObject>("SaveSlotPrefab");
        //     }
            
        //     if (saveSlotPrefab == null)
        //     {
        //         Debug.LogWarning("Префаб не найден в Resources, ищем в папке префабов...");
        //         // Пытаемся загрузить префаб из папки префабов через AssetDatabase (работает только в редакторе)
        //         #if UNITY_EDITOR
        //         saveSlotPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefabs/UI/SaveSlotUI.prefab");
        //         #endif
                
        //         if (saveSlotPrefab != null)
        //         {
        //             Debug.Log("Префаб найден в папке Assets/prefabs/UI/SaveSlotUI.prefab");
        //         }
        //         else
        //         {
        //             Debug.LogError("Префаб слота сохранения не найден нигде! Создайте префаб SaveSlotPrefab в папке Resources/UI");
        //         }
        //     }
        //     else
        //     {
        //         Debug.Log("Префаб слота сохранения успешно загружен из Resources");
        //     }
        // }
        
        // Проверяем наличие необходимых компонентов UI
        if (saveLoadMenuCanvas == null)
        {
            Debug.LogError("saveLoadMenuCanvas не назначен!");
        }
        
        if (saveSlotContainer == null)
        {
            Debug.LogError("saveSlotContainer не назначен!");
        }
        
        if (saveLoadTitleText == null)
        {
            Debug.LogError("saveLoadTitleText не назначен!");
        }
            
        // Скрываем меню паузы при старте
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
            
        if (saveLoadMenuCanvas != null)
            saveLoadMenuCanvas.SetActive(false);
    }

    private void Update()
    {
        // Быстрое сохранение (F5)
        if (Input.GetKeyDown(KeyCode.F5))
        {
            QuickSave();
        }
        
        // Быстрая загрузка (F9)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            QuickLoad();
        }
        
        // Меню паузы (Escape)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }
    
    // Методы для вызова через кнопки в инспекторе
    
    // Метод для кнопки "Продолжить"
    public void BroadcastReturnToGame()
    {
        ReturnToGame();
    }
    
    // Метод для кнопки "Сохранить"
    public void BroadcastShowSaveMenu()
    {
        ShowSaveMenu();
    }
    
    // Метод для кнопки "Загрузить"
    public void BroadcastShowLoadMenu()
    {
        ShowLoadMenu();
    }
    
    // Метод для кнопки "Назад" в меню сохранения/загрузки
    public void BroadcastReturnToPauseMenu()
    {
        ReturnToPauseMenu();
    }
    
    // Метод для кнопки "Главное меню"
    public void BroadcastGoToMainMenu()
    {
        GoToMainMenu();
    }
    
    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseMenuCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Блокируем ввод игрока
            if (player != null)
                player.BlockInput(true);
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenuCanvas.SetActive(false);
            saveLoadMenuCanvas.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Разблокируем ввод игрока
            if (player != null)
                player.BlockInput(false);
        }
    }
    
    public void ShowSaveMenu()
    {
        Debug.Log("Показываем меню сохранения");
        
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
            
        if (saveLoadMenuCanvas != null)
        {
            saveLoadMenuCanvas.SetActive(true);
            
            // Обновляем заголовок
            if (saveLoadTitleText != null)
                saveLoadTitleText.text = "Сохранение игры";
                
            // Заполняем слоты сохранений
            PopulateSaveSlots(true);
            
            // Принудительно обновляем макет контейнера слотов
            ForceUpdateSaveSlotLayout();
        }
        else
        {
            Debug.LogError("saveLoadMenuCanvas не назначен!");
        }
    }
    
    public void ShowLoadMenu()
    {
        Debug.Log("Показываем меню загрузки");
        
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
            
        if (saveLoadMenuCanvas != null)
        {
            saveLoadMenuCanvas.SetActive(true);
            
            // Обновляем заголовок
            if (saveLoadTitleText != null)
                saveLoadTitleText.text = "Загрузка игры";
                
            // Заполняем слоты сохранений
            PopulateSaveSlots(false);
            
            // Принудительно обновляем макет контейнера слотов
            ForceUpdateSaveSlotLayout();
        }
        else
        {
            Debug.LogError("saveLoadMenuCanvas не назначен!");
        }
    }
    
    public void ReturnToPauseMenu()
    {
        saveLoadMenuCanvas.SetActive(false);
        pauseMenuCanvas.SetActive(true);
    }
    
    public void ReturnToGame()
    {
        TogglePauseMenu();
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // Здесь будет загрузка сцены главного меню
        // SceneManager.LoadScene("MainMenu");
        Debug.Log("Переход в главное меню");
    }
    
    private void PopulateSaveSlots(bool isSaveMode)
    {
        Debug.Log($"PopulateSaveSlots: начало заполнения слотов, режим сохранения: {isSaveMode}");
        
        // Проверяем наличие контейнера
        if (saveSlotContainer == null)
        {
            Debug.LogError("PopulateSaveSlots: saveSlotContainer не назначен!");
            return;
        }
        
        // Сначала деактивируем все слоты
        foreach (Transform child in saveSlotContainer)
        {
            child.gameObject.SetActive(false);
        }
        
        // Получаем список сохранений
        string[] saveFiles = GetSaveFiles();
        Debug.Log($"PopulateSaveSlots: найдено {saveFiles.Length} файлов сохранений");
        
        // Счетчик для отслеживания использованных слотов
        int slotIndex = 0;
        
        // Заполняем слоты для существующих сохранений
        for (int i = 0; i < saveFiles.Length && slotIndex < saveSlotContainer.childCount; i++)
        {
            Transform slotTransform = saveSlotContainer.GetChild(slotIndex);
            SaveSlotUI slotUI = slotTransform.GetComponent<SaveSlotUI>();
            
            if (slotUI != null)
            {
                string saveName = saveFiles[i];
                string savePath = Path.Combine(SavePath, saveName + ".json");
                GameSaveData saveData = LoadSaveMetadata(savePath);
                
                if (saveData != null)
                {
                    // Если дата сохранения некорректная (01.01.0001), устанавливаем текущую дату
                    if (saveData.saveTime.Year < 2000)
                    {
                        saveData.saveTime = DateTime.Now;
                    }
                    
                    slotUI.Initialize(saveData, isSaveMode);
                    
                    if (isSaveMode)
                    {
                        slotUI.SetButtonAction(() => ManualSave(saveName));
                    }
                    else
                    {
                        slotUI.SetButtonAction(() => ManualLoad(saveName));
                    }
                    
                    Debug.Log($"PopulateSaveSlots: слот для {saveName} успешно инициализирован");
                    slotIndex++;
                }
                else
                {
                    Debug.LogWarning($"PopulateSaveSlots: не удалось загрузить метаданные для {savePath}");
                }
            }
            else
            {
                Debug.LogError($"PopulateSaveSlots: слот {slotIndex} не содержит компонент SaveSlotUI!");
            }
        }
        
        // Добавляем новый слот для сохранения, если в режиме сохранения
        // Показываем только один слот для нового сохранения
        if (isSaveMode && slotIndex < saveSlotContainer.childCount)
        {
            Transform newSlotTransform = saveSlotContainer.GetChild(slotIndex);
            SaveSlotUI newSlotUI = newSlotTransform.GetComponent<SaveSlotUI>();
            
            if (newSlotUI != null)
            {
                int nextSaveNumber = saveFiles.Length + 1;
                string newSaveName = $"save_{nextSaveNumber}";
                
                newSlotUI.InitializeAsNew(newSaveName);
                newSlotUI.SetButtonAction(() => ManualSave(newSaveName));
                Debug.Log($"PopulateSaveSlots: слот для нового сохранения {newSaveName} успешно инициализирован");
                slotIndex++;
            }
            else
            {
                Debug.LogError($"PopulateSaveSlots: слот для нового сохранения не содержит компонент SaveSlotUI!");
            }
            
            // Добавляем кнопку "Быстрое сохранение" в режиме сохранения
            if (slotIndex < saveSlotContainer.childCount)
            {
                Transform quickSaveTransform = saveSlotContainer.GetChild(slotIndex);
                SaveSlotUI quickSaveUI = quickSaveTransform.GetComponent<SaveSlotUI>();
                
                if (quickSaveUI != null)
                {
                    quickSaveUI.InitializeQuickSave();
                    quickSaveUI.SetButtonAction(() => {
                        QuickSave();
                        // Обновляем слоты после сохранения
                        PopulateSaveSlots(true);
                    });
                    Debug.Log("PopulateSaveSlots: слот для быстрого сохранения успешно инициализирован");
                    slotIndex++;
                }
                else
                {
                    Debug.LogError("PopulateSaveSlots: слот для быстрого сохранения не содержит компонент SaveSlotUI!");
                }
            }
        }
        else if (isSaveMode)
        {
            // Если нет свободных слотов для нового сохранения, выводим предупреждение
            Debug.LogWarning("PopulateSaveSlots: нет свободных слотов для нового сохранения!");
        }
        
        Debug.Log($"PopulateSaveSlots: всего активировано {slotIndex} из {saveSlotContainer.childCount} слотов");
        
        // Принудительно обновляем макет
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(saveSlotContainer as RectTransform);
    }
    
    // Удаляем неиспользуемые методы:
    // - CreateSaveSlot
    // - CreateNewSaveSlot
    // - CreateQuickSaveSlot
    // - CreateTemporarySaveSlotPrefab
    
    // Оставляем только DelayedLayoutRebuild для использования при необходимости
    private IEnumerator DelayedLayoutRebuild()
    {
        // Ждем один кадр
        yield return null;
        
        // Обновляем макет
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(saveSlotContainer as RectTransform);
        
        // Проверяем все дочерние объекты
        foreach (Transform child in saveSlotContainer)
        {
            if (child.gameObject.activeSelf)
            {
                RectTransform rectTransform = child.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Debug.Log($"Слот {child.name}: localPosition={rectTransform.localPosition}, anchoredPosition={rectTransform.anchoredPosition}");
                }
            }
        }
    }
    
    // Загружает только метаданные сохранения для отображения в UI
    private GameSaveData LoadSaveMetadata(string filePath)
    {
        if (!File.Exists(filePath))
            return null;
            
        try
        {
            string jsonData = File.ReadAllText(filePath);
            return JsonUtility.FromJson<GameSaveData>(jsonData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при загрузке метаданных сохранения: {e.Message}");
            return null;
        }
    }

    public void QuickSave()
    {
        SaveGame(QuickSavePath, "Быстрое сохранение");
        Debug.Log("Быстрое сохранение создано");
    }

    public void QuickLoad()
    {
        if (File.Exists(QuickSavePath))
    {
        LoadGame(QuickSavePath);
            Debug.Log("Быстрое сохранение загружено");
        }
        else
        {
            Debug.LogWarning("Быстрое сохранение не найдено");
        }
    }

    public void ManualSave(string saveName)
    {
        string filePath = Path.Combine(SavePath, $"{saveName}.json");
        SaveGame(filePath, saveName);
        
        // Если открыто меню сохранения, обновляем его
        if (saveLoadMenuCanvas.activeSelf)
        {
            PopulateSaveSlots(true);
        }
        
        Debug.Log($"Игра сохранена в: {saveName}");
    }

    public void ManualLoad(string saveName)
    {
        string filePath = Path.Combine(SavePath, $"{saveName}.json");
        LoadGame(filePath);
        
        // Закрываем меню паузы
        TogglePauseMenu();
        
        Debug.Log($"Игра загружена из: {saveName}");
    }

    private void SaveGame(string filePath, string saveName)
    {
        try
        {
            GameSaveData saveData = new GameSaveData
            {
                player = SavePlayer(),
                buildings = SaveBuildings(),
                resources = SaveResources(),
                quests = SaveQuests(),
                npcs = SaveNPCs(),
                enemies = SaveEnemies(),
                saveTime = DateTime.Now,
                saveName = saveName
            };

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(filePath, json);
        
        // Удаляем старые сохранения, если их больше максимального количества
        string[] saveFiles = Directory.GetFiles(SavePath, "*.json")
            .Where(f => !f.EndsWith("quicksave.json"))
            .OrderByDescending(f => File.GetLastWriteTime(f))
            .ToArray();
            
        if (saveFiles.Length > MaxManualSaves)
        {
            for (int i = MaxManualSaves; i < saveFiles.Length; i++)
            {
                File.Delete(saveFiles[i]);
            }
        }
            
            Debug.Log($"Игра успешно сохранена в {filePath}");
            
            // Показываем уведомление об успешном сохранении
            if (buildingSystem != null)
            {
                buildingSystem.ShowNotification("Игра успешно сохранена!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при сохранении игры: {e.Message}");
            
            // Показываем уведомление об ошибке
            if (buildingSystem != null)
            {
                buildingSystem.ShowNotification("Ошибка при сохранении игры!");
            }
        }
    }

    private void LoadGame(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Файл сохранения не найден: {filePath}");
            return;
        }

        string jsonData = File.ReadAllText(filePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);

        LoadPlayer(saveData.player);
        LoadBuildings(saveData.buildings);
        LoadResources(saveData.resources);
        LoadQuests(saveData.quests);
        LoadNPCs(saveData.npcs);
        LoadEnemies(saveData.enemies);
        
        Debug.Log($"Игра загружена из: {filePath}");
    }

    private PlayerSaveData SavePlayer()
    {
        if (player == null)
            return null;

        return new PlayerSaveData
        {
            position = player.transform.position,
            rotation = player.transform.rotation,
            health = player.PlayerHealth,
            inventory = SaveInventory()
        };
    }
    
    private Dictionary<string, int> SaveInventory()
    {
        Dictionary<string, int> inventoryData = new Dictionary<string, int>();
        InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
        
        if (inventory != null)
        {
            // Здесь будет код для сохранения инвентаря
            // Пример: foreach (var item in inventory.items) { inventoryData[item.id] = item.count; }
        }
        
        return inventoryData;
    }

    private void LoadPlayer(PlayerSaveData data)
    {
        if (player == null || data == null)
            return;

        // Сохраняем текущий CharacterController
        CharacterController controller = player.GetComponent<CharacterController>();
        
        // Отключаем CharacterController, чтобы не мешал перемещению
        if (controller != null)
            controller.enabled = false;
            
        // Устанавливаем точную позицию и поворот
        player.transform.position = data.position;
        player.transform.rotation = data.rotation;
        
        // Включаем CharacterController обратно
        if (controller != null)
            controller.enabled = true;
        
        // Исправляем установку здоровья игрока - напрямую устанавливаем значение
        float currentHealth = player.PlayerHealth;
        if (data.health > currentHealth)
        {
            player.Heal(data.health - currentHealth);
        }
        else if (data.health < currentHealth)
        {
            player.TakeDamage(currentHealth - data.health);
        }
        
        LoadInventory(data.inventory);
        
        Debug.Log($"Позиция игрока загружена: {data.position}");
    }
    
    private void LoadInventory(Dictionary<string, int> inventoryData)
    {
        if (inventoryData == null)
            return;
            
        InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
        if (inventory != null)
        {
            // Здесь будет код для загрузки инвентаря
            // Пример: foreach (var item in inventoryData) { inventory.AddItem(item.Key, item.Value); }
        }
    }

    private List<BuildingSaveData> SaveBuildings()
    {
        List<BuildingSaveData> buildings = new List<BuildingSaveData>();
        
        foreach (GameObject building in GameObject.FindGameObjectsWithTag("Building"))
        {
            BuildingInstance buildingInstance = building.GetComponent<BuildingInstance>();
            bool canUpgrade = buildingInstance != null && buildingInstance.data.upgradeTo != null;
            int buildingLevel = 1; // По умолчанию уровень 1
            
            // Получаем уровень здания из поля level
            if (buildingInstance != null)
            {
                buildingLevel = buildingInstance.level;
            }
            
            buildings.Add(new BuildingSaveData
            {
                name = building.name.Replace("(Clone)", "").Trim(),
                position = building.transform.position,
                rotation = building.transform.rotation,
                level = buildingLevel,
                canBeUpgraded = canUpgrade
            });
        }
        
        return buildings;
    }

    private void LoadBuildings(List<BuildingSaveData> buildings)
    {
        if (buildingSystem == null || buildings == null)
            return;

        // Очищаем ссылку на текущее здание
        buildingSystem.ClearCurrentBuilding();

        // Удаляем все существующие здания
        foreach (GameObject building in GameObject.FindGameObjectsWithTag("Building"))
        {
            Destroy(building);
        }

        // Создаем здания из сохранения
        foreach (BuildingSaveData buildingData in buildings)
        {
            // Находим префаб здания по имени
            BuildingData buildingType = null;
            
            // Определяем тип здания по уровню
            if (buildingData.level <= 1)
                buildingType = buildingSystem.buildingLevel1;
            else
                buildingType = buildingSystem.buildingLevel2;
                
            if (buildingType != null)
            {
                // Создаем здание
                GameObject buildingInstance = Instantiate(buildingType.prefab, buildingData.position, buildingData.rotation);
                buildingInstance.name = buildingData.name;
                buildingInstance.tag = "Building"; // Убедимся, что тег установлен
                
                // Настраиваем компонент BuildingInstance
                BuildingInstance instance = buildingInstance.GetComponent<BuildingInstance>();
                if (instance == null)
                {
                    // Если компонента нет, добавляем его
                    instance = buildingInstance.AddComponent<BuildingInstance>();
                }
                
                if (instance != null)
                {
                    instance.data = buildingType;
                    instance.system = buildingSystem;
                    instance.level = buildingData.level; // Устанавливаем уровень здания
                }
                
                // Активируем все коллайдеры
                foreach (var c in buildingInstance.GetComponentsInChildren<Collider>())
                {
                    c.enabled = true;
                }
                
                // Регистрируем здание в системе строительства
                buildingSystem.RegisterCurrentBuilding(buildingInstance);
            }
        }
    }

    private ResourcesSaveData SaveResources()
    {
        if (buildingSystem == null)
            return null;

        return new ResourcesSaveData
        {
            wood = buildingSystem.wood,
            stone = buildingSystem.stone,
            gold = buildingSystem.gold
        };
    }

    private void LoadResources(ResourcesSaveData data)
    {
        if (buildingSystem == null || data == null)
            return;

        buildingSystem.wood = data.wood;
        buildingSystem.stone = data.stone;
        buildingSystem.gold = data.gold;
        buildingSystem.UpdateResourceUI();
    }

    private List<QuestSaveData> SaveQuests()
    {
        if (questSystem == null)
            return new List<QuestSaveData>();

        List<QuestSaveData> questsData = new List<QuestSaveData>();
        
        // Получаем все квесты из SimpleQuestSystem
        // Поскольку метод GetAllQuests() отсутствует, используем доступные данные
        if (questSystem is MonoBehaviour)
        {
            // Пытаемся получить квесты из публичного поля
            var questsField = questSystem.GetType().GetField("availableQuests", 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (questsField != null)
            {
                var quests = questsField.GetValue(questSystem) as Quest[];
                if (quests != null)
                {
                    foreach (var quest in quests)
                    {
                        QuestSaveData questData = new QuestSaveData
                        {
                            id = quest.id,
                            isActive = quest.isActive,
                            isCompleted = quest.isCompleted,
                            objectives = new List<QuestObjectiveSaveData>()
                        };
                        
                        foreach (var objective in quest.objectives)
                        {
                            questData.objectives.Add(new QuestObjectiveSaveData
                            {
                                currentAmount = objective.currentAmount,
                                isCompleted = objective.isCompleted
                            });
                        }
                        
                        questsData.Add(questData);
                    }
                }
            }
        }
        
        return questsData;
    }

    private void LoadQuests(List<QuestSaveData> quests)
    {
        if (questSystem == null || quests == null)
            return;

        // Поскольку методы ResetQuests, SetQuestObjectiveProgress, CompleteQuestObjective и CompleteQuest отсутствуют,
        // мы можем только попытаться получить доступ к внутренним данным через рефлексию
        
        // Пытаемся получить доступ к квестам
        var questsField = questSystem.GetType().GetField("availableQuests", 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
            
        if (questsField != null)
        {
            var availableQuests = questsField.GetValue(questSystem) as Quest[];
            if (availableQuests != null)
            {
                // Обновляем состояние квестов
                foreach (var questData in quests)
                {
                    foreach (var quest in availableQuests)
                    {
                        if (quest.id == questData.id)
                        {
                            quest.isActive = questData.isActive;
                            quest.isCompleted = questData.isCompleted;
                            
                            // Обновляем состояние задач
                            for (int i = 0; i < questData.objectives.Count && i < quest.objectives.Length; i++)
                            {
                                quest.objectives[i].currentAmount = questData.objectives[i].currentAmount;
                                quest.objectives[i].isCompleted = questData.objectives[i].isCompleted;
                            }
                            
                            break;
                        }
                    }
                }
                
                // Обновляем UI квестов, если есть такой метод
                var updateMethod = questSystem.GetType().GetMethod("UpdateQuestDisplay", 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                    
                if (updateMethod != null)
                {
                    updateMethod.Invoke(questSystem, null);
                }
            }
        }
    }
    
    private List<NPCSaveData> SaveNPCs()
    {
        List<NPCSaveData> npcsData = new List<NPCSaveData>();
        
        // Находим всех NPC на сцене
        foreach (FriendlyNPC npc in FindObjectsByType<FriendlyNPC>(FindObjectsSortMode.None))
        {
            npcsData.Add(new NPCSaveData
            {
                npcId = npc.name,
                position = npc.transform.position,
                rotation = npc.transform.rotation,
                isAlive = true // FriendlyNPC всегда живы
            });
        }
        
        return npcsData;
    }
    
    private void LoadNPCs(List<NPCSaveData> npcs)
    {
        if (npcs == null)
            return;

        // Загружаем состояние NPC
        foreach (NPCSaveData npcData in npcs)
        {
            // Находим NPC по ID
            FriendlyNPC npc = FindObjectsByType<FriendlyNPC>(FindObjectsSortMode.None)
                .FirstOrDefault(n => n.name == npcData.npcId);
                
            if (npc != null)
            {
                // Устанавливаем позицию и поворот
                npc.transform.position = npcData.position;
                npc.transform.rotation = npcData.rotation;
            }
        }
    }
    
    private List<EnemySaveData> SaveEnemies()
    {
        List<EnemySaveData> enemiesData = new List<EnemySaveData>();
        
        // Находим всех врагов на сцене
        foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            // Получаем здоровье через рефлексию, если свойство Health отсутствует
            float health = 100f; // Значение по умолчанию
            bool isAlive = true;
            
            // Пытаемся получить здоровье через рефлексию
            var healthProperty = enemy.GetType().GetProperty("Health");
            if (healthProperty != null)
            {
                health = (float)healthProperty.GetValue(enemy);
                isAlive = health > 0;
            }
            else
            {
                // Пробуем получить через поле
                var healthField = enemy.GetType().GetField("health", 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                    
                if (healthField != null)
                {
                    health = (float)healthField.GetValue(enemy);
                    isAlive = health > 0;
                }
            }
            
            enemiesData.Add(new EnemySaveData
            {
                enemyId = enemy.name,
                position = enemy.transform.position,
                rotation = enemy.transform.rotation,
                health = health,
                isAlive = isAlive
            });
        }
        
        return enemiesData;
    }
    
    private void LoadEnemies(List<EnemySaveData> enemies)
    {
        if (enemies == null)
            return;
            
        // Получаем всех врагов на сцене
        Enemy[] sceneEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        // Загружаем состояние врагов
        foreach (EnemySaveData enemyData in enemies)
        {
            // Находим врага по ID
            Enemy enemy = sceneEnemies.FirstOrDefault(e => e.name == enemyData.enemyId);
                
            if (enemy != null)
            {
                if (enemyData.isAlive)
                {
                    // Устанавливаем позицию и поворот
                    enemy.transform.position = enemyData.position;
                    enemy.transform.rotation = enemyData.rotation;
                    
                    // Устанавливаем здоровье через рефлексию
                    var setHealthMethod = enemy.GetType().GetMethod("SetHealth");
                    if (setHealthMethod != null)
                    {
                        setHealthMethod.Invoke(enemy, new object[] { enemyData.health });
                    }
                    else
                    {
                        // Пробуем установить через поле
                        var healthField = enemy.GetType().GetField("health", 
                            System.Reflection.BindingFlags.Public | 
                            System.Reflection.BindingFlags.NonPublic | 
                            System.Reflection.BindingFlags.Instance);
                            
                        if (healthField != null)
                        {
                            healthField.SetValue(enemy, enemyData.health);
                        }
                    }
                }
                else
                {
                    // Если враг мертв, уничтожаем его
                    Destroy(enemy.gameObject);
                }
            }
        }
    }

    public string[] GetSaveFiles()
    {
        if (!Directory.Exists(SavePath))
            return new string[0];

        return Directory.GetFiles(SavePath, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name != "quicksave")
            .OrderByDescending(name => File.GetLastWriteTime(Path.Combine(SavePath, name + ".json")))
            .ToArray();
    }

    // Метод для принудительного обновления макета контейнера слотов
    public void ForceUpdateSaveSlotLayout()
    {
        if (saveSlotContainer != null)
        {
            Debug.Log("Принудительное обновление макета контейнера слотов");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(saveSlotContainer as RectTransform);
            StartCoroutine(DelayedLayoutRebuild());
        }
    }
} 