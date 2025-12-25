using UnityEngine;
using UnityEngine.InputSystem;

public class ClickableObject : MonoBehaviour
{
    [Header("Click Settings")]
    [Tooltip("Fare ile mi yoksa sadece UI ile mi týklanabilir?")]
    public bool useRaycast = true;
    
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
        // Fareyi üzerine getirdiðinde renk deðiþimi
        if (objectRenderer != null)
        {
            objectRenderer.material.color = highlightColor;
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
        playerHealth.Heal(100f);
    }

    // Event sistemi kullanmak isterseniz
    public delegate void ClickEvent();
    public event ClickEvent OnClickEvent;

    // Inspector'dan çaðrýlabilir versiyon
    public void OnClick()
    {
        OnObjectClicked();
    }
}