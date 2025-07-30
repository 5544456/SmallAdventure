// ВРЕМЕННО ОТКЛЮЧЕНО
/*
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PerkUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject perkPanel;
    [SerializeField] private Transform perkContainer;
    [SerializeField] private GameObject perkButtonPrefab;
    [SerializeField] private TextMeshProUGUI perkPointsText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Settings")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color unavailableColor = Color.gray;
    [SerializeField] private Color maxLevelColor = Color.green;

    private PerkSystem perkSystem;
    private LevelSystem levelSystem;
    private Dictionary<string, Perk> perksByName = new Dictionary<string, Perk>();

    private void Start()
    {
        perkSystem = UnityEngine.Object.FindFirstObjectByType<PerkSystem>();
        levelSystem = UnityEngine.Object.FindFirstObjectByType<LevelSystem>();

        if (perkSystem == null)
        {
            Debug.LogError("PerkSystem not found!");
            return;
        }

        if (levelSystem == null)
        {
            Debug.LogError("LevelSystem not found!");
            return;
        }

        // Инициализируем словарь перков
        foreach (var perk in perkSystem.AvailablePerks)
        {
            perksByName[perk.PerkName] = perk;
        }

        PerkSystem.OnPerkPointsChanged += UpdatePerkPoints;
        PerkSystem.OnPerkUnlocked += OnPerkUnlocked;
        
        CreatePerkButtons();
        UpdatePerkPoints(perkSystem.PerkPoints);
        perkPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePerkPanel();
        }
    }

    private void CreatePerkButtons()
    {
        foreach (Transform child in perkContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Perk perk in perkSystem.AvailablePerks)
        {
            GameObject buttonObj = Instantiate(perkButtonPrefab, perkContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (button != null && buttonText != null)
            {
                buttonText.text = perk.PerkName;
                button.onClick.AddListener(() => OnPerkButtonClicked(perk));

                // Настраиваем начальное состояние кнопки
                UpdateButtonState(button, buttonText, perk);
            }
        }
    }

    private void OnPerkUnlocked(Perk unlockedPerk)
    {
        UpdatePerkButtons();
        UpdatePerkDescription(unlockedPerk);
    }

    private void UpdatePerkButtons()
    {
        foreach (Transform child in perkContainer)
        {
            Button button = child.GetComponent<Button>();
            TextMeshProUGUI buttonText = child.GetComponentInChildren<TextMeshProUGUI>();
            string perkName = buttonText.text;
            
            if (perksByName.TryGetValue(perkName, out Perk perk))
            {
                if (button != null && buttonText != null)
                {
                    UpdateButtonState(button, buttonText, perk);
                }
            }
        }
    }

    private void UpdateButtonState(Button button, TextMeshProUGUI buttonText, Perk perk)
    {
        if (perk.CurrentLevel >= perk.MaxLevel)
        {
            button.interactable = false;
            buttonText.color = maxLevelColor;
        }
        else if (!perk.IsUnlocked && perkSystem.PerkPoints > 0)
        {
            button.interactable = true;
            buttonText.color = availableColor;
        }
        else
        {
            button.interactable = false;
            buttonText.color = unavailableColor;
        }
    }

    private void OnPerkButtonClicked(Perk perk)
    {
        if (perkSystem.UnlockPerk(perk.PerkName))
        {
            UpdatePerkButtons();
            UpdatePerkDescription(perk);
        }
    }

    private void UpdatePerkPoints(int points)
    {
        perkPointsText.text = $"Очки навыков: {points}";
    }

    private void UpdatePerkDescription(Perk perk)
    {
        if (perk != null)
        {
            string description = $"{perk.PerkName} (Уровень {perk.CurrentLevel}/{perk.MaxLevel})\n\n";
            description += perk.Description;
            
            if (perk.CurrentLevel < perk.MaxLevel)
            {
                description += $"\n\nСледующий уровень:\n{perk.GetNextLevelDescription()}";
            }
            
            descriptionText.text = description;
        }
        else
        {
            descriptionText.text = "Выберите навык для просмотра описания";
        }
    }

    public void TogglePerkPanel()
    {
        perkPanel.SetActive(!perkPanel.activeSelf);
        
        if (perkPanel.activeSelf)
        {
            UpdatePerkButtons();
            UpdatePerkDescription(null);
        }
    }

    private void OnDestroy()
    {
        if (perkSystem != null)
        {
            PerkSystem.OnPerkPointsChanged -= UpdatePerkPoints;
            PerkSystem.OnPerkUnlocked -= OnPerkUnlocked;
        }
    }
}
*/ 