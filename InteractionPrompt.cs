using UnityEngine;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string defaultPromptText = "Нажмите E для взаимодействия";
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobHeight = 0.2f;
    
    private Transform mainCamera;
    private Vector3 initialPosition;
    private float bobTimer = 0f;
    
    private void Start()
    {
        mainCamera = Camera.main.transform;
        initialPosition = transform.localPosition;
        
        if (promptText != null)
        {
            promptText.text = defaultPromptText;
        }
    }
    
    private void Update()
    {
        // Поворачиваем подсказку к камере
        if (mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.position);
        }
        
        // Эффект плавного движения вверх-вниз
        bobTimer += Time.deltaTime;
        float bobOffset = Mathf.Sin(bobTimer * bobSpeed) * bobHeight;
        transform.localPosition = initialPosition + new Vector3(0, bobOffset, 0);
    }
    
    public void SetPromptText(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }
    }
    
    public void ResetPromptText()
    {
        if (promptText != null)
        {
            promptText.text = defaultPromptText;
        }
    }
} 