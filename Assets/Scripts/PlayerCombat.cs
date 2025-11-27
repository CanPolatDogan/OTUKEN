using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackDamage = 20f;
    public float attackRange = 3f;
    public float attackCooldown = 1f;
    private float lastAttackTime = 0f;

    [Header("References")]
    private HealthSystem playerHealth;
    private PlayerControls playerControls;

    PlayerLocomotion playerLoco;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        playerControls.PlayerActions.Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        playerControls.PlayerActions.Attack.performed -= OnAttack;
        playerControls.Disable();
    }

    private void Start()
    {
        playerHealth = GetComponent<HealthSystem>();
        playerLoco = GetComponent<PlayerLocomotion>();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        TryAttack();
    }

    private void TryAttack()
    {
        if (playerLoco.isAttacking)
        {
            return;
        }

        // Cooldown kontrolü
        if (Time.time - lastAttackTime < attackCooldown)
        {
            Debug.Log("Çok hýzlý saldýrýyorsunuz!");
            return;
        }

        // Hedef seçili mi kontrol et
        GameObject target = TargetSelection.Instance.GetCurrentTarget();

        if (target == null)
        {
            Debug.Log("Hedef seçilmedi!");
            return;
        }

        // Hedef hala hayatta mý
        HealthSystem targetHealth = target.GetComponent<HealthSystem>();
        if (targetHealth == null || !targetHealth.IsAlive())
        {
            Debug.Log("Hedef geçersiz!");
            TargetSelection.Instance.DeselectTarget();
            return;
        }

        // Menzil kontrolü
        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > attackRange)
        {
            Debug.Log($"Hedef çok uzakta! Mesafe: {distance:F1}m, Menzil: {attackRange}m");
            return;
        }

        // Saldýrý gerçekleþtir
        PerformAttack(target, targetHealth);
    }

    private void PerformAttack(GameObject target, HealthSystem targetHealth)
    {
        // Hedefe hasar ver
        targetHealth.TakeDamage(attackDamage);
        lastAttackTime = Time.time;

        // Hedefe dön
        Vector3 direction = (target.transform.position - transform.position).normalized;
        direction.y = 0; // Sadece yatay düzlemde dön
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        Debug.Log($"{targetHealth.entityName}'e {attackDamage} hasar verildi! Kalan can: {targetHealth.currentHealth}");

        // Düþmaný uyandýr (karþýlýk vermesi için)
        EnemyAI enemyAI = target.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.OnAttacked(gameObject);
        }

        // Saldýrý animasyonu buraya eklenebilir
        // animator.SetTrigger("Attack");
    }

    // Menzil göstergesi için Gizmo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}