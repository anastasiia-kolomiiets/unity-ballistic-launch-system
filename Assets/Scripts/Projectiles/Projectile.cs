using UnityEngine;

/// <summary>
/// Projectile behavior with optional quadratic air drag simulation.
/// Handles custom gravity + drag forces when air resistance is enabled,
/// collision detection, explosion on impact and automatic destruction.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Effects")]
    [Tooltip("Prefab to instantiate on impact (explosion particle system, etc.)")]
    public GameObject explosionPrefab;

    [Header("Physics Parameters")]
    [Tooltip("Enable realistic air drag simulation (requires custom force application)")]
    public bool useAirDrag = false;

    [Tooltip("Drag coefficient Cd – typical values: 0.47 (sphere), 0.8–1.0 (cylinder/projectile)")]
    public float dragCoefficient = 0.47f;

    [Tooltip("Cross-sectional area of the projectile (m²) – affects drag force")]
    public float crossSectionArea = 0.012f;

    [Tooltip("Air density (kg/m³) – standard sea level value is 1.225")]
    public float airDensity = 1.225f;

    [Tooltip("Mass of the projectile (kg) – used in force calculations")]
    public float mass = 3.5f;

    private Rigidbody rb;
    private bool hasHit = false; // Prevents multiple explosion triggers

    private void Awake()
    {
        // Cache rigidbody reference early
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Safety timeout: destroy projectile after 20 seconds if it doesn't hit anything
        // Prevents accumulation of projectiles in long flights or misses
        Destroy(gameObject, 20f);
    }

    /// <summary>
    /// Configures the Rigidbody for drag simulation mode.
    /// Disables built-in gravity and damping to take full control via custom forces.
    /// Must be called right after instantiation (from Launcher).
    /// </summary>
    public void SetupRigidbody()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        // Skip if drag is disabled or rigidbody is missing
        if (!useAirDrag || rb == null) return;

        // Disable Unity's built-in gravity – we apply it manually to combine with drag
        rb.useGravity = false;

        // Remove linear/angular damping – we calculate drag explicitly
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        // Apply custom mass (important when drag is enabled)
        rb.mass = mass;
    }

    private void FixedUpdate()
    {
        // Skip physics if drag is off, already hit, or rigidbody missing
        if (!useAirDrag || hasHit || rb == null) return;

        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;

        // Avoid calculations at near-zero speed (prevents NaN or division issues)
        if (speed > 0.05f)
        {
            // Quadratic drag force: Fd = ½ ρ Cd A v² (direction opposite to velocity)
            float dragMagnitude = 0.5f * airDensity * dragCoefficient * crossSectionArea * speed * speed;
            Vector3 dragForce = -velocity.normalized * dragMagnitude;

            // Manual gravity force: Fg = m * g
            Vector3 gravityForce = Physics.gravity * rb.mass;

            // Combine both forces and apply in one step
            Vector3 totalForce = dragForce + gravityForce;
            rb.AddForce(totalForce, ForceMode.Force);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Prevent multiple triggers (e.g. bouncing or multiple contact points)
        if (hasHit) return;
        hasHit = true;

        // Optional: log hit on specific tagged object (e.g. for scoring or feedback)
        if (collision.gameObject.CompareTag("Target"))
        {
            Debug.Log("Projectile hit target");
        }

        Explode();
    }

    /// <summary>
    /// Instantiates explosion effect (if assigned) and destroys the projectile.
    /// Called on first collision.
    /// </summary>
    private void Explode()
    {
        if (explosionPrefab != null)
        {
            // Spawn explosion at current position with default rotation
            Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        // Remove projectile from scene
        Destroy(gameObject);
    }
}