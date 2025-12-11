using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetSelection : MonoBehaviour
{
    public static TargetSelection Instance;

    [Header("Target Settings")]
    public GameObject currentTarget;
    public LayerMask targetLayer;
    public float maxSelectionDistance = 50f;

    [Header("Selection Visual")]
    public Sprite indicatorSprite;
    public Color indicatorColor = Color.green;
    public Color aggressiveIndicatorColor = Color.red;
    public float indicatorSize = 2f;
    public float indicatorHeightOffset = 0.1f;

    [Header("Indicator Animation")]
    public bool rotateIndicator = true;
    public float rotationSpeed = 100f;
    public bool pulseEffect = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.15f;

    [SerializeField] GameObject loadingPanel;

    private GameObject currentIndicator;
    private Vector3 indicatorOriginalScale;
    private SpriteRenderer indicatorSpriteRenderer;

    // Agresif düþmanlar için indicator'lar
    private Dictionary<GameObject, GameObject> aggressiveIndicators = new Dictionary<GameObject, GameObject>();

    private PlayerControls playerControls;
    private GameObject player;

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

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player tag'li obje bulunamadý!");
        }

        loadingPanel.SetActive(true);   // Aç
        StartCoroutine(CloseLoadingPanel());  // 1 saniye sonra kapat
    }

    private IEnumerator CloseLoadingPanel()
    {
        yield return new WaitForSeconds(1f);  // 1 saniye bekle
        loadingPanel.SetActive(false);        // Kapat
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

    private void Update()
    {
        // Mevcut hedef yok olmuþsa veya ölmüþse
        if (currentTarget != null)
        {
            if (currentTarget == null || !currentTarget.activeInHierarchy)
            {
                SelectNextTarget();
            }
            else
            {
                HealthSystem targetHealth = currentTarget.GetComponent<HealthSystem>();
                if (targetHealth != null && targetHealth.currentHealth <= 0)
                {
                    SelectNextTarget();
                }
            }

            // Düþman agresif mi kontrol et ve indicator rengini güncelle
            UpdateIndicatorColor();
        }

        // Indicator animasyonlarý
        UpdateIndicatorAnimation();

        // Tüm düþmanlarý tara ve agresif olanlar için indicator oluþtur
        UpdateAggressiveIndicators();
    }

    private void UpdateAggressiveIndicators()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<GameObject> aggressiveEnemies = new List<GameObject>();

        // Agresif düþmanlarý tespit et
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;

            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();

            // Düþman agresif ve canlý mý?
            bool isAggressiveAndAlive = enemyAI != null &&
                                       enemyAI.isAggressive &&
                                       enemyHealth != null &&
                                       enemyHealth.IsAlive();

            // Seçili hedef deðilse ve agresifse
            if (isAggressiveAndAlive && enemy != currentTarget)
            {
                aggressiveEnemies.Add(enemy);

                // Bu düþman için indicator yoksa oluþtur
                if (!aggressiveIndicators.ContainsKey(enemy))
                {
                    CreateAggressiveIndicator(enemy);
                }
            }
        }

        // Artýk agresif olmayan veya ölen düþmanlarýn indicator'larýný temizle
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var pair in aggressiveIndicators)
        {
            if (!aggressiveEnemies.Contains(pair.Key) || pair.Key == null)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value);
                }
                toRemove.Add(pair.Key);
            }
        }

        foreach (GameObject enemy in toRemove)
        {
            aggressiveIndicators.Remove(enemy);
        }

        // Agresif indicator'larý animasyon uygula
        AnimateAggressiveIndicators();
    }

    private void CreateAggressiveIndicator(GameObject enemy)
    {
        GameObject indicator = new GameObject("AggressiveIndicator2D");
        indicator.transform.SetParent(enemy.transform);
        indicator.transform.localPosition = new Vector3(0, indicatorHeightOffset, 0);

        SpriteRenderer spriteRenderer = indicator.AddComponent<SpriteRenderer>();

        if (indicatorSprite != null)
        {
            spriteRenderer.sprite = indicatorSprite;
        }
        else
        {
            spriteRenderer.sprite = CreateDefaultCircleSprite();
        }

        spriteRenderer.color = Color.orange; // Kýrmýzý
        spriteRenderer.sortingOrder = 99; // Seçili indicator'ýn altýnda

        indicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        indicator.transform.localScale = Vector3.one * indicatorSize;

        aggressiveIndicators[enemy] = indicator;
    }

    private void AnimateAggressiveIndicators()
    {
        foreach (var pair in aggressiveIndicators)
        {
            GameObject indicator = pair.Value;
            if (indicator == null) continue;

            // Dönme efekti
            if (rotateIndicator)
            {
                indicator.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
            }

            // Pulse efekti
            if (pulseEffect)
            {
                float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                indicator.transform.localScale = Vector3.one * indicatorSize * scale;
            }
        }
    }

    private void UpdateIndicatorColor()
    {
        if (indicatorSpriteRenderer == null || currentTarget == null) return;

        EnemyAI enemyAI = currentTarget.GetComponent<EnemyAI>();
        if (enemyAI != null && enemyAI.isAggressive)
        {
            indicatorSpriteRenderer.color = aggressiveIndicatorColor;
        }
        else
        {
            indicatorSpriteRenderer.color = indicatorColor;
        }
    }

    private void UpdateIndicatorAnimation()
    {
        if (currentIndicator == null || indicatorSpriteRenderer == null) return;

        if (rotateIndicator)
        {
            currentIndicator.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
        }

        if (pulseEffect)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            currentIndicator.transform.localScale = indicatorOriginalScale * scale;
        }
    }

    private void OnNextTarget(InputAction.CallbackContext context)
    {
        RotatePlayerToTarget();
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
        Create2DSelectionIndicator();

        Debug.Log($"Hedef seçildi: {targetHealth.entityName}");
    }

    private void Create2DSelectionIndicator()
    {
        // Önceki göstergeyi kaldýr
        if (currentIndicator != null)
        {
            Destroy(currentIndicator);
        }

        if (currentTarget == null) return;

        // Eðer bu düþman agresif indicator'a sahipse, onu kaldýr
        if (aggressiveIndicators.ContainsKey(currentTarget))
        {
            Destroy(aggressiveIndicators[currentTarget]);
            aggressiveIndicators.Remove(currentTarget);
        }

        // Yeni 2D indicator oluþtur
        currentIndicator = new GameObject("SelectionIndicator2D");
        currentIndicator.transform.SetParent(currentTarget.transform);
        currentIndicator.transform.localPosition = new Vector3(0, indicatorHeightOffset, 0);

        indicatorSpriteRenderer = currentIndicator.AddComponent<SpriteRenderer>();

        if (indicatorSprite != null)
        {
            indicatorSpriteRenderer.sprite = indicatorSprite;
        }
        else
        {
            indicatorSpriteRenderer.sprite = CreateDefaultCircleSprite();
        }

        indicatorSpriteRenderer.color = indicatorColor;
        indicatorSpriteRenderer.sortingOrder = 100;

        currentIndicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        indicatorOriginalScale = Vector3.one * indicatorSize;
        currentIndicator.transform.localScale = indicatorOriginalScale;
    }

    private Sprite CreateDefaultCircleSprite()
    {
        int resolution = 128;
        Texture2D texture = new Texture2D(resolution, resolution);

        int center = resolution / 2;
        float radius = resolution / 2f;
        float innerRadius = radius * 0.7f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));

                if (distance < radius && distance > innerRadius)
                {
                    float alpha = 1f - Mathf.Abs(distance - ((radius + innerRadius) / 2f)) / ((radius - innerRadius) / 2f);
                    texture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(alpha)));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private void RotatePlayerToTarget()
    {
        if (currentTarget == null || player == null) return;

        Vector3 direction = (currentTarget.transform.position - player.transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
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

    private void SelectNextTarget()
    {
        if (player == null)
        {
            Debug.LogError("Player referansý bulunamadý!");
            return;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            Debug.Log("Etrafta düþman yok!");
            return;
        }

        var enemiesInRange = enemies.Where(enemy =>
            Vector3.Distance(player.transform.position, enemy.transform.position) <= maxSelectionDistance
        ).ToArray();

        if (enemiesInRange.Length == 0)
        {
            Debug.Log("Menzil içinde düþman yok!");
            return;
        }

        GameObject nearestEnemy = GetClosestEnemy(enemiesInRange);

        if (nearestEnemy == currentTarget)
        {
            Debug.Log("En yakýn düþman zaten seçili!");
            return;
        }

        if (nearestEnemy != null)
        {
            SelectTarget(nearestEnemy);
        }
    }

    private GameObject GetClosestEnemy(GameObject[] enemies)
    {
        GameObject closest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(player.transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }
}