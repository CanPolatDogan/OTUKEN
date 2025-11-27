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
    public GameObject healthBarUI;

    [Header("Entity Info")]
    public string entityName;
    public bool isPlayer = false;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        // Canavar için UI'ý baþta gizle
        if (!isPlayer && healthBarUI != null)
        {
            healthBarUI.SetActive(false);
        }
    }

    public void TakeDamage(float damage)
    {
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
        }
    }

    private void Die()
    {
        Debug.Log($"{entityName} öldü!");

        if (!isPlayer)
        {
            // Canavar öldüðünde yapýlacaklar
            Destroy(gameObject, 2f);
        }
        else
        {
            // Oyuncu öldüðünde yapýlacaklar
            Debug.Log("Game Over!");
        }
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}