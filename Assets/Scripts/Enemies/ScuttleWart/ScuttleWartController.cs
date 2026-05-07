using UnityEngine;

/// <summary>
/// Thin orchestrator: wires shared components. Attack animation events target <see cref="ScuttleWartProjectileAttack"/>; death cleanup targets <see cref="EnemyAnimatorBridge"/>.
/// </summary>
[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyPerception))]
[RequireComponent(typeof(EnemyGroundMovement))]
[RequireComponent(typeof(EnemyAnimatorBridge))]
[RequireComponent(typeof(ScuttleWartProjectileAttack))]
public class ScuttleWartController : MonoBehaviour
{
    [Header("Optional ScriptableObject configs")]
    [SerializeField]
    EnemyMovementConfig movementConfig;

    [SerializeField]
    EnemyCombatConfig combatConfig;

    [SerializeField]
    EnemyPerceptionConfig perceptionConfig;

    [SerializeField]
    PlayerReferenceSO playerReference;

    [Header("Health (legacy when combatConfig is null)")]
    [SerializeField]
    float health = 3f;

    [Header("Movement (legacy when movementConfig is null)")]
    [SerializeField]
    float moveSpeed = 2f;

    [Header("Vision & Memory (legacy when perceptionConfig is null)")]
    [SerializeField]
    Transform eyePoint;

    [SerializeField]
    float detectionRadius = 8f;

    [SerializeField]
    float attackRadius = 1.5f;

    [SerializeField]
    float hearingRadius = 3f;

    [SerializeField, Range(0f, 360f)]
    float visionAngle = 120f;

    [SerializeField]
    float flipDeadzone = 0.5f;

    [SerializeField]
    float memoryDuration = 1.5f;

    [SerializeField]
    LayerMask visionBlockers;

    [Header("Advanced AI & Physics")]
    [SerializeField]
    float retreatDistance = 2.5f;

    [SerializeField]
    float knockbackForce = 18f;

    [Header("Platform Settings")]
    [SerializeField]
    Transform groundCheck;

    [SerializeField]
    LayerMask groundLayer;

    [SerializeField]
    float groundCheckDistance = 0.5f;

    [Header("Patrol Settings")]
    [SerializeField]
    float patrolWaitTime = 2f;

    EnemyHealth _enemyHealth;
    EnemyPerception _perception;
    EnemyGroundMovement _motor;
    EnemyAnimatorBridge _animBridge;
    ScuttleWartBrain _brain;

    /// <summary>Inspector / debugging — driven by brain.</summary>
    public ScuttleWartBrain.State CurrentState => _brain != null ? _brain.CurrentState : ScuttleWartBrain.State.Patrolling;

    void Awake()
    {
        _enemyHealth = GetComponent<EnemyHealth>();
        _perception = GetComponent<EnemyPerception>();
        _motor = GetComponent<EnemyGroundMovement>();
        _animBridge = GetComponent<EnemyAnimatorBridge>();

        _enemyHealth.ApplyBindings(combatConfig, health);

        _perception.ApplyBindings(
            perceptionConfig,
            eyePoint,
            playerReference,
            retreatDistance,
            detectionRadius,
            attackRadius,
            hearingRadius,
            visionAngle,
            memoryDuration,
            visionBlockers);

        _motor.ApplyBindings(
            movementConfig,
            groundCheck,
            groundLayer,
            moveSpeed,
            flipDeadzone,
            groundCheckDistance,
            patrolWaitTime);

        _animBridge.ApplyBindings(_enemyHealth);

        _brain = new ScuttleWartBrain(_perception, _motor, _animBridge, _enemyHealth, playerReference);
    }

    void FixedUpdate()
    {
        _brain.TickFixed();
    }

    public void TakeDamage(float amount)
    {
        _enemyHealth.TakeDamage(amount);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (_enemyHealth.IsDead || !collision.gameObject.CompareTag("Player"))
            return;

        _enemyHealth.TakeDamage(1f);

        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            float knock = combatConfig != null ? combatConfig.knockbackForceToPlayer : knockbackForce;
            float dirX = collision.transform.position.x > transform.position.x ? 1f : -1f;
            Vector2 knockbackVector = new Vector2(dirX * 1.2f, 0.2f).normalized;

            playerRb.linearVelocity = Vector2.zero;
            playerRb.AddForce(knockbackVector * knock, ForceMode2D.Impulse);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }
    }
#endif
}
