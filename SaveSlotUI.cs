using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public TextMeshProUGUI saveNameText;
    public TextMeshProUGUI saveDateText;
    public Button actionButton;
    
    private string saveName;
    
    public void Initialize(GameSaveData data, bool isSaveMode)
    {
        if (data != null)
        {
            saveName = data.saveName;
            saveNameText.text = data.saveName;
            saveDateText.text = data.saveTime.ToString("dd.MM.yyyy HH:mm");
            
            TextMeshProUGUI buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isSaveMode ? "Сохранить" : "Загрузить";
            }
        }
        gameObject.SetActive(true);
        
        Debug.Log($"Слот инициализирован: {data?.saveName}, режим сохранения: {isSaveMode}");
    }
    
    public void InitializeAsNew(string newSaveName)
    {
        saveName = newSaveName;
        saveNameText.text = "Новое сохранение";
        
        // Используем актуальную дату и время
        DateTime currentTime = DateTime.Now;
        saveDateText.text = currentTime.ToString("dd.MM.yyyy HH:mm");
        
        TextMeshProUGUI buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = "Создать";
        }
        
        gameObject.SetActive(true);
        
        Debug.Log($"Слот инициализирован как новое сохранение: {newSaveName}, дата: {currentTime}");
    }
    
    public void InitializeQuickSave()
    {
        saveName = "quicksave";
        saveNameText.text = "Быстрое сохранение";
        
        // Используем актуальную дату и время
        DateTime currentTime = DateTime.Now;
        saveDateText.text = currentTime.ToString("dd.MM.yyyy HH:mm");
        
        TextMeshProUGUI buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = "Сохранить";
        }
        
        gameObject.SetActive(true);
        
        Debug.Log($"Слот инициализирован как быстрое сохранение, дата: {currentTime}");
    }
    
    public void SetButtonAction(UnityAction action)
    {
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(action);
    }
    
    // Вызывается при нажатии на кнопку (для привязки в редакторе)
    public void OnButtonClick()
    {
        Debug.Log($"Нажата кнопка в слоте: {saveName}");
    }
} 