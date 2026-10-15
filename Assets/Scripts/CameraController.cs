using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player; 
    [SerializeField] private float smoothTime = 0.3f; 
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); 

    [Header("Zoom Settings")]
    [Tooltip("Lower values zoom in, higher values zoom out (Default is 5)")]
    [SerializeField] private float targetZoom = 5f; 

    private Vector3 velocity = Vector3.zero;
    private Camera cam; // Reference to the Camera component

    private void Start()
    {
        // Grab the Camera component attached to this GameObject
        cam = GetComponent<Camera>();
        
        // Immediately set the zoom level
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = targetZoom;
        }
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 targetPosition = player.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            
            // Keeps the zoom updated if you change it during play mode
            if (cam != null) cam.orthographicSize = targetZoom; 
        }
    }
}