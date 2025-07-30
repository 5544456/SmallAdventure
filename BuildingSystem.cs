using UnityEngine;
using TMPro;

public class BuildingSystem : MonoBehaviour
{
    [Header("Данные зданий")]
    public BuildingData buildingLevel1;
    public BuildingData buildingLevel2;

    [Header("Ресурсы игрока")]
    public int wood = 1000;
    public int stone = 50;
    public int gold = 1000;

    [Header("UI и предпросмотр")]
    public BuildUIController buildUI;
    public Material previewGreen;
    public Material previewRed;
    public GameObject notificationCanvas;
    public TextMeshProUGUI notificationText;

    [Header("Тексты ресурсов (UI)")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;

    [Header("Прочее")]
    public LayerMask groundLayer;
    public float raycastDistance = 100f;

    private GameObject previewInstance;
    public BuildingData selectedBuilding;
    private bool isPreviewActive = false;
    private bool canBuild = false;
    public bool isBuildMenuOpen = false;
    public GameObject currentBuildingInstance;
    private bool wasPreviewMoved = false;
    private Player cachedPlayer;
    public bool isUpgradeMode = false;
    private GameObject upgradeTargetBuilding = null;
    private GameObject upgradePreviewTarget = null;

    void Start()
    {
        UpdateResourceUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            // Проверяем, не активен ли диалог
            DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
            if (dialogueManager != null && dialogueManager.IsDialogueActive())
            {
                // Если диалог активен, не открываем меню строительства
                Debug.Log("[BuildingSystem] Невозможно открыть меню строительства во время диалога");
                return;
            }
            
            if (isBuildMenuOpen)
                CloseBuildMenu();
            else
                OpenBuildMenu();
        }

        if (isPreviewActive)
        {
            UpdatePreview();
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (canBuild)
                {
                    PlaceBuilding();
                }
                else
                {
                    ShowNotification("Здание нельзя построить здесь!");
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                CancelPreview();
            }
        }
    }

    public void OpenBuildMenu()
    {
        // Проверяем, не открыт ли инвентарь, и закрываем его если открыт
        var inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager != null && inventoryManager.IsInventoryOpen)
        {
            inventoryManager.ToggleInventory();
        }
        
