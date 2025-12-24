using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public float deathDestroyDelay = 3f; // Ölüm animasyonu için bekleme süresi

    public bool isDead = false;

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