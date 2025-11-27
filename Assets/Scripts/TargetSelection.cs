using UnityEngine;
using UnityEngine.InputSystem;

public class TargetSelection : MonoBehaviour
{
    public static TargetSelection Instance;

    [Header("Target Settings")]
    public GameObject currentTarget;
    public LayerMask targetLayer; // Enemy layer'ýný seçin
    public float maxSelectionDistance = 50f; // Maksimum seçim mesafesi

    [Header("Selection Visual")]
    public GameObject selectionIndicator; // Seçilen hedefin altýna konulacak gösterge
    private GameObject currentIndicator;

    private PlayerControls playerControls;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        playerControls.PlayerActions.NextTarget.performed += OnNextTarget;
    }

    private void OnDisable()
    {
        playerControls.PlayerActions.NextTarget.performed -= OnNextTarget;
        playerControls.Disable();
    }

    private void OnNextTarget(InputAction.CallbackContext context)
    {
        SelectNextTarget();
    }

    public void SelectTarget(GameObject target)
    {
        // Önceki hedefi temizle
        if (currentTarget != null)
        {
            HealthSystem previousHealth = currentTarget.GetComponent<HealthSystem>();
            if (previousHealth != null)
            {
                previousHealth.ShowHealthBar(false);
            }
        }

        // Yeni hedefi seç
        currentTarget = target;

        // Hedefin can barýný göster
        HealthSystem targetHealth = currentTarget.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            targetHealth.ShowHealthBar(true);
        }

        // Seçim göstergesini oluþtur
        ShowSelectionIndicator();

        // Oyuncuyu hedefe döndür
        RotatePlayerToTarget();

        Debug.Log($"Hedef seçildi: {targetHealth.entityName}");
    }

    private void RotatePlayerToTarget()
    {
        if (currentTarget == null) return;

        // Oyuncu objesini bul (bu script GameManager'da olduðu için Player'ý bulmalýyýz)
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("Player tag'li obje bulunamadý!");
            return;
        }

        // Hedefe doðru yönü hesapla
        Vector3 direction = (currentTarget.transform.position - player.transform.position).normalized;
        direction.y = 0; // Sadece yatay düzlemde dön

        if (direction != Vector3.zero)
        {
            // Oyuncuyu hedefe döndür
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            player.transform.rotation = targetRotation;
        }
    }

    public void DeselectTarget()
    {
        if (currentTarget != null)
        {
            HealthSystem targetHealth = currentTarget.GetComponent<HealthSystem>();
            if (targetHealth != null)
            {
                targetHealth.ShowHealthBar(false);
            }
        }

        currentTarget = null;

        if (currentIndicator != null)
        {
            Destroy(currentIndicator);
        }

        Debug.Log("Hedef seçimi kaldýrýldý");
    }

    private void ShowSelectionIndicator()
    {
        // Önceki göstergeyi kaldýr
        if (currentIndicator != null)
        {
            Destroy(currentIndicator);
        }

        // Yeni gösterge oluþtur
        if (selectionIndicator != null && currentTarget != null)
        {
            currentIndicator = Instantiate(selectionIndicator, currentTarget.transform);
            currentIndicator.transform.localPosition = Vector3.zero;
        }
    }

    private void SelectNextTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            Debug.Log("Etrafta düþman yok!");
            return;
        }

        // Eðer hedef yoksa ilk düþmaný seç
        if (currentTarget == null)
        {
            SelectTarget(enemies[0]);
            return;
        }

        // Mevcut hedefin index'ini bul ve sonrakini seç
        int currentIndex = System.Array.IndexOf(enemies, currentTarget);
        int nextIndex = (currentIndex + 1) % enemies.Length;

        SelectTarget(enemies[nextIndex]);
    }

    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }
}