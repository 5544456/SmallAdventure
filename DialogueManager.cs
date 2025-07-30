using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button[] responseButtons;
    [SerializeField] private GameObject responsesPanel;

    [Header("Настройки")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private AudioSource dialogueAudio;
    
    [Header("Другие канвасы для блокировки")]
    [SerializeField] private GameObject[] canvasesToBlock;
    [SerializeField] private GameObject hpSpCanvas; // Канвас с HP/SP интерфейсом

    private DialogueData currentDialogue;
    private int currentNodeIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private FriendlyNPC currentNPC;
    private Player player;
    private InventoryManager inventoryManager;
    private BuildingSystem buildingSystem;
    private bool isDialogueActive = false;

    private void Start()
    {
        // Проверяем наличие диалоговой панели
        CheckDialoguePanel();

        // Скрываем панель ответов
        if (responsesPanel != null)
        {
            responsesPanel.SetActive(false);
        }

        // Настраиваем кнопку "Далее"
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(DisplayNextDialogueLine);
        }

        // Получаем ссылки на менеджеры
        player = FindFirstObjectByType<Player>();
        inventoryManager = FindFirstObjectByType<InventoryManager>();
        buildingSystem = FindFirstObjectByType<BuildingSystem>();
    }

    // Метод для проверки и настройки диалоговой панели
    private void CheckDialoguePanel()
    {
        if (dialoguePanel == null)
        {
            Debug.LogWarning("DialogueManager: Панель диалога не назначена! Создаем временную панель...");
            CreateTemporaryDialoguePanel();
        }
        else
        {
            // Проверяем, что панель находится в активной иерархии
            Transform parent = dialoguePanel.transform.parent;
            bool isInActiveHierarchy = true;
            
            while (parent != null)
            {
                if (!parent.gameObject.activeInHierarchy)
                {
                    isInActiveHierarchy = false;
                    Debug.LogWarning($"DialogueManager: Родительский объект {parent.name} неактивен!");
                    break;
                }
                parent = parent.parent;
            }
            
            if (!isInActiveHierarchy)
            {
                Debug.LogWarning("DialogueManager: Панель диалога находится в неактивной иерархии!");
            }
            
            // Скрываем панель диалога при запуске
            dialoguePanel.SetActive(false);
        }
    }

    // Метод для создания временной диалоговой панели
    private void CreateTemporaryDialoguePanel()
    {
        // Проверяем, существует ли уже диалоговая панель
        if (dialoguePanel != null)
        {
            Debug.Log("DialogueManager: Используем существующую диалоговую панель");
            // Не удаляем существующую панель, а просто убеждаемся, что она настроена правильно
            Canvas canvas = dialoguePanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
                canvas.sortingOrder = 100;
            }
            return;
        }
        
        // Проверяем, существует ли уже канвас для диалога
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        GameObject canvasObj;
        
        if (existingCanvas != null && existingCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Debug.Log("DialogueManager: Используем существующий Canvas");
            canvasObj = existingCanvas.gameObject;
        }
        else
        {
            // Создаем новый Canvas для диалога
            Debug.Log("DialogueManager: Создаем новый Canvas");
            canvasObj = new GameObject("DialogueCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Создаем панель диалога
        GameObject panelObj = new GameObject("DialoguePanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        dialoguePanel = panelObj;
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.4f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Создаем текст имени говорящего
        GameObject nameObj = new GameObject("SpeakerName");
        nameObj.transform.SetParent(panelObj.transform, false);
        speakerNameText = nameObj.AddComponent<TextMeshProUGUI>();
        speakerNameText.fontSize = 24;
        speakerNameText.color = Color.yellow;
        speakerNameText.alignment = TextAlignmentOptions.TopLeft;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.05f, 0.7f);
        nameRect.anchorMax = new Vector2(0.95f, 0.95f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        // Создаем текст диалога
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(panelObj.transform, false);
        dialogueText = textObj.AddComponent<TextMeshProUGUI>();
        dialogueText.fontSize = 18;
        dialogueText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.2f);
        textRect.anchorMax = new Vector2(0.95f, 0.7f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Создаем кнопку "Далее"
        GameObject buttonObj = new GameObject("NextButton");
        buttonObj.transform.SetParent(panelObj.transform, false);
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.8f, 1f);
        nextButton = button;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.8f, 0.05f);
        buttonRect.anchorMax = new Vector2(0.95f, 0.15f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Далее";
        buttonText.fontSize = 16;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        // Создаем панель ответов
        GameObject responsesPanelObj = new GameObject("ResponsesPanel");
        responsesPanelObj.transform.SetParent(panelObj.transform, false);
        responsesPanel = responsesPanelObj;
        
        RectTransform responsesPanelRect = responsesPanelObj.GetComponent<RectTransform>();
        responsesPanelRect.anchorMin = new Vector2(0.05f, 0.05f);
        responsesPanelRect.anchorMax = new Vector2(0.75f, 0.2f);
        responsesPanelRect.offsetMin = Vector2.zero;
        responsesPanelRect.offsetMax = Vector2.zero;
        
        // Создаем несколько кнопок ответов
        responseButtons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject respButtonObj = new GameObject($"ResponseButton{i}");
            respButtonObj.transform.SetParent(responsesPanelObj.transform, false);
            
            Button respButton = respButtonObj.AddComponent<Button>();
            Image respButtonImage = respButtonObj.AddComponent<Image>();
            respButtonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            responseButtons[i] = respButton;
            
            RectTransform respButtonRect = respButtonObj.GetComponent<RectTransform>();
            float height = 1f / 3f;
            respButtonRect.anchorMin = new Vector2(0f, 1f - (i + 1) * height);
            respButtonRect.anchorMax = new Vector2(1f, 1f - i * height);
            respButtonRect.offsetMin = new Vector2(5f, 2f);
            respButtonRect.offsetMax = new Vector2(-5f, -2f);
            
            GameObject respButtonTextObj = new GameObject("Text");
            respButtonTextObj.transform.SetParent(respButtonObj.transform, false);
            TextMeshProUGUI respButtonText = respButtonTextObj.AddComponent<TextMeshProUGUI>();
            respButtonText.text = $"Ответ {i+1}";
            respButtonText.fontSize = 14;
            respButtonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform respButtonTextRect = respButtonTextObj.GetComponent<RectTransform>();
            respButtonTextRect.anchorMin = Vector2.zero;
            respButtonTextRect.anchorMax = Vector2.one;
            respButtonTextRect.offsetMin = new Vector2(10f, 0f);
            respButtonTextRect.offsetMax = new Vector2(-10f, 0f);
        }
        
        // Настраиваем обработчик нажатия
        button.onClick.AddListener(DisplayNextDialogueLine);
        
        // Скрываем панель до начала диалога
        dialoguePanel.SetActive(false);
        responsesPanel.SetActive(false);
        
        Debug.Log("DialogueManager: Временная диалоговая панель создана");
    }

    public void StartDialogue(DialogueData dialogue, FriendlyNPC npc)
    {
        if (dialogue == null || dialogue.nodes.Count == 0) 
        {
            Debug.LogError("DialogueManager: Диалоговые данные отсутствуют или пусты!");
            return;
        }

        Debug.Log("DialogueManager: Начинаем диалог с " + (npc != null ? npc.GetNPCName() : "неизвестный NPC"));

        // Сохраняем текущий диалог и NPC
        currentDialogue = dialogue;
        currentNPC = npc;
        currentNodeIndex = 0;
        isDialogueActive = true;

        // Показываем панель диалога
        if (dialoguePanel != null)
        {
            Debug.Log("DialogueManager: Активируем панель диалога");
            
            // Активируем панель
            dialoguePanel.SetActive(true);
            
            // Проверяем, видима ли панель
            Canvas canvas = dialoguePanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"DialogueManager: Canvas панели диалога - enabled: {canvas.enabled}, " +
                          $"sortingOrder: {canvas.sortingOrder}, renderMode: {canvas.renderMode}");
                
                // Убедимся, что Canvas включен и имеет высокий sortingOrder
                canvas.enabled = true;
                canvas.sortingOrder = 100; // Высокий приоритет отображения
                
                CanvasGroup canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    Debug.Log($"DialogueManager: CanvasGroup - alpha: {canvasGroup.alpha}, " +
                              $"interactable: {canvasGroup.interactable}, blocksRaycasts: {canvasGroup.blocksRaycasts}");
                    
                    // Убедимся, что CanvasGroup настроен правильно
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
            }
            else
            {
                Debug.LogWarning("DialogueManager: Панель диалога не имеет родительского Canvas!");
            }
        }
        else
        {
            Debug.LogError("DialogueManager: Панель диалога не назначена в инспекторе!");
            CreateTemporaryDialoguePanel();
            dialoguePanel.SetActive(true);
        }

        // Скрываем подсказку взаимодействия у NPC
        if (currentNPC != null)
        {
            currentNPC.HideInteractionPrompt();
        }

        // Блокируем управление игроком
        if (player != null)
        {
            player.BlockInput(true);
        }
        
        // Закрываем другие канвасы и блокируем их открытие
        CloseAndBlockOtherCanvases();

        // Отображаем первую строку диалога
        DisplayDialogueNode(currentDialogue.nodes[currentNodeIndex]);
    }
    
    // Метод для закрытия и блокировки других канвасов
    private void CloseAndBlockOtherCanvases()
    {
        // Закрываем инвентарь, если он открыт
        if (inventoryManager != null && inventoryManager.IsInventoryOpen)
        {
            inventoryManager.ToggleInventory();
        }
        
        // Закрываем меню строительства, если оно открыто
        if (buildingSystem != null && buildingSystem.isBuildMenuOpen)
        {
            buildingSystem.CloseBuildMenu();
        }
        
        // Скрываем канвас HP/SP
        if (hpSpCanvas != null)
        {
            hpSpCanvas.SetActive(false);
        }
        
        // Отключаем указанные в инспекторе канвасы
        if (canvasesToBlock != null)
        {
            foreach (GameObject canvas in canvasesToBlock)
            {
                if (canvas != null)
                {
                    canvas.SetActive(false);
                }
            }
        }
    }
    
    // Метод для разблокировки других канвасов
    private void UnblockOtherCanvases()
    {
        isDialogueActive = false;
    }

    private void DisplayDialogueNode(DialogueNode node)
    {
        // Отображаем имя говорящего
        if (speakerNameText != null)
        {
            if (node.isPlayerSpeaking)
            {
                speakerNameText.text = "Вы";
            }
            else
            {
                speakerNameText.text = currentNPC != null ? currentNPC.GetNPCName() : "NPC";
            }
        }

        // Отображаем текст диалога с эффектом печатания
        if (dialogueText != null)
        {
            // Останавливаем предыдущий корутин, если он запущен
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeDialogue(node.text));
        }

        // Скрываем кнопки ответов, пока не закончится печатание
        if (responsesPanel != null)
        {
            responsesPanel.SetActive(false);
        }

        // Если есть ответы, настраиваем кнопки ответов
        if (node.responses.Count > 0)
        {
            // Скрываем кнопку "Далее"
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(false);
            }

            // Настраиваем кнопки ответов после завершения печатания
        }
        else
        {
            // Показываем кнопку "Далее"
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator TypeDialogue(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            
            // Воспроизводим звук печатания, если он есть
            if (dialogueAudio != null)
            {
                dialogueAudio.Play();
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // Если у текущего узла есть ответы, показываем их
        if (currentDialogue.nodes[currentNodeIndex].responses.Count > 0)
        {
            DisplayResponses();
        }
    }

    private void DisplayResponses()
    {
        if (responsesPanel == null || responseButtons == null || responseButtons.Length == 0) return;

        // Показываем панель ответов
        responsesPanel.SetActive(true);

        // Получаем ответы текущего узла
        List<DialogueResponse> responses = currentDialogue.nodes[currentNodeIndex].responses;

        // Настраиваем кнопки ответов
        for (int i = 0; i < responseButtons.Length; i++)
        {
            if (i < responses.Count)
            {
                // Показываем кнопку и настраиваем её текст
                responseButtons[i].gameObject.SetActive(true);
                TextMeshProUGUI buttonText = responseButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = responses[i].text;
                }

                // Настраиваем обработчик нажатия
                int responseIndex = i; // Сохраняем индекс для использования в лямбда-выражении
                responseButtons[i].onClick.RemoveAllListeners();
                responseButtons[i].onClick.AddListener(() => SelectResponse(responseIndex));
            }
            else
            {
                // Скрываем лишние кнопки
                responseButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectResponse(int responseIndex)
    {
        if (currentDialogue == null || currentNodeIndex >= currentDialogue.nodes.Count) return;

        DialogueNode currentNode = currentDialogue.nodes[currentNodeIndex];
        if (responseIndex >= currentNode.responses.Count) return;

        // Получаем выбранный ответ
        DialogueResponse selectedResponse = currentNode.responses[responseIndex];

        // Переходим к следующему узлу диалога
        if (selectedResponse.nextNodeIndex >= 0 && selectedResponse.nextNodeIndex < currentDialogue.nodes.Count)
        {
            currentNodeIndex = selectedResponse.nextNodeIndex;
            DisplayDialogueNode(currentDialogue.nodes[currentNodeIndex]);
        }
        else
        {
            // Если следующего узла нет, завершаем диалог
            EndDialogue();
        }
    }

    public void DisplayNextDialogueLine()
    {
        // Если сейчас идет печатание, показываем весь текст сразу
        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            
            if (dialogueText != null && currentDialogue != null && currentNodeIndex < currentDialogue.nodes.Count)
            {
                dialogueText.text = currentDialogue.nodes[currentNodeIndex].text;
                isTyping = false;
                
                // Если у текущего узла есть ответы, показываем их
                if (currentDialogue.nodes[currentNodeIndex].responses.Count > 0)
                {
                    DisplayResponses();
                }
            }
            
            return;
        }

        // Переходим к следующему узлу диалога
        currentNodeIndex++;

        // Проверяем, есть ли еще узлы диалога
        if (currentDialogue != null && currentNodeIndex < currentDialogue.nodes.Count)
        {
            DisplayDialogueNode(currentDialogue.nodes[currentNodeIndex]);
        }
        else
        {
            // Если узлов больше нет, завершаем диалог
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        // Скрываем панель диалога
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Разблокируем управление игроком
        if (player != null)
        {
            player.BlockInput(false);
        }

        // Сообщаем NPC о завершении диалога
        if (currentNPC != null)
        {
            currentNPC.EndInteraction();
        }
        
        // Показываем канвас HP/SP
        if (hpSpCanvas != null)
        {
            hpSpCanvas.SetActive(true);
        }
        
        // Разблокируем другие канвасы
        UnblockOtherCanvases();

        // Сбрасываем текущий диалог
        currentDialogue = null;
        currentNodeIndex = 0;
        currentNPC = null;
        isDialogueActive = false;
    }
    
    // Публичный метод для проверки, активен ли диалог
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
} 