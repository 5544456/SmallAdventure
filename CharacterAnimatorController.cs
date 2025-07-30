using UnityEngine;

public class CharacterAnimatorController : MonoBehaviour
{
    private Animator animator;
    private int moveHash = Animator.StringToHash("isMoving");

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator не найден на " + gameObject.name);
        }
    }

    public void SetMoving(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool(moveHash, isMoving);
            Debug.Log($"Setting isMoving to: {isMoving}"); // Для отладки
        }
    }
}