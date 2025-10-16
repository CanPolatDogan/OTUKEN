using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    Animator anim;
    int horizontal, vertical;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement, bool isSprinting)
    {
        float snappedHorizontal, snappedVertical;

        if (horizontalMovement > 0 && horizontalMovement < 0.55f)
            snappedHorizontal = 0.5f;
        else if (horizontalMovement >= 0.55f)
            snappedHorizontal = 1f;
        else if (horizontalMovement < 0 && horizontalMovement > -0.55f)
            snappedHorizontal = -0.5f;
        else if (horizontalMovement <= -0.55f)
            snappedHorizontal = -1f;
        else
            snappedHorizontal = 0;

        if (verticalMovement > 0 && verticalMovement < 0.55f)
            snappedVertical = 0.5f;
        else if (verticalMovement >= 0.55f)
            snappedVertical = 1f;
        else if (verticalMovement < 0 && verticalMovement > -0.55f)
            snappedVertical = -0.5f;
        else if (verticalMovement <= -0.55f)
            snappedVertical = -1f;
        else
            snappedVertical = 0;

        if (isSprinting)
        {
            snappedHorizontal = horizontalMovement;
            snappedVertical = 2;
        }

        anim.SetFloat(horizontal, snappedHorizontal, 0.1f, Time.deltaTime);
        anim.SetFloat(vertical, snappedVertical, 0.1f, Time.deltaTime);
    }
}
