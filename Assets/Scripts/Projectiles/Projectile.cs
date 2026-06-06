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
    [Tooltip("All projectile physics parameters including air resistance, mass, and drag.")]
    public AirResistanceSettings airResistanceSettings;

    [Header("Trail Settings")]
    [Tooltip("How many seconds the smoke trail should remain visible and fade after impact")]
    public float trailCleanupDelay = 4.0f;

    private Rigidbody rb;
    private bool hasHit = false; // Prevents multiple explosion triggers
    private Transform trailObject;  // Reference to SmokeTrail child

    private void Awake()
    {
        // Cache rigidbody reference early
        rb = GetComponent<Rigidbody>();

        // Find child object with smoke trail
        trailObject = transform.Find("SmokeTrail");

        if (trailObject == null)
        {
            Debug.LogWarning("SmokeTrail child object not found on projectile!", gameObject);
        }
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
        if (!airResistanceSettings.useAirDrag || rb == null) return;

        // Disable Unity's built-in gravity – we apply it manually to combine with drag
        rb.useGravity = false;

        // Remove linear/angular damping – we calculate drag explicitly
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        // Apply custom mass (important when drag is enabled)
        rb.mass = airResistanceSettings.mass;
    }

    private void FixedUpdate()
    {
        // Skip physics if drag is off, already hit, or rigidbody missing
        if (!airResistanceSettings.useAirDrag || hasHit || rb == null) return;

        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;

        // Avoid calculations at near-zero speed (prevents NaN or division issues)
        if (speed > 0.05f)
        {
            // Quadratic drag force: Fd = ½ ρ Cd A v² (direction opposite to velocity)
            float dragMagnitude = 0.5f * airResistanceSettings.airDensity * airResistanceSettings.dragCoefficient * airResistanceSettings.crossSectionArea * speed * speed;
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
    /// Instantiates explosion effect (if assigned) and detaches the smoke trail before destroying the projectile.
    /// Called on first collision.
    /// </summary>
    private void Explode()
    {
        // if (explosionPrefab != null)
        // {
        //     // Spawn explosion at current position with default rotation
        //     Instantiate(
        //         explosionPrefab,
        //         transform.position,
        //         Quaternion.identity
        //     );
        // }

        // Detach and fade out the smoke trail
        DetachAndFadeTrail();

        // Remove projectile from scene
        Destroy(gameObject);
    }

    /// <summary>
    /// Detaches the SmokeTrail child object from the projectile,
    /// stops emitting new trail points, and destroys the trail after a delay.
    /// This allows the trail to remain visible in the air and fade naturally.
    /// </summary>
    private void DetachAndFadeTrail()
    {
        if (trailObject == null) return;

        // Detach the trail from the projectile so it stays in the world
        trailObject.SetParent(null);

        // Stop generating new trail points
        TrailRenderer trailRenderer = trailObject.GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        // Destroy the trail object after the specified delay
        Destroy(trailObject.gameObject, trailCleanupDelay);
    }
}