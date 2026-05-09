using UnityEngine;

/// <summary>
/// Controls the launcher object: aims it using calculated ballistic angles,
/// spawns projectiles and applies physics settings (drag, mass, gravity mode).
/// </summary>
public class Launcher : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The part of the launcher that rotates vertically (barrel)")]
    public Transform barrelPivot;

    [Tooltip("Exact spawn point and initial direction of the projectile")]
    public Transform firePoint;

    [Tooltip("Prefab of the projectile to instantiate")]
    public GameObject projectilePrefab;

    [Header("Settings")]
    [Tooltip("Initial speed of the projectile (m/s)")]
    public float launchSpeed = 25f;

    /// <summary>
    /// Reference to the most recently spawned projectile (useful for debugging or further control)
    /// </summary>
    public Projectile lastProjectile { get; private set; }

    /// <summary>
    /// Main fire method. Calculates required angles, rotates launcher & barrel,
    /// spawns projectile and sets its physics properties.
    /// </summary>
    /// <param name="end">Coordinates of the target</param>
    /// <param name="airResistanceSettings">All projectile physics parameters including air resistance, mass, and drag.</param>
    /// <returns></returns>
    public BallisticResult Fire(Vector3 end, AirResistanceSettings airResistanceSettings)
    {
        Vector3 start = firePoint.position;

        BallisticResult result = BallisticCalculator.CalculateLaunchAngles(start, end, launchSpeed, airResistanceSettings);;

        if (!result.success)
        {
            Debug.LogWarning("Target is out of reach with current parameters");
            return result;
        }

        // Apply horizontal rotation (yaw) to the whole launcher object
        transform.rotation = Quaternion.Euler(0, result.yaw, 0);

        // Apply vertical elevation (pitch) only to the barrel pivot
        // Note: negative sign because Unity's local X rotation is inverted for pitch up
        barrelPivot.localRotation = Quaternion.Euler(-result.pitch, 0, 0);

        // Spawn projectile at fire point with current rotation
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        lastProjectile = projectile.GetComponent<Projectile>();

        if (lastProjectile == null)
        {
            Debug.LogError("Projectile prefab is missing Projectile component!");
            return result;
        }

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Projectile prefab is missing Rigidbody component!");
            return result;
        }

        // Set initial velocity in the direction the barrel is facing
        Vector3 velocity = firePoint.forward * launchSpeed;
        rb.linearVelocity = velocity;

        // Important: apply drag & mass settings immediately after spawn
        // to prevent wrong gravity behavior in the first physics steps
        ApplySettingsToLastProjectile(airResistanceSettings);

        return result;
    }

    /// <summary>
    /// Convenience method called from UI.
    /// Updates launcher position, target position and muzzle velocity before firing.
    /// </summary>
    /// <returns>Result of the Fire() call</returns>
    public BallisticResult FireFromUI(Vector3 launcherPos, Vector3 targetPos, float speed, AirResistanceSettings airResistanceSettings)
    {
        // Update launcher and target transforms for this shot
        transform.position = launcherPos;
        launchSpeed = speed;

        return Fire(targetPos, airResistanceSettings);
    }

    /// <summary>
    /// Transfers drag, mass and physics mode settings to the last spawned projectile.
    /// Must be called right after Instantiate to avoid gravity duplication issues.
    /// </summary>
    public void ApplySettingsToLastProjectile(AirResistanceSettings airResistanceSettings)
    {
        if (lastProjectile == null) return;

        lastProjectile.airResistanceSettings = airResistanceSettings;

        // Apply rigidbody configuration immediately
        lastProjectile.SetupRigidbody();
    }

    /// <summary>
    /// Draws a red line in Scene view showing current shooting direction (debug helper).
    /// </summary>
    void OnDrawGizmos()
    {
        if (firePoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            firePoint.position,
            firePoint.position + firePoint.forward * 5f
        );
    }
}