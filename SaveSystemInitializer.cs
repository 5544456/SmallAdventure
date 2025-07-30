using UnityEngine;
using UnityEngine.UI;

public class SaveSystemInitializer : MonoBehaviour
{
    [Header("Ссылки на кнопки")]
    public Button continueButton;
    public Button saveButton;
    public Button loadButton;
    public Button mainMenuButton;
    public Button backButton;

    private SaveSystem saveSystem;

    private void Start()
    {
        // Находим SaveSystem
        saveSystem = FindFirstObjectByType<SaveSystem>();
        
        if (saveSystem == null)
        {
            Debug.LogError("SaveSystem не найден на сцене! Добавьте объект с компонентом SaveSystem.");
            return;
        }
        
        // Настраиваем кнопки
        SetupButtons();
    }

    private void SetupButtons()
    {
        // Кнопка "Продолжить"
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(saveSystem.ReturnToGame);
        }
        
        // Кнопка "Сохранить"
        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(saveSystem.ShowSaveMenu);
        }
        
        // Кнопка "Загрузить"
        if (loadButton != null)
        {
            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(saveSystem.ShowLoadMenu);
        }
        
        // Кнопка "Главное меню"
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(saveSystem.GoToMainMenu);
        }
        
        // Кнопка "Назад" в меню сохранения/загрузки
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(saveSystem.ReturnToPauseMenu);
        }
    }
} 