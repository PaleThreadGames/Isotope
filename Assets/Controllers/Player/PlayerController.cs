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
    // If we're in the air and hugging a wall, kill horizontal velocity 
    // This stops the "friction" from sticking you to the wall
    if (!isGrounded && IsTouchingWall())
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    else
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }
}

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
        Debug.Log("Is Grounded: " + isGrounded + " Is Touching Wall: " + IsTouchingWall());
    }
}