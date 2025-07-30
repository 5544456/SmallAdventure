using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildUIController : MonoBehaviour
{
    public GameObject menuPanel;
    public Button buildingNameButton;
    public TextMeshProUGUI buildingNameButtonText;
    public GameObject buildingResourcesPanel;
    public TextMeshProUGUI buildingResourcesText;
    public Transform previewParent;
    public Button buildButton;
    public Button closeButton;
    public Button upgradeButton;

    private BuildingSystem system;
    private BuildingData currentData;
    private GameObject previewModelInstance;
    private bool buildingSelected = false;

    public void ShowMenu(BuildingSystem sys)
    {
        system = sys;
        menuPanel.SetActive(true);
        buildButton.interactable = false;
        buildingResourcesPanel.SetActive(false);
        if (previewModelInstance != null) Destroy(previewModelInstance);
        buildingSelected = false;
        upgradeButton.gameObject.SetActive(false);
        if (system.currentBuildingInstance == null)
        {
            currentData = system.buildingLevel1;
        }
        else
        {
            var inst = system.currentBuildingInstance.GetComponent<BuildingInstance>();
            if (inst != null && inst.data != null && inst.data.upgradeTo != null)
            {
                currentData = inst.data.upgradeTo;
                upgradeButton.gameObject.SetActive(true);
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() => {
                    system.StartUpgradeMode(system.currentBuildingInstance);
                    Invoke("RefreshAfterUpgrade", 0.2f);
                });
            }
            else
            {
                buildingNameButtonText.text = "Максимальный уровень";
                buildingNameButton.interactable = false;
                return;
            }
        }
        buildingNameButtonText.text = currentData.buildingName;
        buildingNameButton.interactable = true;
        HidePreviewModel();
    }

    private void Awake()
    {
        buildingNameButton.onClick.AddListener(OnBuildingNameButton);
        buildButton.onClick.AddListener(OnBuildButton);
        closeButton.onClick.AddListener(OnCloseButton);
    }

    private void OnBuildingNameButton()
    {
        if (currentData == null) return;
        buildingResourcesPanel.SetActive(true);
        buildingResourcesText.text = $"Дерево: {currentData.woodCost}  Камень: {currentData.stoneCost}  Золото: {currentData.goldCost}";
        ShowPreviewModel(currentData);
        buildButton.interactable = true;
        buildingSelected = true;
        system.selectedBuilding = currentData;
    }

    private void OnBuildButton()
    {
        if (!buildingSelected) return;
        system.SelectBuilding(currentData);
        HideMenu();
    }

    private void OnCloseButton()
    {
        system.CloseBuildMenu();
        system.CancelPreview();
    }

    private void ShowPreviewModel(BuildingData data)
    {
        HidePreviewModel();
        if (data != null && data.prefab != null && previewParent != null)
        {
            previewModelInstance = Instantiate(data.prefab, previewParent);
            previewModelInstance.transform.localPosition = Vector3.zero;
            previewModelInstance.transform.localRotation = Quaternion.identity;
            previewModelInstance.transform.localScale = Vector3.one * 0.2f;
            foreach (var c in previewModelInstance.GetComponentsInChildren<Collider>())
                Destroy(c);
            foreach (var r in previewModelInstance.GetComponentsInChildren<Rigidbody>())
                Destroy(r);
            previewParent.gameObject.SetActive(true);
        }
    }

    private void HidePreviewModel()
    {
        if (previewModelInstance != null)
        {
            Destroy(previewModelInstance);
            previewModelInstance = null;
        }
        if (previewParent != null)
            previewParent.gameObject.SetActive(false);
    }

    public void HideMenu()
    {
        menuPanel.SetActive(false);
        HidePreviewModel();
        buildingResourcesPanel.SetActive(false);
        buildButton.interactable = false;
        buildingSelected = false;
    }

    private void RefreshAfterUpgrade()
    {
        if (system != null && system.currentBuildingInstance != null)
        {
            var inst = system.currentBuildingInstance.GetComponent<BuildingInstance>();
            if (inst != null && inst.data != null && inst.data.upgradeTo != null)
            {
                upgradeButton.gameObject.SetActive(true);
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() => {
                    system.StartUpgradeMode(system.currentBuildingInstance);
                    Invoke("RefreshAfterUpgrade", 0.2f);
                });
                buildingNameButtonText.text = inst.data.upgradeTo.buildingName;
                buildingNameButton.interactable = true;
            }
            else
            {
                upgradeButton.gameObject.SetActive(false);
                buildingNameButtonText.text = "Максимальный уровень";
                buildingNameButton.interactable = false;
            }
        }
    }
} 