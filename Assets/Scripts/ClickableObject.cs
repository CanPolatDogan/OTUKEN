using UnityEngine;
using UnityEngine.InputSystem;

public class ClickableObject : MonoBehaviour
{
    [Header("Click Settings")]
    [Tooltip("Fare ile mi yoksa sadece UI ile mi týklanabilir?")]
    public bool useRaycast = true;

    [Header("Distance Settings")]
    public float maxInteractionDistance = 10f; // Maksimum etkileþim mesafesi

    [Header("Visual Feedback")]
    public Color highlightColor = Color.yellow;
    public Color normalColor = Color.white;
    private Renderer objectRenderer;
    private Color originalColor;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private Mouse mouse;
    private bool isMouseOver = false;

    [SerializeField] HealthSystem playerHealth;

    private void Start()
    {
        // Mouse referansýný al
        mouse = Mouse.current;

        // Objenin renderer'ýný al
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
            normalColor = originalColor;
        }

        // 3D obje için collider kontrolü
        if (useRaycast && GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"{gameObject.name} üzerinde Collider yok! Týklama çalýþmayabilir.");
        }

        // PlayerHealth kontrolü
        if (playerHealth == null)
        {
            Debug.LogWarning($"{gameObject.name} üzerinde PlayerHealth atanmamýþ!");
        }
    }

    private void Update()
    {
        if (!useRaycast || mouse == null) return;

        // Fare ile obje üzerine gelince
        if (IsMouseOver())
        {
            if (!isMouseOver)
            {
                OnMouseEnterObject();
                isMouseOver = true;
            }

            // Sol týk kontrolü (Yeni Input System)
            if (mouse.leftButton.wasPressedThisFrame)
            {
                OnObjectClicked();
            }
        }
        else
        {
            if (isMouseOver)
            {
                OnMouseExitObject();
                isMouseOver = false;
            }
        }
    }

    private bool IsMouseOver()
    {
        // Yeni Input System ile mouse pozisyonu
        Vector2 mousePosition = mouse.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject == gameObject;
        }

        return false;
    }

    private void OnMouseEnterObject()
    {
        // Mesafe kontrolü yap
        if (playerHealth != null)
        {
            float distance = Vector3.Distance(transform.position, playerHealth.transform.position);

            // Eðer mesafe uygunsa yeþil, deðilse kýrmýzý yap
            if (distance <= maxInteractionDistance)
            {
                if (objectRenderer != null)
                {
                    objectRenderer.material.color = highlightColor;
                }
            }
            else
            {
                if (objectRenderer != null)
                {
                    objectRenderer.material.color = Color.red; // Uzak olduðunu göster
                }
            }
        }
        else
        {
            // PlayerHealth yoksa normal highlight
            if (objectRenderer != null)
            {
                objectRenderer.material.color = highlightColor;
            }
        }

        // Ýsteðe baðlý: Cursor deðiþtir
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseExitObject()
    {
        // Fareyi çektiðinde normal renge dön
        if (objectRenderer != null)
        {
            objectRenderer.material.color = normalColor;
        }
    }

    // ======== ANA FONKSÝYON - BURAYA ÝSTEDÝÐÝNÝZÝ YAZIN ========
    public void OnObjectClicked()
    {
        if (playerHealth == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("PlayerHealth atanmamýþ!");
            return;
        }

        // Mesafe kontrolü
        float distance = Vector3.Distance(transform.position, playerHealth.transform.position);

        if (distance <= maxInteractionDistance)
        {
            // Mesafe uygun, heal iþlemini yap
            playerHealth.Heal(100f);

            if (showDebugLogs)
                Debug.Log($"Oyuncu þifa aldý! Mesafe: {distance:F2}m");
        }
        else
        {
            // Mesafe çok uzak
            if (showDebugLogs)
                Debug.Log($"Çok uzaksýn! Mesafe: {distance:F2}m (Max: {maxInteractionDistance}m)");
        }
    }

    // Inspector'dan çaðrýlabilir versiyon
    public void OnClick()
    {
        OnObjectClicked();
    }

    // Debug için Gizmo çiz
    private void OnDrawGizmosSelected()
    {
        // Etkileþim mesafesini göster
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxInteractionDistance);
    }
}