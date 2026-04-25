using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    
    [Header("Dash Settings")]
    public float dashForce = 24f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing;

    [Header("Detection Settings")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float wallCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private float originalGravity;

    void Start() 
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravity = rb.gravityScale;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isDashing) return; // Prevent jumping while dashing

        if (context.started && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // New Dash Input Method
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        
        // Use current move direction, or face forward if idle
        float dashDir = moveInput.x != 0 ? Mathf.Sign(moveInput.x) : transform.localScale.x;
        
        rb.gravityScale = 0f; // Stay level during dash
        rb.linearVelocity = new Vector2(dashDir * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void FixedUpdate()
    {
        if (isDashing) return; // Skip movement logic if dashing

        float horizontalMove = moveInput.x * moveSpeed;
        bool touchingWall = IsTouchingWall();

        if (!isGrounded && touchingWall)
        {
            bool pushingIntoWall = (moveInput.x > 0 && IsWallToRight()) || (moveInput.x < 0 && IsWallToLeft());

            if (pushingIntoWall)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(horizontalMove, rb.linearVelocity.y);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontalMove, rb.linearVelocity.y);
        }
    }

    bool IsWallToRight() => Physics2D.Raycast(transform.position, Vector2.right, 0.6f, groundLayer);
    bool IsWallToLeft() => Physics2D.Raycast(transform.position, Vector2.left, 0.6f, groundLayer);

    bool IsTouchingWall()
    {
        RaycastHit2D hitRight = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.8f), 0, Vector2.right, wallCheckDistance, groundLayer);
        RaycastHit2D hitLeft = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.8f), 0, Vector2.left, wallCheckDistance, groundLayer);
        return hitRight.collider != null || hitLeft.collider != null;
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
}