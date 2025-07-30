using UnityEngine;

public class ShieldSocket : MonoBehaviour
{
    public GameObject shieldPrefab;
    private GameObject currentShield;
    private Animator animator;
    private Transform rightHandBone;

    [Header("Debug")]
    public bool showGizmos = true;
    public Vector3 shieldPositionOffset = Vector3.zero;
    public Vector3 shieldRotationOffset = Vector3.zero;

    void Start()
    {
        // Получаем аниматор
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator не найден в " + gameObject.name + " или его дочерних объектах!");
            return;
        }

        // Находим кость правой руки
        rightHandBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
        if (rightHandBone == null)
        {
            Debug.LogError("Кость правой руки не найдена!");
            return;
        }

        Debug.Log("Кость правой руки найдена: " + rightHandBone.name);

        // Если есть префаб щита, создаем его
        if (shieldPrefab != null)
        {
            EquipShield(shieldPrefab);
        }
    }

    public void EquipShield(GameObject shieldToEquip)
    {
        if (currentShield != null)
        {
            Destroy(currentShield);
        }

        if (rightHandBone != null && shieldToEquip != null)
        {
            currentShield = Instantiate(shieldToEquip, rightHandBone);
            currentShield.transform.localPosition = shieldPositionOffset;
            currentShield.transform.localRotation = Quaternion.Euler(shieldRotationOffset);
        }
    }

    public void UnequipShield()
    {
        if (currentShield != null)
        {
            Destroy(currentShield);
            currentShield = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (rightHandBone != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rightHandBone.position, 0.1f);
            Gizmos.DrawLine(rightHandBone.position, rightHandBone.position + rightHandBone.forward * 0.2f);
        }
    }
} 