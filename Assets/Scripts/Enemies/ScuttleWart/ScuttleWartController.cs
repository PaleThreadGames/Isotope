using UnityEngine;

public class ScuttleWartController : MonoBehaviour
{
    public enum State { Patrolling, Chasing, Attacking, Retreating } 
    [Header("State")]
    public State currentState = State.Patrolling;

    [Header("Health Settings")]
    public float health = 3f;
    private bool isDead = false;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    
    [Header("Vision & Memory Settings")]
    public Transform eyePoint; 
    public float detectionRadius = 8f;
    public float attackRadius = 1.5f;
    public float hearingRadius = 3f; 
    [Range(0, 360)]
    public float visionAngle = 120f; 
    public float flipDeadzone = 0.5f; 
    public float memoryDuration = 1.5f; 
    public LayerMask visionBlockers; 
    
    private float timeSinceSawPlayer = 0f;

    [Header("Advanced AI & Physics")]
    public float retreatDistance = 2.5f; 
    public float knockbackForce = 18f;    
    
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

    [Header("Attack Settings")]
    public GameObject laserPrefab;
    public Transform firePoint;
    public float laserSpeed = 10f;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (rb.mass < 50) rb.mass = 100f; 
        if (moveDirection == 0) moveDirection = 1; 
        timeSinceSawPlayer = memoryDuration; 
    }

    void FixedUpdate() {
        if (isDead) return;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") || anim.IsInTransition(0))
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        Vector3 sightOrigin = (eyePoint != null) ? eyePoint.position : transform.position;
        float distanceToPlayer = Vector2.Distance(sightOrigin, player.position);
        
        bool hasClearLineOfSight = false;
        Vector2 dirToPlayer = (player.position - sightOrigin).normalized;
        RaycastHit2D hit = Physics2D.Raycast(sightOrigin, dirToPlayer, distanceToPlayer, visionBlockers);
        if (hit.collider == null) hasClearLineOfSight = true;

        Vector2 bugForward = (transform.localScale.x > 0) ? Vector2.right : Vector2.left;
        float angleToPlayer = Vector2.Angle(bugForward, dirToPlayer);

        bool inVisionCone = distanceToPlayer < detectionRadius && angleToPlayer < (visionAngle / 2f) && hasClearLineOfSight;
        bool inHearingRange = Vector2.Distance(transform.position, player.position) < hearingRadius;
        
        if (inVisionCone || inHearingRange) timeSinceSawPlayer = 0f;
        else timeSinceSawPlayer += Time.fixedDeltaTime;

        bool remembersPlayer = timeSinceSawPlayer < memoryDuration;

        if (remembersPlayer && distanceToPlayer < retreatDistance) {
            currentState = State.Retreating;
            isWaiting = false;
        }
        else if (distanceToPlayer < attackRadius && (inVisionCone || inHearingRange)) {
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
            case State.Chasing: MoveLogic(distanceToPlayer, false); break;
            case State.Retreating: MoveLogic(distanceToPlayer, true); break;
            case State.Attacking: StopAndAttack(); break;
        }
    }

    public void TakeDamage(float amount) {
        if (isDead) return;

        health -= amount;    

        if (health <= 0) {
            Die();
        }
    }

    void MoveLogic(float distToPlayer, bool isRetreating) {
        if (isWaiting) isWaiting = false; 

        float playerDirX = player.position.x - transform.position.x;
        int moveDir = (playerDirX > 0) ? 1 : -1;

        if (Mathf.Abs(playerDirX) > flipDeadzone) {
            HandleFlipping(moveDir);
        }

        if (isRetreating) moveDir *= -1; 

        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        
        if (isGroundAhead) {
            float speedMult = isRetreating ? 0.75f : 1f;
            rb.linearVelocity = new Vector2(moveDir * moveSpeed * speedMult, rb.linearVelocity.y);
            anim.SetFloat("Speed", 1f);
        } else {
            StopAndIdle();
        }
    }

    void Die() {
        isDead = true;
        anim.SetBool("isDead", isDead);
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;     
    }

    public void HandleDeathCleanup() {
        Destroy(gameObject);
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

    void StopAndAttack() {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetFloat("Speed", 0f);

        float playerDirX = player.position.x - transform.position.x;
        if (Mathf.Abs(playerDirX) > flipDeadzone) {
            HandleFlipping((playerDirX > 0) ? 1 : -1);
        }

        anim.SetTrigger("Attack");
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            // Sandbox Mode: Touch bug = Bug takes damage
            TakeDamage(1f);

            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null) {
                float dirX = (collision.transform.position.x > transform.position.x) ? 1f : -1f;
                Vector2 knockbackVector = new Vector2(dirX * 1.2f, 0.2f).normalized; 
                
                playerRb.linearVelocity = Vector2.zero; 
                playerRb.AddForce(knockbackVector * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    public void FireLaser() {
        if (laserPrefab == null || firePoint == null) return; 
        GameObject spawnedLaser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
        float facingDirection = (transform.localScale.x > 0) ? 1f : -1f;
        Rigidbody2D laserRb = spawnedLaser.GetComponent<Rigidbody2D>();
        if (laserRb != null) laserRb.linearVelocity = new Vector2(facingDirection * laserSpeed, 0);
        if (facingDirection < 0) {
            Vector3 laserScale = spawnedLaser.transform.localScale;
            laserScale.x *= -1;
            spawnedLaser.transform.localScale = laserScale;
        }
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
        if (currentState == State.Patrolling) Flip();
        isWaiting = false;
    }

    private void OnDrawGizmosSelected() {
        if (groundCheck != null) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }

        Vector3 sightOrigin = (eyePoint != null) ? eyePoint.position : transform.position;
        Vector3 forward = (transform.localScale.x > 0) ? Vector3.right : Vector3.left;
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(sightOrigin, retreatDistance);

        Vector3 upperLimit = Quaternion.Euler(0, 0, visionAngle / 2) * forward;
        Vector3 lowerLimit = Quaternion.Euler(0, 0, -visionAngle / 2) * forward;
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); 
        Gizmos.DrawRay(sightOrigin, upperLimit * detectionRadius);
        Gizmos.DrawRay(sightOrigin, lowerLimit * detectionRadius);
        
        if(player != null) {
            float d = Vector2.Distance(sightOrigin, player.position);
            RaycastHit2D hit = Physics2D.Raycast(sightOrigin, (player.position - sightOrigin).normalized, d, visionBlockers);
            Gizmos.color = (hit.collider == null) ? Color.green : Color.red;
            Gizmos.DrawLine(sightOrigin, player.position);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(sightOrigin, attackRadius);
    }
}