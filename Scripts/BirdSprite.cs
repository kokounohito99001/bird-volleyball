using UnityEngine;

// Placeholder asset script for Unity
// Attach this to any sprite or prefab that needs to be identified as a bird
public class BirdSprite : MonoBehaviour
{
    [Header("Bird Identification")]
    public bool isPlayerOne = true;
    
    void Start()
    {
        // Ensure the bird has the correct layer for collision detection
        gameObject.layer = LayerMask.NameToLayer("Bird");
        
        // Add tag if not already set
        if (!isPlayerOne)
        {
            gameObject.tag = "AI";
        }
    }
}
