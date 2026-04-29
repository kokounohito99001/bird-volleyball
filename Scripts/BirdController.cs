using UnityEngine;

public class BirdController : MonoBehaviour
{
    [Header("Bird Settings")]
    public bool isPlayerOne = true;
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float lowPassForce = 8f;
    public float smashForce = 15f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool hasTouchedBall;
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        HandleMovement();
        HandleActions();
        
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VelocityY", rb.velocity.y);
        }
    }
    
    void HandleMovement()
    {
        float move = 0f;
        
        if (isPlayerOne)
        {
            if (ControlsManager.Instance != null)
            {
                if (ControlsManager.Instance.IsMoveLeft()) move = -1f;
                if (ControlsManager.Instance.IsMoveRight()) move = 1f;
            }
            else
            {
                move = Input.GetAxis("Horizontal");
            }
        }
        else
        {
            // AI control will be handled separately
            return;
        }
        
        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
        
        if (move != 0)
        {
            spriteRenderer.flipX = move < 0;
        }
    }
    
    void HandleActions()
    {
        bool lowPassPressed = false;
        bool smashPressed = false;
        
        if (ControlsManager.Instance != null)
        {
            lowPassPressed = ControlsManager.Instance.IsLowPassPressed();
            smashPressed = ControlsManager.Instance.IsSmashPressed();
        }
        else
        {
            lowPassPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
            smashPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1);
        }
        
        if (lowPassPressed && isGrounded && !hasTouchedBall)
        {
            PerformLowPass();
        }
        
        if (smashPressed && !isGrounded && !hasTouchedBall)
        {
            PerformSmash();
        }
    }
    
    void PerformLowPass()
    {
        rb.velocity = new Vector2(rb.velocity.x, lowPassForce);
        hasTouchedBall = true;
        
        if (animator != null)
        {
            animator.SetTrigger("LowPass");
        }
    }
    
    void PerformSmash()
    {
        rb.velocity = new Vector2(rb.velocity.x, smashForce);
        hasTouchedBall = true;
        
        if (animator != null)
        {
            animator.SetTrigger("Smash");
        }
    }
    
    public void ResetTouch()
    {
        hasTouchedBall = false;
    }
    
    public bool HasTouchedBall()
    {
        return hasTouchedBall;
    }
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
