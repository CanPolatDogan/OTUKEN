using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackDamage = 15f;
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float chaseRange = 10f;
    public float returnDistance = 15f; // Spawn noktasýndan bu kadar uzaklaþýrsa geri döner

    [Header("State")]
    public GameObject currentTarget;
    private Vector3 spawnPosition;
    private bool isAggressive = false;

    private HealthSystem enemyHealth;

    private enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Returning
    }

    private EnemyState currentState = EnemyState.Idle;

    private void Start()
    {
        enemyHealth = GetComponent<HealthSystem>();
        spawnPosition = transform.position;
    }

    private void Update()
    {
        if (enemyHealth == null || !enemyHealth.IsAlive())
        {
            return;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                IdleBehavior();
                break;
            case EnemyState.Chasing:
                ChaseBehavior();
                break;
            case EnemyState.Attacking:
                AttackBehavior();
                break;
            case EnemyState.Returning:
                ReturnBehavior();
                break;
        }
    }

    private void IdleBehavior()
    {
        // Yakýnda oyuncu var mý kontrol et
        if (isAggressive && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distance <= chaseRange)
            {
                currentState = EnemyState.Chasing;
            }
        }
    }

    private void ChaseBehavior()
    {
        if (currentTarget == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        // Hedef ölmüþ mü kontrol et
        HealthSystem targetHealth = currentTarget.GetComponent<HealthSystem>();
        if (targetHealth == null || !targetHealth.IsAlive())
        {
            currentTarget = null;
            isAggressive = false;
            currentState = EnemyState.Returning;
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        float spawnDistance = Vector3.Distance(transform.position, spawnPosition);

        // Çok uzaklaþtýysa geri dön
        if (spawnDistance > returnDistance)
        {
            currentState = EnemyState.Returning;
            return;
        }

        // Saldýrý menzilindeyse saldýr
        if (distance <= attackRange)
        {
            currentState = EnemyState.Attacking;
        }
        else if (distance <= chaseRange)
        {
            // Hedefe doðru hareket et
            MoveTowards(currentTarget.transform.position);
        }
        else
        {
            // Hedef çok uzaklaþtý
            currentState = EnemyState.Returning;
        }
    }

    private void AttackBehavior()
    {
        if (currentTarget == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

        // Hedefe dön
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }

        // Menzil dýþýna çýktýysa kovala
        if (distance > attackRange)
        {
            currentState = EnemyState.Chasing;
            return;
        }

        // Saldýrý gerçekleþtir
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
        }
    }

    private void ReturnBehavior()
    {
        float distance = Vector3.Distance(transform.position, spawnPosition);

        if (distance < 1f)
        {
            // Spawn noktasýna ulaþtý
            currentTarget = null;
            isAggressive = false;
            currentState = EnemyState.Idle;

            // Caný yenile (opsiyonel)
            if (enemyHealth != null)
            {
                enemyHealth.Heal(enemyHealth.maxHealth);
            }
        }
        else
        {
            MoveTowards(spawnPosition);
        }
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }

        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void PerformAttack()
    {
        if (currentTarget == null) return;

        HealthSystem targetHealth = currentTarget.GetComponent<HealthSystem>();
        if (targetHealth != null && targetHealth.IsAlive())
        {
            targetHealth.TakeDamage(attackDamage);
            lastAttackTime = Time.time;

            Debug.Log($"{enemyHealth.entityName} oyuncuya {attackDamage} hasar verdi!");

            // Saldýrý animasyonu buraya eklenebilir
            // animator.SetTrigger("Attack");
        }
    }

    // Oyuncu saldýrdýðýnda çaðrýlýr
    public void OnAttacked(GameObject attacker)
    {
        if (!isAggressive)
        {
            isAggressive = true;
            currentTarget = attacker;
            currentState = EnemyState.Chasing;
            Debug.Log($"{enemyHealth.entityName} saldýrýya uðradý ve karþýlýk veriyor!");
        }
    }

    // Görsel gösterim için
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPosition, returnDistance);
        }
    }
}