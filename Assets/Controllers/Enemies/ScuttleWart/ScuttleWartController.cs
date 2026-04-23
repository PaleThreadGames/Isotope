using UnityEngine;

public class ScuttleWartController : MonoBehaviour
{
    public enum State { Patrolling, Chasing, Attacking }
    [Header("State")]
    public State currentState = State.Patrolling;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    
    [Header("Vision & Memory Settings")]
    public Transform eyePoint; // NEW: The origin point for the vision cone
    public float detectionRadius = 8f;
    public float attackRadius = 1.5f;
    public float hearingRadius = 3f; 
    [Range(0, 360)]
    public float visionAngle = 120f; 
    public float flipDeadzone = 0.5f; 
    public float memoryDuration = 1.5f; 
    
    private float timeSinceSawPlayer = 0f;

    [Header("Platform Settings")]
    public Transform groundCheck; 
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.5f;

    [Header("Patrol Settings")]
    public float patrolWaitTime = 2f;
    private bool isWaiting = false;
    private int moveDirection = 1; 

    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (moveDirection == 0) moveDirection = 1; 
        timeSinceSawPlayer = memoryDuration; 
    }

    void FixedUpdate() {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") || anim.IsInTransition(0)) {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return; 
        }

        // 1. DEFINE WHERE THE BUG IS LOOKING FROM
        // Fallback to transform.position if you forget to assign the eyePoint in the Inspector
        Vector3 sightOrigin = (eyePoint != null) ? eyePoint.position : transform.position;

        // 2. CALCULATE DISTANCES AND ANGLES FROM THE EYE
        float distanceToPlayer = Vector2.Distance(sightOrigin, player.position);
        Vector2 dirToPlayer = (player.position - sightOrigin).normalized;
        Vector2 bugForward = (transform.localScale.x > 0) ? Vector2.right : Vector2.left;
        float angleToPlayer = Vector2.Angle(bugForward, dirToPlayer);

        // 3. THE SENSES
        bool inVisionCone = distanceToPlayer < detectionRadius && angleToPlayer < (visionAngle / 2f);
        
        // Hearing usually still originates from the center/body, not the eye
        bool inHearingRange = Vector2.Distance(transform.position, player.position) < hearingRadius;
        
        if (inVisionCone || inHearingRange) {
            timeSinceSawPlayer = 0f;
        } else {
            timeSinceSawPlayer += Time.fixedDeltaTime;
        }

        bool remembersPlayer = timeSinceSawPlayer < memoryDuration;
        bool inAttackRange = distanceToPlayer < attackRadius && (inVisionCone || inHearingRange);

        // 4. STATE SWITCHING
        if (inAttackRange) {
            currentState = State.Attacking;
            isWaiting = false; 
        }
        else if (remembersPlayer) {
            currentState = State.Chasing;
            isWaiting = false; 
        }
        else {
            currentState = State.Patrolling;
        }

        switch (currentState) {
            case State.Patrolling: Patrol(); break;
            case State.Chasing: Chase(); break;
            case State.Attacking: StopAndAttack(); break;
        }
    }

    void Patrol() {
        if (isWaiting) return;

        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        bool isWallAhead = Physics2D.Raycast(groundCheck.position, new Vector2(moveDirection, 0), 0.2f, groundLayer);

        if (!isGroundAhead || isWallAhead) {
            StartCoroutine(WaitAndFlip()); 
            return; 
        }

        rb.linearVelocity = new Vector2(moveDirection * (moveSpeed * 0.5f), rb.linearVelocity.y);
        anim.SetFloat("Speed", 1f);
        HandleFlipping(moveDirection);
    }

    void Chase() {
        if (isWaiting) isWaiting = false; 

        float playerDirX = player.position.x - transform.position.x;
        
        if (Mathf.Abs(playerDirX) > flipDeadzone) {
            int dirX = (playerDirX > 0) ? 1 : -1;
            HandleFlipping(dirX);
        }

        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        
        if (isGroundAhead) {
            float moveDir = (transform.localScale.x > 0) ? 1 : -1;
            rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
            anim.SetFloat("Speed", 1f);
        } else {
            StopAndIdle();
        }
    }

    void StopAndAttack() {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetFloat("Speed", 0f);

        float playerDirX = player.position.x - transform.position.x;
        if (Mathf.Abs(playerDirX) > flipDeadzone) {
            HandleFlipping((playerDirX > 0) ? 1 : -1);
        }

        anim.SetTrigger("Attack");
    }

    void StopAndIdle() {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetFloat("Speed", 0f);
    }

    void HandleFlipping(int direction) {
        if (direction > 0 && transform.localScale.x < 0) Flip();
        else if (direction < 0 && transform.localScale.x > 0) Flip();
    }

    void Flip() {
        moveDirection *= -1;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    System.Collections.IEnumerator WaitAndFlip() {
        isWaiting = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetFloat("Speed", 0f);
        
        yield return new WaitForSeconds(patrolWaitTime);
        
        if (currentState == State.Patrolling) {
            Flip();
        }
        isWaiting = false;
    }

    private void OnDrawGizmosSelected() {
        if (groundCheck != null) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }

        Vector3 sightOrigin = (eyePoint != null) ? eyePoint.position : transform.position;
        Vector3 forward = (transform.localScale.x > 0) ? Vector3.right : Vector3.left;
        Vector3 upperLimit = Quaternion.Euler(0, 0, visionAngle / 2) * forward;
        Vector3 lowerLimit = Quaternion.Euler(0, 0, -visionAngle / 2) * forward;

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); 
        Gizmos.DrawRay(sightOrigin, upperLimit * detectionRadius);
        Gizmos.DrawRay(sightOrigin, lowerLimit * detectionRadius);
        Gizmos.DrawRay(sightOrigin, forward * detectionRadius);
        
        // Attack radius also originates from the eye now
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(sightOrigin, attackRadius);

        Gizmos.color = new Color(0f, 0f, 1f, 0.2f); 
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }
}