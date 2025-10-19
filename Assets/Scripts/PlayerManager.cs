using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    AnimatorManager animatorManager;
    InputManager inputManager;
    CameraManager cameraManager;
    PlayerLocomotion playerLocomotion;

    [HideInInspector] public bool isInteracting;

    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        inputManager = GetComponent<InputManager>();
        cameraManager = FindAnyObjectByType<CameraManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void Update()
    {
        inputManager.HandleAllInputs();
    }

    private void FixedUpdate()
    {
        playerLocomotion.HandleAllMovement();
    }

    private void LateUpdate()
    {
        cameraManager.HandleAllCameraMovement();

        isInteracting = animatorManager.animator.GetBool("isInteracting");
        playerLocomotion.isJumping = animatorManager.animator.GetBool("isJumping");
        animatorManager.animator.SetBool("isGrounded", playerLocomotion.isGrounded);
    }
}
