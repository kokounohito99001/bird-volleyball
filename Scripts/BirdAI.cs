using UnityEngine;

public class BirdAI : MonoBehaviour
{
    [Header("AI Settings")]
    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty difficulty = Difficulty.Medium;
    
    [Header("Movement Settings")]
    public float reactionDelay = 0.3f;
    public float predictionAccuracy = 0.8f;
    public float movementSmoothing = 5f;
    
    private BirdController birdController;
    private Rigidbody2D rb;
    private Transform ballTransform;
    private float targetXPosition;
    private float reactionTimer;
    private bool isReadyToReact;
    private Vector2 lastBallPosition;
    
    void Awake()
    {
        birdController = GetComponent<BirdController>();
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            ballTransform = ball.transform;
        }
        
        SetDifficulty();
    }
    
    void SetDifficulty()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                reactionDelay = 0.5f;
                predictionAccuracy = 0.5f;
                movementSmoothing = 3f;
                break;
            case Difficulty.Medium:
                reactionDelay = 0.3f;
                predictionAccuracy = 0.7f;
                movementSmoothing = 5f;
                break;
            case Difficulty.Hard:
                reactionDelay = 0.15f;
                predictionAccuracy = 0.9f;
                movementSmoothing = 8f;
                break;
        }
    }
    
    void Update()
    {
        if (ballTransform == null) return;
        
        // Only react when ball is on AI's side (right side)
        if (ballTransform.position.x < 0)
        {
            return;
        }
        
        // Reaction delay
        if (!isReadyToReact)
        {
            reactionTimer += Time.deltaTime;
            if (reactionTimer >= reactionDelay)
            {
                isReadyToReact = true;
            }
            return;
        }
        
        // Predict where ball will land
        PredictBallLanding();
        
        // Move towards predicted position
        MoveToTarget();
        
        // Decide whether to jump/hit
        DecideAction();
    }
    
    void PredictBallLanding()
    {
        Vector2 ballVelocity = ballTransform.GetComponent<Rigidbody2D>().velocity;
        
        // Simple parabolic prediction
        float timeToLand = 0f;
        
        if (ballVelocity.y < 0)
        {
            // Ball is falling
            float height = ballTransform.position.y - (-4f); // Ground at y=-4
            timeToLand = Mathf.Sqrt(2 * height / 9.8f);
        }
        
        float predictedX = ballTransform.position.x + ballVelocity.x * timeToLand * predictionAccuracy;
        
        // Clamp to court boundaries
        predictedX = Mathf.Clamp(predictedX, 1f, 7f);
        
        targetXPosition = predictedX;
    }
    
    void MoveToTarget()
    {
        float currentX = transform.position.x;
        float direction = Mathf.Sign(targetXPosition - currentX);
        
        rb.velocity = new Vector2(direction * birdController.moveSpeed, rb.velocity.y);
        
        // Flip sprite
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && direction < 0)
        {
            sr.flipX = true;
        }
        else if (sr != null && direction > 0)
        {
            sr.flipX = false;
        }
    }
    
    void DecideAction()
    {
        if (birdController == null) return;
        
        float distanceToBall = Vector2.Distance(transform.position, ballTransform.position);
        
        // Jump and smash if ball is high and close
        if (distanceToBall < 2f && ballTransform.position.y > transform.position.y + 1f && 
            ballTransform.GetComponent<Rigidbody2D>().velocity.y < 0)
        {
            // Simulate smash input
            if (!birdController.HasTouchedBall())
            {
                birdController.PerformSmash();
            }
        }
        // Low pass if ball is low
        else if (distanceToBall < 1.5f && ballTransform.position.y < transform.position.y + 0.5f)
        {
            if (!birdController.HasTouchedBall())
            {
                birdController.PerformLowPass();
            }
        }
    }
    
    public void ResetAI()
    {
        reactionTimer = 0f;
        isReadyToReact = false;
    }
}
