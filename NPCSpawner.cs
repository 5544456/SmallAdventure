using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("Префабы NPC")]
    [SerializeField] private GameObject friendlyNPCPrefab;
    [SerializeField] private GameObject patrollingNPCPrefab;
    
    [Header("Настройки стационарного NPC")]
    [SerializeField] private Transform stationaryNPCSpawnPoint;
    [SerializeField] private DialogueData stationaryNPCDialogue;
    
    [Header("Настройки патрулирующего NPC")]
    [SerializeField] private Transform patrollingNPCSpawnPoint;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private DialogueData patrollingNPCDialogue;
    
    [Header("Настройки позиционирования")]
    [SerializeField] private float yOffset = -0.5f; // Смещение по Y для корректировки высоты
    
    private void Start()
    {
        // Создаем стационарного NPC
        if (friendlyNPCPrefab != null && stationaryNPCSpawnPoint != null)
        {
            // Корректируем позицию спавна с учетом смещения по Y
            Vector3 spawnPos = new Vector3(
                stationaryNPCSpawnPoint.position.x,
                stationaryNPCSpawnPoint.position.y + yOffset,
                stationaryNPCSpawnPoint.position.z
            );
            
            GameObject stationaryNPC = Instantiate(
                friendlyNPCPrefab, 
                spawnPos, 
                stationaryNPCSpawnPoint.rotation
            );
            
            // Настраиваем стационарного NPC
            FriendlyNPC npcScript = stationaryNPC.GetComponent<FriendlyNPC>();
            if (npcScript != null && stationaryNPCDialogue != null)
            {
                // Настраиваем диалог через SerializeField
                // Другие настройки можно установить через инспектор
            }
        }
        
        // Создаем патрулирующего NPC
        if (patrollingNPCPrefab != null && patrollingNPCSpawnPoint != null)
        {
            // Корректируем позицию спавна с учетом смещения по Y
            Vector3 spawnPos = new Vector3(
                patrollingNPCSpawnPoint.position.x,
                patrollingNPCSpawnPoint.position.y + yOffset,
                patrollingNPCSpawnPoint.position.z
            );
            
            GameObject patrollingNPC = Instantiate(
                patrollingNPCPrefab, 
                spawnPos, 
                patrollingNPCSpawnPoint.rotation
            );
            
            // Настраиваем патрулирующего NPC
            FriendlyNPC npcScript = patrollingNPC.GetComponent<FriendlyNPC>();
            if (npcScript != null && patrollingNPCDialogue != null)
            {
                // Настраиваем диалог через SerializeField
                // Другие настройки можно установить через инспектор
            }
        }
    }
} 