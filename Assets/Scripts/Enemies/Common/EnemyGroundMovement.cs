using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Ground motor: patrol edge/wall, chase, flip. Does not decide why it moves.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyGroundMovement : MonoBehaviour, IMovable
{
    [SerializeField]
    EnemyMovementConfig movementConfig;

    [SerializeField]
    Transform groundCheck;

    [SerializeField]
    LayerMask groundLayer;

    [Header("Legacy (when movementConfig is null)")]
    [SerializeField]
    float moveSpeed = 2f;

    [SerializeField]
    float flipDeadzone = 0.5f;

    [SerializeField]
    float groundCheckDistance = 0.5f;

    [SerializeField]
    float patrolWaitTime = 2f;

    Rigidbody2D _rb;
    int _moveDirection = 1;
    bool _isWaiting;

    /// <summary>Set by orchestrator: still patrolling when patrol wait coroutine ends.</summary>
    public Func<bool> IsPatrollingStateActive { get; set; }

    public Transform Transform => transform;

    public bool IsPatrolWaiting => _isWaiting;

    float EffectiveMoveSpeed => movementConfig != null ? movementConfig.moveSpeed : moveSpeed;
    float EffectiveFlipDeadzone => movementConfig != null ? movementConfig.flipDeadzone : flipDeadzone;
    float EffectiveGroundCheckDistance => movementConfig != null ? movementConfig.groundCheckDistance : groundCheckDistance;
    float EffectivePatrolWait => movementConfig != null ? movementConfig.patrolWaitTime : patrolWaitTime;
    float PatrolSpeedMult => movementConfig != null ? movementConfig.patrolSpeedMultiplier : 0.5f;
    float RetreatSpeedMult => movementConfig != null ? movementConfig.retreatSpeedMultiplier : 0.75f;
    float MinMass => movementConfig != null ? movementConfig.minRigidbodyMass : 100f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb.mass < 50f && movementConfig != null)
            _rb.mass = Mathf.Max(_rb.mass, MinMass);
        else if (_rb.mass < 50f)
            _rb.mass = 100f;

        if (_moveDirection == 0)
            _moveDirection = 1;
    }

    public void ApplyBindings(
        EnemyMovementConfig config,
        Transform groundCheckRef,
        LayerMask layer,
        float legacySpeed,
        float legacyFlipDeadzone,
        float legacyGroundDist,
        float legacyPatrolWait)
    {
        movementConfig = config;
        groundCheck = groundCheckRef;
        groundLayer = layer;
        if (movementConfig == null)
        {
            moveSpeed = legacySpeed;
            flipDeadzone = legacyFlipDeadzone;
            groundCheckDistance = legacyGroundDist;
            patrolWaitTime = legacyPatrolWait;
        }
    }

    public void StopHorizontal()
    {
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
    }

    public void PatrolFixedUpdate()
    {
        if (_isWaiting)
            return;

        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, EffectiveGroundCheckDistance, groundLayer);
        bool isWallAhead = Physics2D.Raycast(groundCheck.position, new Vector2(_moveDirection, 0f), 0.2f, groundLayer);

        if (!isGroundAhead || isWallAhead)
        {
            StartCoroutine(WaitAndFlip());
            return;
        }

        _rb.linearVelocity = new Vector2(_moveDirection * (EffectiveMoveSpeed * PatrolSpeedMult), _rb.linearVelocity.y);
        HandleFlipping(_moveDirection);
    }

    /// <returns>True if horizontal move was applied (ground ahead).</returns>
    public bool ChaseFixedUpdate(Transform target, bool retreating)
    {
        if (_isWaiting)
            _isWaiting = false;

        float dx = target.position.x - transform.position.x;
        int moveDir = dx > 0f ? 1 : -1;

        if (Mathf.Abs(dx) > EffectiveFlipDeadzone)
            HandleFlipping(moveDir);

        if (retreating)
            moveDir *= -1;

        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, EffectiveGroundCheckDistance, groundLayer);

        if (isGroundAhead)
        {
            float mult = retreating ? RetreatSpeedMult : 1f;
            _rb.linearVelocity = new Vector2(moveDir * EffectiveMoveSpeed * mult, _rb.linearVelocity.y);
            return true;
        }

        StopHorizontal();
        return false;
    }

    public void PrepareAttackFacing(Transform target)
    {
        float dx = target.position.x - transform.position.x;
        if (Mathf.Abs(dx) > EffectiveFlipDeadzone)
            HandleFlipping(dx > 0f ? 1 : -1);
    }

    IEnumerator WaitAndFlip()
    {
        _isWaiting = true;
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        yield return new WaitForSeconds(EffectivePatrolWait);

        bool ok = IsPatrollingStateActive != null && IsPatrollingStateActive();
        if (ok)
            Flip();

        _isWaiting = false;
    }

    void HandleFlipping(int direction)
    {
        if (direction > 0 && transform.localScale.x < 0f)
            Flip();
        else if (direction < 0 && transform.localScale.x > 0f)
            Flip();
    }

    void Flip()
    {
        _moveDirection *= -1;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1f;
        transform.localScale = newScale;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.cyan;
        float dist = movementConfig != null ? movementConfig.groundCheckDistance : groundCheckDistance;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * dist);
    }
}
