using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Ball Settings")]
    public float gravityScale = 2.5f;
    public float maxSpeed = 20f;
    public float minSpeed = 5f;
    
    [Header("Collision Settings")]
    public LayerMask birdLayer;
    public LayerMask groundLayer;
    public LayerMask netLayer;
    
    private Rigidbody2D rb;
    private Vector2 lastPosition;
    private int touchesOnSide;
    private bool isOnPlayerOneSide;
    private int lastTouchingPlayer; // 1 or 2
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }
    
    void Start()
    {
        ResetBall(1);
    }
    
    void Update()
    {
        // Clamp velocity
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
        
        // Track which side the ball is on
        float currentX = transform.position.x;
        bool currentlyOnPlayerOneSide = currentX < 0;
        
        if (currentlyOnPlayerOneSide != isOnPlayerOneSide)
        {
            isOnPlayerOneSide = currentlyOnPlayerOneSide;
            touchesOnSide = 0;
        }
        
        // Check for ground collision manually
        if (transform.position.y <= -4f) // Ground level
        {
            HandleGroundHit();
        }
    }
    
    void HandleGroundHit()
    {
        string hittingSide = isOnPlayerOneSide ? "PlayerTwo" : "PlayerOne";
        GameManager.Instance?.ScorePoint(hittingSide);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & birdLayer) != 0)
        {
            BirdController bird = collision.gameObject.GetComponent<BirdController>();
            
            if (bird != null)
            {
                int birdPlayer = bird.isPlayerOne ? 1 : 2;
                
                // Check if same team already touched
                if (lastTouchingPlayer == birdPlayer)
                {
                    return; // Can't touch twice
                }
                
                lastTouchingPlayer = birdPlayer;
                
                // Calculate hit direction based on bird position and velocity
                Vector2 hitDirection = (transform.position - (Vector2)bird.transform.position).normalized;
                
                // Add some upward force
                hitDirection.y = Mathf.Abs(hitDirection.y) + 0.5f;
                hitDirection.Normalize();
                
                // Apply force based on bird's action
                float hitForce = 10f;
                
                if (bird.HasTouchedBall())
                {
                    // This was an active hit (low pass or smash)
                    if (collision.relativeVelocity.magnitude > 5f)
                    {
                        hitForce = 15f; // Smash
                    }
                    else
                    {
                        hitForce = 8f; // Low pass
                    }
                }
                
                rb.velocity = hitDirection * hitForce;
                
                // Play sound effect here
                AudioManager.Instance?.PlaySound("BallHit");
            }
        }
        
        if (((1 << collision.gameObject.layer) & netLayer) != 0)
        {
            // Ball hit the net
            AudioManager.Instance?.PlaySound("NetHit");
        }
    }
    
    public void ResetBall(int servingPlayer)
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        
        // Position ball above serving player
        float xPos = servingPlayer == 1 ? -3f : 3f;
        transform.position = new Vector2(xPos, 2f);
        
        lastTouchingPlayer = 0;
        touchesOnSide = 0;
        isOnPlayerOneSide = servingPlayer == 1;
        
        // Start countdown
        GameManager.Instance?.StartCountdown(servingPlayer);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
