using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI References")]
    public Slider healthBar;
    public TMP_Text healthText;
    public TMP_Text nameText;
    public GameObject healthBarUI;

    [Header("Entity Info")]
    public string entityName;
    public bool isPlayer = false;

    [Header("Death Settings")]
    public float deathDestroyDelay = 3f; // Ölüm animasyonu için bekleme süresiü
    public bool isDead = false;

    [Header("Visual Feedback")]
    private Renderer[] enemyRenderers;
    private List<Material[]> originalMaterials = new List<Material[]>();
    private Coroutine flashCoroutine;

    [Header("Flash Settings")]
    public float flashIntensity = 0.4f; // Kýrmýzýlýk oraný (0-1 arasý, 0.4 = %40 kýrmýzý)
    public float flashDuration = 0.15f; // Flash süresi

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (isPlayer)
        {
            if (PlayerPrefs.GetString("PlayerName") == string.Empty)
            {
                nameText.text = "Oyuncu";
            }
            else
            {
                nameText.text = PlayerPrefs.GetString("PlayerName");
            }
        }

        if (!isPlayer && healthBarUI != null)
        {
            healthBarUI.SetActive(false);
        }

        // Renderer'larý kaydet (düþmanlar için)
        if (!isPlayer)
        {
            CacheRenderers();
        }
    }

    private void CacheRenderers()
    {
        enemyRenderers = GetComponentsInChildren<Renderer>(true);

        originalMaterials.Clear();

        foreach (Renderer renderer in enemyRenderers)
        {
            if (renderer != null && renderer.materials != null)
            {
                Material[] materials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    materials[i] = new Material(renderer.materials[i]);
                }
                originalMaterials.Add(materials);
            }
        }

        Debug.Log($"{gameObject.name}: {enemyRenderers.Length} renderer bulundu");
    }

    public void FlashRed()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRedCoroutine());
    }

    private System.Collections.IEnumerator FlashRedCoroutine()
    {
        List<Material> allMaterials = new List<Material>();
        List<Color> originalColors = new List<Color>();

        // Tüm materyalleri topla ve orijinal renklerini sakla
        foreach (Renderer renderer in enemyRenderers)
        {
            if (renderer == null) continue;

            foreach (Material mat in renderer.materials)
            {
                if (mat == null) continue;

                allMaterials.Add(mat);

                // Orijinal rengi sakla
                Color originalColor = Color.white;
                if (mat.HasProperty("_Color"))
                {
                    originalColor = mat.GetColor("_Color");
                }
                else if (mat.HasProperty("_BaseColor"))
                {
                    originalColor = mat.GetColor("_BaseColor");
                }
                originalColors.Add(originalColor);
            }
        }

        // Hafif kýrmýzý tint uygula
        for (int i = 0; i < allMaterials.Count; i++)
        {
            Material mat = allMaterials[i];
            Color originalColor = originalColors[i];

            // Orijinal renk ile kýrmýzý arasýnda karýþým (Lerp)
            Color tintedColor = Color.Lerp(originalColor, Color.red, flashIntensity);

            // Rengi uygula
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", tintedColor);
            }
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", tintedColor);
            }
        }

        // Bekle
        yield return new WaitForSeconds(flashDuration);

        // Orijinal renklere dön
        for (int i = 0; i < allMaterials.Count; i++)
        {
            Material mat = allMaterials[i];
            Color originalColor = originalColors[i];

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", originalColor);
            }
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", originalColor);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // Ölüyse hasar alma

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }

        if (currentHealth <= 0)
        {
            ShowHealthBar(false);
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)} / {maxHealth}";
        }
    }

    public void ShowHealthBar(bool show)
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(show);

            if (show)
            {
                nameText.text = entityName;
                // Bar açýlýrken can deðerlerini de güncelle!
                UpdateHealthUI();
            }
        }
    }

    private void Die()
    {
        if (isDead) return; // Birden fazla çaðrýlmasýný önle
        isDead = true;

        Debug.Log($"{entityName} öldü!");

        if (!isPlayer)
        {
            // EnemyAI'a ölüm animasyonunu tetikle
            EnemyAI enemyAI = GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.OnDeath();
            }

            // Collider'ý kapat (düþman üzerinden geçilebilsin)
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            // Rigidbody varsa kinematic yap
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            // Health bar'ý gizle
            ShowHealthBar(false);

            // Animasyon oynadýktan sonra objeyi yok et
            Destroy(gameObject, deathDestroyDelay);
        }
        else
        {
            // Oyuncu öldüðünde yapýlacaklar
            Debug.Log("Game Over!");

            // Oyuncu kontrollerini devre dýþý býrak
            InputManager inputManager = GetComponent<InputManager>();
            if (inputManager != null)
            {
                inputManager.enabled = false;
            }

            // Oyuncu animatörü varsa ölüm animasyonu
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("isDead", true);
            }

            // Tüm düþmanlara oyuncunun öldüðünü bildir
            NotifyEnemiesOfPlayerDeath();

            // Collider'ý kapat (düþmanlar üzerinden geçebilsin)
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }

    private void NotifyEnemiesOfPlayerDeath()
    {
        // Sahnedeki tüm düþmanlarý bul
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();

        foreach (EnemyAI enemy in allEnemies)
        {
            enemy.OnPlayerDeath();
        }

        Debug.Log($"{allEnemies.Length} düþmana oyuncunun öldüðü bildirildi.");
    }

    public bool IsAlive()
    {
        return !isDead && currentHealth > 0;
    }
}