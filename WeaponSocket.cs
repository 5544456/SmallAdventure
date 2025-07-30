using UnityEngine;

public class WeaponSocket : MonoBehaviour
{
    public GameObject weaponPrefab;
    private GameObject currentWeapon;
    private Animator animator;
    private Transform leftHandBone;

    [Header("Debug")]
    public bool showGizmos = true;
    public Vector3 weaponPositionOffset = Vector3.zero;
    public Vector3 weaponRotationOffset = Vector3.zero;

    void Start()
    {
        // Получаем аниматор
        animator = GetComponentInChildren<Animator>(); // Проверка на самом и дочерних объектах
        if (animator == null)
        {
            Debug.LogError("Animator не найден в " + gameObject.name + " или его дочерних объектах!");
            return;
        }

        // Находим кость правой руки
        leftHandBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        if (leftHandBone == null)
        {
            Debug.LogError("Кость правой руки не найдена!");
            return;
        }

        Debug.Log("Кость руки найдена: " + leftHandBone.name);

        // Если есть префаб оружия, создаем его
        if (weaponPrefab != null)
        {
            EquipWeapon(weaponPrefab);
        }
    }

    public void EquipWeapon(GameObject weaponToEquip)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        if (leftHandBone != null && weaponToEquip != null)
        {
            currentWeapon = Instantiate(weaponToEquip, leftHandBone);
            currentWeapon.transform.localPosition = weaponPositionOffset;
            currentWeapon.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
        }
    }

    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (leftHandBone != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(leftHandBone.position, 0.1f);
            Gizmos.DrawLine(leftHandBone.position, leftHandBone.position + leftHandBone.forward * 0.2f);
        }
    }
}