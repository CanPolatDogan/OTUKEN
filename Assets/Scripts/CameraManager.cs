using UnityEngine;

public class CameraManager : MonoBehaviour
{
    InputManager inputManager;
    Transform targetTransform; // The target the camera will follow
    Transform cameraTransform; // the transform of the camera object

    [SerializeField] Transform cameraPivot; // the object the camera pivots around
    [SerializeField] LayerMask collisionLayers; // Layers to consider for camera collision

    private float defaultPosition;
    private Vector3 cameraFollowVelocity = Vector3.zero;
    private Vector3 cameraVectorPosition;

    float cameraCollisionRadius = 0.2f; // Radius of the sphere used for collision detection
    float cameraCollisionOffset = 0.2f;
    float minimumCollisionOffset = 0.2f;
    float cameraFollowSpeed = 0.2f; // Speed at which the camera follows the target
    float cameraLookSpeed = 0.3f;
    float cameraPivotSpeed = 0.3f;

    float lookAngle, pivotAngle;

    float minimumPivot = -35;
    float maximumPivot = 35;

    private void Awake()
    {
        targetTransform = FindAnyObjectByType<PlayerManager>().transform;
        inputManager = FindAnyObjectByType<InputManager>();
        cameraTransform = Camera.main.transform;
        defaultPosition = cameraTransform.localPosition.z;
    }

    public void HandleAllCameraMovement()
    {
        FollowTarget();
        RotateCamera();
        HandleCameraCollisions();
    }

    private void FollowTarget()
    {
        Vector3 targetPosition = Vector3.SmoothDamp(transform.position, targetTransform.position, ref cameraFollowVelocity, cameraFollowSpeed);
        transform.position = targetPosition;
    }

    private void RotateCamera()
    {
        Vector3 rotation;
        Quaternion targetRotation;

        lookAngle = lookAngle + (inputManager.cameraInputX * cameraLookSpeed);
        pivotAngle = pivotAngle - (inputManager.cameraInputY * cameraPivotSpeed);
        pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);

        rotation = Vector3.zero;
        rotation.y = lookAngle;
        targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = pivotAngle;
        targetRotation = Quaternion.Euler(rotation);
        cameraPivot.localRotation = targetRotation;
    }

    private void HandleCameraCollisions()
    {
        float targetPosition = defaultPosition;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivot.position;
        direction.Normalize();

        if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetPosition), collisionLayers))
        {
            float distance = Vector3.Distance(cameraPivot.position, hit.point);
            targetPosition =- (distance - cameraCollisionOffset);
        }

        if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
            targetPosition =- minimumCollisionOffset;

        cameraVectorPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, 0.2f);
        cameraTransform.localPosition = cameraVectorPosition;
    }
}
