using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float wallCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;

    void Start() => rb = GetComponent<Rigidbody2D>();

    // Matches the Move (CallbackContext) in your screenshot
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Matches the Jump (CallbackContext) in your screenshot
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

void FixedUpdate()
{
    float horizontalMove = moveInput.x * moveSpeed;

    // Check for walls
    bool touchingWall = IsTouchingWall();

    // The Logic: If we are in the air AND touching a wall...
    if (!isGrounded && touchingWall)
    {
        // Check if we are trying to move TOWARDS the wall
        // (Assuming you're using the BoxCast logic from before)
        bool pushingIntoWall = (moveInput.x > 0 && IsWallToRight()) || (moveInput.x < 0 && IsWallToLeft());

        if (pushingIntoWall)
        {
            // Set horizontal velocity to 0 so the physics engine doesn't "grip"
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            // Allow them to move AWAY from the wall freely
            rb.linearVelocity = new Vector2(horizontalMove, rb.linearVelocity.y);
        }
    }
    else
    {
        // Normal ground or air movement
        rb.linearVelocity = new Vector2(horizontalMove, rb.linearVelocity.y);
    }
}

// Separate checks for left and right to see which way we are pushing
bool IsWallToRight() => Physics2D.Raycast(transform.position, Vector2.right, 0.6f, groundLayer);
bool IsWallToLeft() => Physics2D.Raycast(transform.position, Vector2.left, 0.6f, groundLayer);

bool IsTouchingWall()
{
    // Check right side
    RaycastHit2D hitRight = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.8f), 0, Vector2.right, wallCheckDistance, groundLayer);
    // Check left side
    RaycastHit2D hitLeft = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.8f), 0, Vector2.left, wallCheckDistance, groundLayer);
    
    return hitRight.collider != null || hitLeft.collider != null;
}

    void Update()
    {
        // Ground check circle
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
}