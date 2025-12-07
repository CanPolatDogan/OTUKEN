using System.Collections;
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
    public float returnDistance = 15f;

    [Header("State")]
    public GameObject currentTarget;
    public bool isAggressive = false;
    private Vector3 spawnPosition;

    [Header("Animation")]
    private Animator animator;

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
        animator = GetComponent<Animator>();
        spawnPosition = transform.position;

        // Animator kontrolü
        if (animator == null)
        {
            Debug.LogError($"{gameObject.name} üzerinde Animator component bulunamadý!");
        }
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
        // Idle animasyonu (isRunning false olunca otomatik oynar)
        SetRunningAnimation(false);

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
        // Koþma animasyonu
        SetRunningAnimation(true);

        if (currentTarget == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

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

        if (spawnDistance > returnDistance)
        {
            currentState = EnemyState.Returning;
            return;
        }

        if (distance <= attackRange)
        {
            currentState = EnemyState.Attacking;
        }
        else if (distance <= chaseRange)
        {
            MoveTowards(currentTarget.transform.position);
        }
        else
        {
            currentState = EnemyState.Returning;
        }
    }

    private void AttackBehavior()
    {
        // Saldýrýrken koþma animasyonunu durdur
        SetRunningAnimation(false);

        if (currentTarget == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }

        if (distance > attackRange)
        {
            currentState = EnemyState.Chasing;
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
        }
    }

    private void ReturnBehavior()
    {
        // Geri dönerken koþma animasyonu
        SetRunningAnimation(true);

        float distance = Vector3.Distance(transform.position, spawnPosition);

        if (distance < 1f)
        {
            currentTarget = null;
            isAggressive = false;
            currentState = EnemyState.Idle;

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

        // Saldýrý animasyonunu tetikle
        TriggerAttackAnimation();

        HealthSystem targetHealth = currentTarget.GetComponent<HealthSystem>();
        if (targetHealth != null && targetHealth.IsAlive())
        {
            targetHealth.TakeDamage(attackDamage);
            lastAttackTime = Time.time;

            Debug.Log($"{enemyHealth.entityName} oyuncuya {attackDamage} hasar verdi!");
        }
    }

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

    // Animasyon metodlarý
    private void SetRunningAnimation(bool isRunning)
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", isRunning);
        }
    }

    private void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }
    }

    private void SetDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
    }

    // HealthSystem'den çaðrýlabilmesi için public metod
    public void OnDeath()
    {
        StartCoroutine(SmoothMoveDown());
        SetDeathAnimation();
        SetRunningAnimation(false);

        // AI'ý devre dýþý býrak
        this.enabled = false;
    }

    IEnumerator SmoothMoveDown()
    {
        float duration = 0.5f;      // toplam süre
        float targetAmount = -1.7f; // toplam azalacak miktar
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + new Vector3(0, targetAmount, 0);

        yield return new WaitForSeconds(1.7f); // 0.5 saniye bekler

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos; // Tam noktasýna oturt
    }

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