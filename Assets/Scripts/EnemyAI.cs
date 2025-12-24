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

    [Header("Patrol Settings")]
    public float patrolRadius = 5f; // 10x10 alan için 5 birim yarýçap
    public float patrolWaitTime = 2f; // Noktalarda bekleme süresi
    public float patrolPointReachDistance = 0.5f;
    private Vector3 currentPatrolTarget;
    private float patrolWaitTimer = 0f;
    private bool isWaitingAtPatrolPoint = false;

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

        // Ýlk patrol noktasýný belirle
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

        // Hedef ölmüþse idle'a dön
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
        // Agresif deðilse patrol yap
        if (!isAggressive)
        {
            PatrolBehavior();
        }
        else
        {
            SetRunningAnimation(false);
        }

        // Oyuncu menzile girdi mi kontrol et
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
            // Oyuncu aramaya devam et
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
        // Spawn noktasýndan 10x10'luk alanda rastgele nokta
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);
        currentPatrolTarget = spawnPosition + new Vector3(randomX, 0, randomZ);
    }

    private void PatrolBehavior()
    {
        // Koþma animasyonu
        SetRunningAnimation(true);

        // Bekleme durumu
        if (isWaitingAtPatrolPoint)
        {
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                isWaitingAtPatrolPoint = false;
                patrolWaitTimer = 0f;
                SetNewPatrolPoint();
            }
            return;
        }

        // Patrol noktasýna git
        float distance = Vector3.Distance(transform.position, currentPatrolTarget);

        if (distance < patrolPointReachDistance)
        {
            // Noktaya ulaþtý, bekle
            isWaitingAtPatrolPoint = true;
            SetRunningAnimation(false); // Beklerken dur
        }
        else
        {
            MoveTowards(currentPatrolTarget);
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
            float finalDamage = attackDamage;

            // Oyuncu savunma yapýyor mu kontrol et
            PlayerLocomotion playerLoco = currentTarget.GetComponent<PlayerLocomotion>();
            if (playerLoco != null && playerLoco.isDefending)
            {
                // Oyuncunun baktýðý yön
                float playerYRotation = currentTarget.transform.eulerAngles.y;

                // Düþmanýn açýsýný hesapla (oyuncuya göre)
                Vector3 directionToEnemy = (transform.position - currentTarget.transform.position).normalized;
                float angleToEnemy = Mathf.Atan2(directionToEnemy.x, directionToEnemy.z) * Mathf.Rad2Deg;

                // Açý farkýný hesapla (-180 ile 180 arasýnda normalize et)
                float angleDifference = Mathf.DeltaAngle(playerYRotation, angleToEnemy);

                // Eðer düþman önde ise (100 derece içinde)
                if (Mathf.Abs(angleDifference) <= 80f)
                {
                    // %80 hasar azaltma
                    finalDamage *= 0.2f; // 0.2 = %20 hasar (yani %80 azaltma)
                    Debug.Log($"{enemyHealth.entityName} saldýrýsý BLOKLANDI! Hasar: {attackDamage}  {finalDamage}");
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

    // Oyuncu öldüðünde çaðrýlýr
    public void OnPlayerDeath()
    {
        Debug.Log($"{enemyHealth.entityName} savaþý kazandý ve geri dönüyor.");

        currentTarget = null;
        isAggressive = false;
        currentState = EnemyState.Returning;

        // Animasyonlarý sýfýrla
        SetRunningAnimation(true); // Geri dönerken koþacak
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
            // Tüm trigger ve bool'larý sýfýrla
            animator.SetBool("isRunning", false);
            animator.ResetTrigger("attack");

            // Death'i aktif et
            animator.SetBool("isDead", true);

            Debug.Log("Death animasyonu aktifleþtirildi - Animator enabled: " + animator.enabled);
        }
    }

    // HealthSystem'den çaðrýlabilmesi için public metod
    public void OnDeath()
    {
        SetDeathAnimation();

        // Kýsa bir gecikme ile AI'ý kapat (animasyon baþlasýn diye)
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