using UnityEngine;

public class CourtBoundary : MonoBehaviour
{
    [Header("Boundary Settings")]
    public float courtWidth = 16f;
    public float courtHeight = 9f;
    public float netHeight = 2f;
    public float netPositionX = 0f;
    
    [Header("Visual Settings")]
    public Color lineColor = Color.white;
    public float lineWidth = 0.1f;
    
    void OnDrawGizmos()
    {
        Vector3 center = transform.position;
        
        // Draw court boundaries
        Gizmos.color = lineColor;
        
        // Left boundary
        Vector3 leftBottom = center + new Vector3(-courtWidth / 2f, -courtHeight / 2f, 0);
        Vector3 leftTop = center + new Vector3(-courtWidth / 2f, courtHeight / 2f, 0);
        Gizmos.DrawLine(leftBottom, leftTop);
        
        // Right boundary
        Vector3 rightBottom = center + new Vector3(courtWidth / 2f, -courtHeight / 2f, 0);
        Vector3 rightTop = center + new Vector3(courtWidth / 2f, courtHeight / 2f, 0);
        Gizmos.DrawLine(rightBottom, rightTop);
        
        // Bottom boundary (ground)
        Gizmos.DrawLine(leftBottom, rightBottom);
        
        // Top boundary (optional, for visual reference)
        Gizmos.DrawLine(leftTop, rightTop);
        
        // Draw net
        Gizmos.color = Color.gray;
        Vector3 netBottom = center + new Vector3(netPositionX, -courtHeight / 2f, 0);
        Vector3 netTop = center + new Vector3(netPositionX, netHeight, 0);
        Gizmos.DrawLine(netBottom, netTop);
        
        // Draw service lines (middle of each side)
        Gizmos.color = lineColor;
        Vector3 leftServiceLine = center + new Vector3(-courtWidth / 4f, -courtHeight / 2f, 0);
        Vector3 rightServiceLine = center + new Vector3(courtWidth / 4f, -courtHeight / 2f, 0);
        Gizmos.DrawLine(leftServiceLine, leftServiceLine + Vector3.up * 0.5f);
        Gizmos.DrawLine(rightServiceLine, rightServiceLine + Vector3.up * 0.5f);
    }
}
