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
    public float walkSpeed = 1f; // Patrol için yürüme hýzý
    public float chaseRange = 10f;
    public float returnDistance = 15f;

    [Header("Patrol Settings")]
    public float patrolRadius = 5f;
    public float patrolWaitTime = 2f; // Noktalarda bekleme süresi
    public float patrolIdleTime = 3f; // Yürürken durup idle yapma süresi
    public float patrolPointReachDistance = 0.5f;
    private Vector3 currentPatrolTarget;
    private float patrolWaitTimer = 0f;
    private float patrolIdleTimer = 0f;
    private bool isWaitingAtPatrolPoint = false;
    private bool isIdleWhilePatrolling = false; // Yürürken durma durumu

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

        SetNewPatrolPoint();

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

        if (currentTarget != null)
        {
            HealthSystem targetHealth = currentTarget.GetComponent<HealthSystem>();
            if (targetHealth != null && !targetHealth.IsAlive())
            {
                OnPlayerDeath();
                return;
            }
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
        if (!isAggressive)
        {
            PatrolBehavior();
        }
        else
        {
            SetWalkingAnimation(false);
            SetRunningAnimation(false);
        }

        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distance <= chaseRange)
            {
                isAggressive = true;
                currentState = EnemyState.Chasing;
            }
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= chaseRange)
                {
                    currentTarget = player;
                    isAggressive = true;
                    currentState = EnemyState.Chasing;
                }
            }
        }
    }

    private void SetNewPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);
        currentPatrolTarget = spawnPosition + new Vector3(randomX, 0, randomZ);
    }

    private void PatrolBehavior()
    {
        // Yürürken durma (idle) durumu
        if (isIdleWhilePatrolling)
        {
            SetWalkingAnimation(false);
            patrolIdleTimer += Time.deltaTime;

            if (patrolIdleTimer >= patrolIdleTime)
            {
                isIdleWhilePatrolling = false;
                patrolIdleTimer = 0f;
            }
            return;
        }

        // Noktaya ulaþýnca bekleme durumu
        if (isWaitingAtPatrolPoint)
        {
            SetWalkingAnimation(false);
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                isWaitingAtPatrolPoint = false;
                patrolWaitTimer = 0f;
                SetNewPatrolPoint();

                // Yeni noktaya giderken %50 þans ile durup idle yap
                if (Random.value > 0.5f)
                {
                    isIdleWhilePatrolling = true;
                }
            }
            return;
        }

        // Patrol noktasýna yürüyerek git
        SetWalkingAnimation(true);
        float distance = Vector3.Distance(transform.position, currentPatrolTarget);

        if (distance < patrolPointReachDistance)
        {
            isWaitingAtPatrolPoint = true;
        }
        else
        {
            MoveTowards(currentPatrolTarget, walkSpeed);
        }
    }

    private void ChaseBehavior()
    {
        SetWalkingAnimation(false);
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
            MoveTowards(currentTarget.transform.position, moveSpeed);
        }
        else
        {
            currentState = EnemyState.Returning;
        }
    }

    private void AttackBehavior()
    {
        SetWalkingAnimation(false);
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
        SetWalkingAnimation(false);
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
            MoveTowards(spawnPosition, moveSpeed);
        }
    }

    private void MoveTowards(Vector3 targetPosition, float speed)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }

        transform.position += direction * speed * Time.deltaTime;
    }

    private void PerformAttack()
    {
        if (currentTarget == null) return;

        TriggerAttackAnimation();

        HealthSystem targetHealth = currentTarget.GetComponent<HealthSystem>();
        if (targetHealth != null && targetHealth.IsAlive())
        {
            float finalDamage = attackDamage;

            PlayerLocomotion playerLoco = currentTarget.GetComponent<PlayerLocomotion>();
            if (playerLoco != null && playerLoco.isDefending)
            {
                float playerYRotation = currentTarget.transform.eulerAngles.y;

                Vector3 directionToEnemy = (transform.position - currentTarget.transform.position).normalized;
                float angleToEnemy = Mathf.Atan2(directionToEnemy.x, directionToEnemy.z) * Mathf.Rad2Deg;

                float angleDifference = Mathf.DeltaAngle(playerYRotation, angleToEnemy);

                if (Mathf.Abs(angleDifference) <= 80f)
                {
                    finalDamage *= 0.2f;
                    Debug.Log($"{enemyHealth.entityName} saldýrýsý BLOKLANDI! Hasar: {attackDamage} ? {finalDamage}");
                }
                else
                {
                    Debug.Log($"{enemyHealth.entityName} arkadan saldýrdý! Savunma etkisiz. Açý farký: {angleDifference}°");
                }
            }

            targetHealth.TakeDamage(finalDamage);
            lastAttackTime = Time.time;

            Debug.Log($"{enemyHealth.entityName} oyuncuya {finalDamage} hasar verdi!");
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

    public void OnPlayerDeath()
    {
        Debug.Log($"{enemyHealth.entityName} savaþý kazandý ve idle durumuna geçiyor.");

        // Hedefi temizle
        currentTarget = null;
        isAggressive = false;

        // Idle durumuna geç
        currentState = EnemyState.Idle;

        // Tüm animasyonlarý sýfýrla
        SetWalkingAnimation(false);
        SetRunningAnimation(false);

        isWaitingAtPatrolPoint = true;
        isIdleWhilePatrolling = false;
        patrolWaitTimer = 0f;
        patrolIdleTimer = 0f;

        // Yeni patrol noktasý belirle
        SetNewPatrolPoint();
    }

    private void SetWalkingAnimation(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
        }
    }

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
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.ResetTrigger("attack");

            animator.SetBool("isDead", true);

            Debug.Log("Death animasyonu aktifleþtirildi - Animator enabled: " + animator.enabled);
        }
    }

    public void OnDeath()
    {
        SetDeathAnimation();
        Invoke("DisableAI", 0.1f);
    }

    private void DisableAI()
    {
        this.enabled = false;
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