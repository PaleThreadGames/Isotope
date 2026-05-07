using UnityEngine;

/// <summary>
/// Animator parameters and death handling. Attack payloads (projectiles, melee) live on enemy-specific components.
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimatorBridge : MonoBehaviour
{
    [SerializeField]
    Animator animator;

    [SerializeField]
    EnemyHealth health;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int IsDeadHash = Animator.StringToHash("isDead");

    [SerializeField]
    bool disablePhysicsOnDeath = true;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void ApplyBindings(EnemyHealth healthRef)
    {
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

        if (disablePhysicsOnDeath)
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
        }
    }

    public bool IsMovementLockedByAttackAnimator
    {
        get
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("Attack") || state.IsName("Spawn") || animator.IsInTransition(0);
        }
    }

    public void SetLocomotionSpeed(float speed)
    {
        animator.SetFloat(SpeedHash, speed);
    }

    public void TriggerAttack()
    {
        animator.SetTrigger(AttackHash);
    }

    /// <summary>Called from death animation event (same GameObject as Animator).</summary>
    public void HandleDeathCleanup()
    {
        Destroy(gameObject);
    }
}