        Time.timeScale = 0f;
        isBuildMenuOpen = true;
        buildUI.ShowMenu(this);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (cachedPlayer == null) cachedPlayer = FindFirstObjectByType<Player>();
        if (cachedPlayer != null) cachedPlayer.BlockInput(true);
    }

    public void CloseBuildMenu()
    {
        Time.timeScale = 1f;
        isBuildMenuOpen = false;
        buildUI.HideMenu();
        if (notificationCanvas != null) notificationCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CancelPreview();
        if (cachedPlayer == null) cachedPlayer = FindFirstObjectByType<Player>();
        if (cachedPlayer != null) cachedPlayer.BlockInput(false);
    }

    public void SelectBuilding(BuildingData building)
    {
        if (building == null)
        {
            ShowNotification("Здание не выбрано (building=null)!");
            return;
        }
        selectedBuilding = building;
        if (!HasResources(selectedBuilding))
        {
            ShowNotification("Недостаточно ресурсов!");
            return;
        }
        Debug.Log($"[BuildingSystem] Выбрано здание: {selectedBuilding.buildingName}");
        CloseBuildMenu();
        StartPreview();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateResourceUI();
    }

    public void StartUpgradeMode(GameObject targetBuilding)
    {
        isUpgradeMode = true;
        upgradeTargetBuilding = targetBuilding;
        selectedBuilding = upgradeTargetBuilding.GetComponent<BuildingInstance>().data.upgradeTo;
        upgradePreviewTarget = targetBuilding;
        StartPreview();
    }

    private void StartPreview()
    {
        if (selectedBuilding == null)
        {
            ShowNotification("Здание не выбрано!");
            return;
        }
        if (selectedBuilding.prefab == null)
        {
            ShowNotification("Prefab здания не назначен!");
            return;
        }
        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = Instantiate(selectedBuilding.prefab);
        // Отключаем все коллайдеры у previewInstance для предпросмотра
        foreach (var c in previewInstance.GetComponentsInChildren<Collider>())
            c.enabled = false;
        SetPreviewMaterial(previewGreen);
        if (isUpgradeMode && upgradeTargetBuilding != null)
        {
            previewInstance.transform.position = upgradeTargetBuilding.transform.position;
            previewInstance.transform.rotation = upgradeTargetBuilding.transform.rotation;
        }
        isPreviewActive = true;
    }

    private void UpdatePreview()
    {
        if (previewInstance == null) return;
        if (isUpgradeMode && upgradeTargetBuilding != null)
        {
            // Предпросмотр появляется ровно на позиции и с ротацией первого здания
            previewInstance.transform.position = upgradeTargetBuilding.transform.position;
            previewInstance.transform.rotation = upgradeTargetBuilding.transform.rotation;
            canBuild = CheckBuildable(upgradeTargetBuilding.transform.position);
            SetPreviewMaterial(canBuild ? previewGreen : previewRed);
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
        {
            Vector3 hitPoint = hit.point;
            previewInstance.transform.position = hitPoint;
            canBuild = CheckBuildable(hitPoint);
            SetPreviewMaterial(canBuild ? previewGreen : previewRed);
        }
        // Если мышка не наведена на землю — предпросмотр не двигается
    }

    private void SetPreviewMaterial(Material mat)
    {
        foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
            r.material = mat;
    }

    private bool CheckBuildable(Vector3 pos)
    {
        if (isUpgradeMode && upgradeTargetBuilding != null)
        {
            // Можно строить только на месте здания первого уровня
            float dist = Vector3.Distance(pos, upgradeTargetBuilding.transform.position);
            if (dist > 0.1f) return false;
            var inst = upgradeTargetBuilding.GetComponent<BuildingInstance>();
            if (inst == null || inst.data.upgradeTo == null) return false;
            
            // Проверяем возможность размещения здания на земле
            Vector3 upgradeCenter = upgradeTargetBuilding.transform.position;
            RaycastHit upgradeHit;
            if (Physics.Raycast(upgradeCenter + Vector3.up * 10f, Vector3.down, out upgradeHit, 20f, groundLayer))
            {
                return true;
            }
            return false;
        }
        
        // Обычная проверка для постройки нового здания
        // Сначала проверяем коллизии с другими зданиями
        Collider[] colliders = Physics.OverlapBox(pos, previewInstance.GetComponent<Collider>().bounds.extents, previewInstance.transform.rotation);
        foreach (var c in colliders)
        {
            if (c.gameObject.CompareTag("Building"))
                return false;
        }
        
        // Теперь проверяем, можно ли разместить здание на земле
        Vector3 buildCenter = pos;
        RaycastHit buildHit;
        if (Physics.Raycast(buildCenter + Vector3.up * 10f, Vector3.down, out buildHit, 20f, groundLayer))
        {
            return true;
        }
        return false;
    }

    private void PlaceBuilding()
    {
        if (isUpgradeMode && upgradeTargetBuilding != null)
        {
            var upgradeInst = upgradeTargetBuilding.GetComponent<BuildingInstance>();
            if (upgradeInst == null || upgradeInst.data.upgradeTo == null) return;
            if (!HasResources(upgradeInst.data.upgradeTo))
            {
                ShowNotification("Недостаточно ресурсов для улучшения!");
                return;
            }
            // Сохраняем только координаты X и Z старого здания
            float oldX = upgradeTargetBuilding.transform.position.x;
            float oldZ = upgradeTargetBuilding.transform.position.z;
            
            // Удаляем старое здание полностью
            Destroy(upgradeTargetBuilding);
            
            // Создаём новое здание второго уровня с нормальным масштабом и без сохранения поворота
            // Используем примерную высоту земли как начальную координату
            GameObject newBuilding = Instantiate(upgradeInst.data.upgradeTo.prefab, new Vector3(oldX, 76f, oldZ), Quaternion.identity);
            newBuilding.transform.localScale = Vector3.one;
            newBuilding.tag = "Building";
            var newInst = newBuilding.AddComponent<BuildingInstance>();
            newInst.data = upgradeInst.data.upgradeTo;
            newInst.system = this;
            newInst.level = 2; // Устанавливаем уровень здания на 2, так как это улучшенное здание
            foreach (var c in newBuilding.GetComponentsInChildren<Collider>())
                c.enabled = true;
                
            // Находим точку земли под зданием и устанавливаем здание на землю
            bool upgradePlaced = PlaceBuildingOnGround(newBuilding);
            
            if (!upgradePlaced)
            {
                // Если не удалось разместить здание на земле
                ShowNotification("Здесь здание построить нельзя!");
                Destroy(newBuilding);
                isPreviewActive = false;
                isUpgradeMode = false;
                upgradeTargetBuilding = null;
                upgradePreviewTarget = null;
                Destroy(previewInstance);
                return;
            }
                
            SpendResources(newInst.data);
            Destroy(previewInstance);
            isPreviewActive = false;
            isUpgradeMode = false;
            upgradeTargetBuilding = null;
            upgradePreviewTarget = null;
            currentBuildingInstance = newBuilding;
            
            // Регистрируем здание в системе строительства
            RegisterCurrentBuilding(newBuilding);
            
            // После апгрейда обновить UI
            if (isBuildMenuOpen && buildUI != null) buildUI.ShowMenu(this);
            return;
        }
        
        // Строим новое здание (убрано условие currentBuildingInstance == null)
        GameObject go = Instantiate(selectedBuilding.prefab, previewInstance.transform.position, previewInstance.transform.rotation);
        go.tag = "Building";
        var buildingInst = go.AddComponent<BuildingInstance>();
        buildingInst.data = selectedBuilding;
        buildingInst.system = this;
        buildingInst.level = 1; // Устанавливаем уровень здания на 1, так как это новое здание
        foreach (var c in go.GetComponentsInChildren<Collider>())
            c.enabled = true;
        
        // Используем новый метод для размещения здания на земле
        bool buildingPlaced = PlaceBuildingOnGround(go);
        
        if (!buildingPlaced)
        {
            // Если не удалось разместить здание на земле
            ShowNotification("Здесь здание построить нельзя!");
            Destroy(go);
            return;
        }
        
        SpendResources(selectedBuilding);
        Destroy(previewInstance);
        isPreviewActive = false;
        currentBuildingInstance = go; // Сохраняем ссылку на последнее построенное здание
        
        // Регистрируем здание в системе строительства
        RegisterCurrentBuilding(go);
        
        // После постройки обновить UI, чтобы появилась кнопка улучшения
        if (isBuildMenuOpen && buildUI != null) buildUI.ShowMenu(this);
        
        Debug.Log("Здание успешно построено!");
    }
    
    // Упрощенный метод для установки здания на уровень земли
    private bool PlaceBuildingOnGround(GameObject building)
    {
        // Получаем все коллайдеры здания
        Collider[] colliders = building.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            // Находим самую нижнюю точку всех коллайдеров
            float minY = float.MaxValue;
            foreach (var col in colliders)
            {
                minY = Mathf.Min(minY, col.bounds.min.y);
            }
            
            // Вычисляем смещение по Y для установки здания на землю
            float offsetY = building.transform.position.y - minY;
            
            // Центр здания
            Vector3 center = building.transform.position;
            
            // Определяем смещение по Y в зависимости от уровня здания
            float yOffset = -1.5f; // По умолчанию для зданий первого уровня (дополнительно снижено на 1)
            
            // Если это улучшенное здание (второго уровня), используем другое смещение
            if (isUpgradeMode)
            {
                yOffset = -1.0f; // Для зданий второго уровня
            }
            
            // Выполняем рейкаст вниз для определения высоты земли
            RaycastHit hit;
            if (Physics.Raycast(center + Vector3.up * 10f, Vector3.down, out hit, 20f, groundLayer))
            {
                // Устанавливаем здание точно на уровень земли, сохраняя X и Z координаты
                // Добавляем соответствующее смещение по Y
                building.transform.position = new Vector3(
                    building.transform.position.x,
                    hit.point.y + offsetY + yOffset,
                    building.transform.position.z
                );
                return true;
            }
            
            // Если рейкаст не попал в землю, но это улучшение здания,
            // то оставляем здание на той же высоте, что и было, но со смещением
            if (isUpgradeMode)
            {
                Debug.Log("Не найдена земля под зданием, но это улучшение. Оставляем на текущей высоте со смещением.");
                building.transform.position = new Vector3(
                    building.transform.position.x,
                    building.transform.position.y - 1.0f, // Для зданий второго уровня
                    building.transform.position.z
                );
                return true;
            }
            else
            {
                // Для зданий первого уровня
                building.transform.position = new Vector3(
                    building.transform.position.x,
                    building.transform.position.y - 1.5f, // Дополнительно снижено на 1
                    building.transform.position.z
                );
                return true;
            }
        }
        return false;
    }

    private bool HasResources(BuildingData data)
    {
        return wood >= data.woodCost && stone >= data.stoneCost && gold >= data.goldCost;
    }
    private void SpendResources(BuildingData data)
    {
        wood -= data.woodCost;
        stone -= data.stoneCost;
        gold -= data.goldCost;
        UpdateResourceUI();
    }
    public void ShowNotification(string msg)
    {
        notificationCanvas.SetActive(true);
        notificationText.text = msg;
        CancelInvoke(nameof(HideNotification));
        Invoke(nameof(HideNotification), 2f);
    }
    private void HideNotification()
    {
        notificationCanvas.SetActive(false);
    }
    public void UpdateResourceUI()
    {
        if (goldText != null) goldText.text = gold.ToString();
        if (woodText != null) woodText.text = wood.ToString();
        if (stoneText != null) stoneText.text = stone.ToString();
    }
    public void CancelPreview()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
        isPreviewActive = false;
        // selectedBuilding = null; // Не сбрасываем выбранное здание!
    }

    public bool IsAnyUIOpen()
    {
        return isBuildMenuOpen || (FindFirstObjectByType<InventoryManager>()?.IsInventoryOpen ?? false);
    }

    // Метод для сохранения ссылки на текущее здание
    public void RegisterCurrentBuilding(GameObject building)
    {
        if (building != null && building.CompareTag("Building"))
        {
            currentBuildingInstance = building;
            Debug.Log($"Зарегистрировано здание: {building.name}");
        }
    }
    
    // Метод для очистки ссылки на текущее здание
    public void ClearCurrentBuilding()
    {
        currentBuildingInstance = null;
    }
} 