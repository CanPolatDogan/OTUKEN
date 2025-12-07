using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    AnimatorManager animatorManager;
    PlayerManager playerManager;
    InputManager inputManager;

    Vector3 moveDirection;
    Transform cameraObject;
    Rigidbody playerRigidbody;

    private float inAirTimer = 0;
    private float leapingVelocity = 6;
    private float fallingVelocity = 66;
    private float rayCastHeightOffSet = 0.5f;
    [SerializeField] LayerMask groundLayer;

    [HideInInspector] public bool isSprinting, isGrounded, isJumping, isAttacking, isDefending;

    float sprintingSpeed = 10f;
    float walkingSpeed = 5f;
    float rotationSpeed = 15f;

    float jumpHeight = 15f;
    float gravityIntensity = -10f;

    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    public void HandleAllMovement()
    {
        HandleFallingAndLanding();

        if (playerManager.isInteracting)
            return;

        HandleMovement();
        HandleRotation();
        HandleDefending();
    }

    private void HandleMovement()
    {
        if (isJumping || isDefending || isAttacking)
            return;

        moveDirection = cameraObject.forward * inputManager.verticalInput;
        moveDirection += cameraObject.right * inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if (isSprinting)
        {
            moveDirection *= sprintingSpeed;
        }
        else
        {
            moveDirection *= walkingSpeed;
        }

        Vector3 movementVelocity = moveDirection;
        playerRigidbody.linearVelocity = movementVelocity;
    }

    private void HandleRotation()
    {
        if (isJumping || isDefending || isAttacking)
            return;

        // Input var mý kontrol et
        if (inputManager.verticalInput == 0 && inputManager.horizontalInput == 0)
            return;

        // Kamera yönüne göre hedef açýyý hesapla
        float targetAngle = 0f;
        float cameraYAngle = cameraObject.eulerAngles.y;

        // 8 yönlü sistem
        if (inputManager.verticalInput > 0 && inputManager.horizontalInput == 0)
        {
            // W - Ýleri
            targetAngle = cameraYAngle;
        }
        else if (inputManager.verticalInput > 0 && inputManager.horizontalInput > 0)
        {
            // W+D - Ýleri-Sað
            targetAngle = cameraYAngle + 45f;
        }
        else if (inputManager.verticalInput == 0 && inputManager.horizontalInput > 0)
        {
            // D - Sað
            targetAngle = cameraYAngle + 90f;
        }
        else if (inputManager.verticalInput < 0 && inputManager.horizontalInput > 0)
        {
            // S+D - Geri-Sað
            targetAngle = cameraYAngle + 135f;
        }
        else if (inputManager.verticalInput < 0 && inputManager.horizontalInput == 0)
        {
            // S - Geri
            targetAngle = cameraYAngle + 180f;
        }
        else if (inputManager.verticalInput < 0 && inputManager.horizontalInput < 0)
        {
            // S+A - Geri-Sol
            targetAngle = cameraYAngle - 135f;
        }
        else if (inputManager.verticalInput == 0 && inputManager.horizontalInput < 0)
        {
            // A - Sol
            targetAngle = cameraYAngle - 90f;
        }
        else if (inputManager.verticalInput > 0 && inputManager.horizontalInput < 0)
        {
            // W+A - Ýleri-Sol
            targetAngle = cameraYAngle - 45f;
        }

        // Hedef rotasyonu oluþtur
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

        // Yumuþak geçiþ (istersen daha hýzlý yapabilirsin)
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 rayCastOrigin = transform.position;
        Vector3 targetPosition = transform.position;
        rayCastOrigin.y += rayCastHeightOffSet;

        if (!isGrounded && !isJumping)
        {
            if (!playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Falling", true);
            }

            inAirTimer += Time.deltaTime;
            playerRigidbody.AddForce(transform.forward * leapingVelocity);
            playerRigidbody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
        }

        if (Physics.SphereCast(rayCastOrigin, 0.2f, -Vector3.up, out hit, groundLayer))
        {
            if (!isGrounded && !playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Land", true);
            }

            Vector3 rayCastHitPoint = hit.point;
            targetPosition.y = rayCastHitPoint.y;
            inAirTimer = 0;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if (isGrounded && !isJumping)
        {
            if (playerManager.isInteracting || inputManager.moveAmount > 0)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime / 0.1f);
            }
            else
            {
                transform.position = targetPosition;
            }
        }
    }

    public void HandleJumping()
    {
        if (isGrounded && !isDefending)
        {
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.PlayTargetAnimation("Jump", false);

            float jumpVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = moveDirection;
            playerVelocity.y = jumpVelocity;
            playerRigidbody.linearVelocity = playerVelocity;
        }
    }

    public void HandleAttacking()
    {
        if (isJumping || isDefending)
            return;

        if (isGrounded && !isAttacking)
        {
            animatorManager.animator.SetBool("isAttacking", true);
            animatorManager.PlayTargetAnimation("Attack", false);
        }
    }

    private void HandleDefending()
    {
        isDefending = inputManager.defendInput;
        animatorManager.animator.SetBool("isDefending", isDefending);
    }
}
