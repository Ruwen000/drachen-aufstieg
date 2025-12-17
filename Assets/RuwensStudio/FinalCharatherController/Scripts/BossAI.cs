using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Game Manager")]
    public GameManager gameManager; 

    [Header("UI")]
    public HealthBarUI bossHealthUI;

    [Header("Referenzen")]
    public Transform player;
    public Transform eyePoint;

    [Header("Sicht / Aufwachen")]
    public float detectionRadius = 30f;
    public float eyeHeight = 2f;
    public LayerMask visionBlockers;
    private bool hasSeenPlayer = false;

    [Header("Allgemeine Bewegung")]
    public float speed = 8f;
    public float rotationSpeed = 5f;
    public bool alwaysFacePlayerAfterDetection = true;

    [Header("Aggro & Verhalten")]
    public float aggroRadius = 25f;
    public bool persistentAggro = true;

    [Header("Stopp Abstand")]
    public float minStopDistance = 6f;
    public float maxStopDistance = 12f;

    [Header("Attacken - Basis")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 20f;
    public float projectileLifetime = 6f;

    [Header("Attacken - Intervalle")]
    public float attackIntervalMin = 1.0f;
    public float attackIntervalMax = 3.0f;

    [Header("Aggression Skalierung mit HP")]
    public bool scaleAttackRateWithHealth = true;
    public float minIntervalAtLowHealth = 0.2f;
    public float maxIntervalAtLowHealth = 0.6f;
    public bool scaleProjectileSpeedWithHealth = true;
    public float projectileSpeedAtLowHealthMultiplier = 1.5f;

    [Header("Lebenspunkte")]
    public float maxHealth = 50f;
    public bool destroyOnDeath = true;

    [Header("Death Effects")]
    public GameObject deathEffectPrefab;
    public AudioClip deathSound;
    public float deathEffectDuration = 3f;

    private float currentHealth;
    private enum State { Idle, MovingToPlayer, Attacking }
    private State state = State.Idle;
    private float targetStopDistance = 8f;
    private Coroutine attackCoroutine;
    private Rigidbody rb;
    private bool isDead = false;

    private float lastDamageTime = -999f;
    public float damageCooldown = 0.02f;

    void Start()
    {
        if (bossHealthUI != null && bossHealthUI.container != null)
            bossHealthUI.container.SetActive(false);

        currentHealth = maxHealth;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        targetStopDistance = Random.Range(minStopDistance, maxStopDistance);

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
            if (gameManager != null)
                Debug.Log("GameManager gefunden: " + gameManager.name);
            else
                Debug.LogError("GameManager nicht gefunden!");
        }
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        if (!hasSeenPlayer)
        {
            float distDetect = Vector3.Distance(GetEyePosition(), player.position);
            if (distDetect <= detectionRadius && HasLineOfSightToPlayer())
            {
                hasSeenPlayer = true;
                if (bossHealthUI != null && bossHealthUI.container != null)
                    bossHealthUI.container.SetActive(true);
                EnterMovingState();
            }
            else
            {
                return;
            }
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (hasSeenPlayer && alwaysFacePlayerAfterDetection)
        {
            FaceTarget(player.position);
        }

        if (persistentAggro)
        {
            if (distToPlayer > targetStopDistance)
            {
                state = State.MovingToPlayer;
                MoveTowardsPlayer();
            }
            else
            {
                if (state != State.Attacking)
                    EnterAttackingState();
            }
        }
        else
        {
            switch (state)
            {
                case State.Idle:
                    if (distToPlayer <= aggroRadius)
                        EnterMovingState();
                    break;

                case State.MovingToPlayer:
                    if (distToPlayer > targetStopDistance)
                    {
                        MoveTowardsPlayer();
                    }
                    else
                    {
                        EnterAttackingState();
                    }

                    if (distToPlayer > aggroRadius + 5f)
                    {
                        EnterIdleState();
                    }
                    break;

                case State.Attacking:
                    if (distToPlayer > maxStopDistance + 1f)
                    {
                        EnterMovingState();
                    }
                    if (distToPlayer > aggroRadius + 5f)
                    {
                        EnterIdleState();
                    }
                    break;
            }
        }
    }

    Vector3 GetEyePosition()
    {
        if (eyePoint != null) return eyePoint.position;
        return transform.position + Vector3.up * eyeHeight;
    }

    bool HasLineOfSightToPlayer()
    {
        Vector3 origin = GetEyePosition();
        Vector3 dir = (player.position - origin);
        float dist = dir.magnitude;
        if (dist <= 0.001f) return true;
        dir /= dist;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, visionBlockers))
        {
            return false;
        }

        return true;
    }

    void MoveTowardsPlayer()
    {
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Vector3 move = dir.normalized * speed * Time.deltaTime;
            transform.position += move;
        }
    }

    void EnterIdleState()
    {
        state = State.Idle;
        StopAttacking();
    }

    void EnterMovingState()
    {
        state = State.MovingToPlayer;
        StopAttacking();
        targetStopDistance = Random.Range(minStopDistance, maxStopDistance);
    }

    void EnterAttackingState()
    {
        state = State.Attacking;
        StartAttacking();
    }

    void StartAttacking()
    {
        if (attackCoroutine == null)
            attackCoroutine = StartCoroutine(AttackRoutine());
    }

    void StopAttacking()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            float t = 1f - Mathf.Clamp01(currentHealth / maxHealth);

            float currentMin = attackIntervalMin;
            float currentMax = attackIntervalMax;
            if (scaleAttackRateWithHealth)
            {
                currentMin = Mathf.Lerp(attackIntervalMin, minIntervalAtLowHealth, t);
                currentMax = Mathf.Lerp(attackIntervalMax, maxIntervalAtLowHealth, t);
                if (currentMax < currentMin) currentMax = currentMin + 0.01f;
            }

            float wait = Random.Range(currentMin, currentMax);
            yield return new WaitForSeconds(wait);

            if (currentHealth <= 0f) yield break;

            if (projectilePrefab != null && projectileSpawnPoint != null && player != null)
            {
                float curProjSpeed = projectileSpeed;
                if (scaleProjectileSpeedWithHealth)
                {
                    curProjSpeed = projectileSpeed * Mathf.Lerp(1f, projectileSpeedAtLowHealthMultiplier, t);
                }

                GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

                Vector3 dir = (player.position - projectileSpawnPoint.position).normalized;
                if (dir.sqrMagnitude < 0.0001f) dir = projectileSpawnPoint.forward;
                proj.transform.rotation = Quaternion.LookRotation(dir);

                if (proj.transform.localScale.magnitude < 0.01f)
                    proj.transform.localScale = Vector3.one;

                Rigidbody projRb = proj.GetComponent<Rigidbody>();
                BossProjectile bp = proj.GetComponent<BossProjectile>();

                if (projRb != null)
                {
                    projRb.isKinematic = false;
                    projRb.useGravity = false;
                    projRb.linearVelocity = dir * curProjSpeed;
                }
                else if (bp != null)
                {
                    bp.Initialize(dir, curProjSpeed, projectileLifetime);
                }
                else
                {
                    bp = proj.AddComponent<BossProjectile>();
                    bp.Initialize(dir, curProjSpeed, projectileLifetime);
                }

                Destroy(proj, projectileLifetime);
            }
        }
    }

    void FaceTarget(Vector3 pos)
    {
        Vector3 lookDir = pos - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude < 0.0001f) return;
        Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        if (Time.time - lastDamageTime < damageCooldown) return;
        lastDamageTime = Time.time;

        float damageMultiplier = SkillSystem.Instance.GetDamageMultiplier();
        amount = amount * 100 * damageMultiplier; 

        currentHealth -= amount;
        Debug.Log($"Boss took {amount:F2} dmg. HP left: {currentHealth:F2}");

        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Die() Methode wird aufgerufen!");

        if (bossHealthUI != null && bossHealthUI.container != null)
            bossHealthUI.container.SetActive(false);

        StopAttacking();
        if (destroyOnDeath)
        {
            Debug.Log("Boss wird zerstört!");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Boss wird deaktiviert!");
            gameObject.SetActive(false);
        }

        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, deathEffectDuration);
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }


        if (gameManager != null)
        {
            Invoke("NotifyGameManager", 1f);
        }
        else
        {
            if (destroyOnDeath)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
    }

    void NotifyGameManager()
    {


        if (destroyOnDeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = hasSeenPlayer ? Color.magenta : Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * eyeHeight, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minStopDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxStopDistance);
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}