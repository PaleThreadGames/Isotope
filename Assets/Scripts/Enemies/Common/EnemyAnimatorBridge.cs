using UnityEngine;

/// <summary>
/// Centralizes animator parameters and animation-driven callbacks (e.g. laser, death cleanup).
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimatorBridge : MonoBehaviour
{
    [SerializeField]
    Animator animator;

    [SerializeField]
    EnemyHealth health;

    [Header("Laser (animation event -> FireLaser)")]
    [SerializeField]
    GameObject laserPrefab;

    [SerializeField]
    Transform firePoint;

    [SerializeField]
    float laserSpeed = 10f;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int IsDeadHash = Animator.StringToHash("isDead");

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void ApplyBindings(GameObject laser, Transform fire, float speed, EnemyHealth healthRef)
    {
        laserPrefab = laser;
        firePoint = fire;
        laserSpeed = speed;
        health = healthRef;
    }

    void OnEnable()
    {
        if (health != null)
            health.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    void HandleDeath()
    {
        animator.SetBool(IsDeadHash, true);

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
    }

    public bool IsMovementLockedByAttackAnimator =>
        animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") || animator.IsInTransition(0);

    public void SetLocomotionSpeed(float speed)
    {
        animator.SetFloat(SpeedHash, speed);
    }

    public void TriggerAttack()
    {
        animator.SetTrigger(AttackHash);
    }

    /// <summary>Called from Attack animation event.</summary>
    public void FireLaser()
    {
        if (laserPrefab == null || firePoint == null)
            return;

        GameObject spawnedLaser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
        float facingDirection = transform.localScale.x > 0f ? 1f : -1f;
        Rigidbody2D laserRb = spawnedLaser.GetComponent<Rigidbody2D>();
        if (laserRb != null)
            laserRb.linearVelocity = new Vector2(facingDirection * laserSpeed, 0f);

        if (facingDirection < 0f)
        {
            Vector3 laserScale = spawnedLaser.transform.localScale;
            laserScale.x *= -1f;
            spawnedLaser.transform.localScale = laserScale;
        }
    }

    /// <summary>Called from death animation event.</summary>
    public void HandleDeathCleanup()
    {
        Destroy(gameObject);
    }
}
