using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackDamage = 20f;
    public float attackRange = 3f;
    public float attackAngle = 90f; // 90 derece açý
    public float attackCooldown = 1f;
    private float lastAttackTime = 0f;

    [Header("Hit Flash Settings")]
    public float flashDuration = 0.2f;
    public Color flashColor = Color.red;

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
        if (playerLoco != null && playerLoco.isAttacking)
        {
            return;
        }

        // Cooldown kontrolü
        if (Time.time - lastAttackTime < attackCooldown)
        {
            Debug.Log("Çok hýzlý saldýrýyorsunuz!");
            return;
        }

        // AoE saldýrý gerçekleþtir
        PerformAoEAttack();
    }

    private void PerformAoEAttack()
    {
        // Menzildeki tüm düþmanlarý bul
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<GameObject> hitEnemies = new List<GameObject>();

        Vector3 playerForward = transform.forward;
        Vector3 playerPosition = transform.position;

        foreach (GameObject enemy in allEnemies)
        {
            if (enemy == null) continue;

            // Mesafe kontrolü
            float distance = Vector3.Distance(playerPosition, enemy.transform.position);
            if (distance > attackRange) continue;

            // Açý kontrolü
            Vector3 directionToEnemy = (enemy.transform.position - playerPosition).normalized;
            float angle = Vector3.Angle(playerForward, directionToEnemy);

            // 90 derece açý içinde mi?
            if (angle <= attackAngle / 2f)
            {
                // Düþman canlý mý?
                HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
                if (enemyHealth != null && enemyHealth.IsAlive())
                {
                    hitEnemies.Add(enemy);
                }
            }
        }

        // Vurulan düþman var mý?
        if (hitEnemies.Count > 0)
        {
            Debug.Log($"{hitEnemies.Count} düþmana vuruldu!");

            foreach (GameObject enemy in hitEnemies)
            {
                // Hasar ver
                HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);

                    // Kýrmýzý flash efekti
                    enemyHealth.FlashRed();

                    Debug.Log($"{enemyHealth.entityName}'e {attackDamage} hasar! Kalan: {enemyHealth.currentHealth}");
                }

                // Düþmaný uyandýr
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.OnAttacked(gameObject);
                }
            }

            lastAttackTime = Time.time;
        }
        else
        {
            Debug.Log("Hiçbir düþman vurulmadý!");
        }
    }

    // Menzil ve açý göstergesi
    private void OnDrawGizmosSelected()
    {
        // Menzil çemberi
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 90 derece açý göstergesi
        Vector3 forward = transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2f, 0) * forward * attackRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2f, 0) * forward * attackRange;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);

        // Açý alanýný göster
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Vector3 previousPoint = transform.position + rightBoundary;
        for (int i = 0; i <= 20; i++)
        {
            float angle = -attackAngle / 2f + (attackAngle / 20f) * i;
            Vector3 point = transform.position + Quaternion.Euler(0, angle, 0) * forward * attackRange;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
}