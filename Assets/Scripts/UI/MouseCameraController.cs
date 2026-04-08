using UnityEngine;

/// <summary>
/// Advanced mouse camera controller designed for ballistic trajectory simulators.
/// Provides intuitive controls similar to modern physics simulation tools:
/// - Left mouse drag  → Orbit (rotate camera around focus point)
/// - Right mouse drag → Pan (move the focus point in the camera's plane)
/// - Mouse wheel      → Zoom in/out toward the mouse cursor position
/// </summary>
public class MouseCameraController : MonoBehaviour
{
    [Header("Orbit Settings")]
    [Tooltip("Sensitivity of camera rotation when dragging with left mouse button.")]
    public float orbitSpeed = 1f;

    [Tooltip("Minimum vertical angle (pitch) the camera can reach.")]
    public float minVerticalAngle = -85f;

    [Tooltip("Maximum vertical angle (pitch) the camera can reach.")]
    public float maxVerticalAngle = 85f;

    [Header("Pan Settings")]
    [Tooltip("Sensitivity of camera panning when dragging with right mouse button.")]
    public float panSpeed = 0.8f;

    [Header("Zoom Settings")]
    [Tooltip("How fast the camera zooms when using the mouse wheel.")]
    public float zoomSpeed = 1f;

    [Tooltip("Minimum allowed distance from the focus point.")]
    public float minDistance = 2f;

    [Tooltip("Maximum allowed distance from the focus point.")]
    public float maxDistance = 30f;

    [Header("Focus Point")]
    [Tooltip("The central point the camera orbits around. " +
             "This is the most important parameter - the camera always looks at this point.")]
    public Vector3 focusPoint = new Vector3(0f, 5f, 10f);

    // Internal camera state
    private float distance = 30f;     // Current distance from focusPoint to camera
    private float yaw = 0f;           // Horizontal rotation angle
    private float pitch = 20f;        // Vertical rotation angle

    private Vector3 lastMousePos;
    private bool isOrbiting = false;
    private bool isPanning = false;

    private Camera cam;

    /// <summary>
    /// Initializes the camera with default values and sets initial position and rotation.
    /// </summary>
    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        // Apply initial position and rotation
        UpdateCameraPositionAndRotation();
    }

    /// <summary>
    /// Called every frame after all other updates. Handles input and updates camera transform.
    /// Using LateUpdate ensures the camera moves smoothly after any projectiles or objects update.
    /// </summary>
    private void LateUpdate()
    {
        HandleMouseInput();
        UpdateCameraPositionAndRotation();
    }

    /// <summary>
    /// Processes all mouse input: orbit (left drag), pan (right drag), and zoom (scroll wheel).
    /// </summary>
    private void HandleMouseInput()
    {
        // === LEFT MOUSE BUTTON - ORBIT ===
        if (Input.GetMouseButtonDown(0))
        {
            isOrbiting = true;
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
            isOrbiting = false;

        if (isOrbiting)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;

            yaw += delta.x * orbitSpeed * 0.1f;
            pitch -= delta.y * orbitSpeed * 0.1f;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

            lastMousePos = Input.mousePosition;
        }

        // === RIGHT MOUSE BUTTON - PAN ===
        if (Input.GetMouseButtonDown(1))
        {
            isPanning = true;
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
            isPanning = false;

        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;

            // Move focus point in camera's local right and up directions
            focusPoint -= transform.right * (delta.x * panSpeed * 0.05f);
            focusPoint -= transform.up * (delta.y * panSpeed * 0.05f);

            lastMousePos = Input.mousePosition;
        }

        // === MOUSE SCROLL WHEEL - ZOOM ===
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            // Zoom toward the point under the mouse cursor
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            float zoomAmount = scroll * zoomSpeed * distance * 0.1f;

            // Move focus point along the ray direction
            focusPoint += ray.direction * zoomAmount;

            // Update distance (zoom in/out)
            distance = Mathf.Clamp(distance - zoomAmount * 1.8f, minDistance, maxDistance);
        }
    }

    /// <summary>
    /// Calculates and applies the new camera position and rotation based on 
    /// current yaw, pitch, distance and focusPoint.
    /// </summary>
    private void UpdateCameraPositionAndRotation()
    {
        // Create rotation from pitch and yaw
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Calculate offset vector behind the focus point
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

        // Set camera position and make it look at the focus point
        transform.position = focusPoint + offset;
        transform.LookAt(focusPoint);
    }

    /// <summary>
    /// Changes the point around which the camera orbits.
    /// Useful for focusing on different parts of the scene (e.g. midpoint between start and target).
    /// </summary>
    /// <param name="newFocus">New focus point in world space.</param>
    public void SetFocusPoint(Vector3 newFocus)
    {
        focusPoint = newFocus;
    }

    /// <summary>
    /// Resets the camera to its initial position, rotation, and focus point.
    /// </summary>
    public void ResetCamera()
    {
        focusPoint = new Vector3(0f, 5f, 10f);
        yaw = 0f;
        pitch = 20f;
        distance = 30f;
    }
}