using System.Configuration.Assemblies;
using UnityEngine;

public class Drone : MonoBehaviour
{
    [Header("Drone Settings")]
    [Tooltip("Speed of the drone in m/s")]
    public float flightSpeed = 15f;

    [Tooltip("How close to the target (XZ plane) to consider it reached")]
    public float arrivalThreshold = 0.3f;

    [Tooltip("Exact spawn point and initial direction of the projectile")]
    public Transform firePoint;

    [Tooltip("Prefab of the projectile to instantiate")]
    public GameObject projectilePrefab;

    private Vector3 targetPosition;
    private bool isFlying = false;
    private bool hasDropped = false;
    private float flightStartTime;
    public AirResistanceSettings airResistanceSettings;

    // Results of calculation from BallisticCalculator.cs
    public ReleaseResult releaseResult { get; private set; }

    /// <summary>
    /// Starts flight from startPos towards targetPos at given speed.
    /// Drone maintains constant height (Y from startPos).
    /// </summary>
    public void StartFlight(Vector3 startPos, Vector3 targetPos, float speed, AirResistanceSettings airSettings)
    {
        transform.position = startPos;
        flightSpeed = Mathf.Max(speed, 0.1f); // prevent zero/negative speed

        // Target position at the same height as start
        targetPosition = new Vector3(targetPos.x, startPos.y, targetPos.z);
        
        // Setting air resistance settings for later use
        airResistanceSettings = airSettings;

        // Setting flight start time for later projectile release
        flightStartTime = Time.time;

        releaseResult = BallisticCalculator.CalculateDroneDrop(startPos, targetPos, flightSpeed, airResistanceSettings);

        if (!releaseResult.success)
        {
            Debug.LogWarning("Target is out of reach with current parameters");
            return;
        }

        // Rotate drone to face target
        Vector3 direction = targetPosition - startPos;
        direction.y = 0f;   // only horizontal rotation

        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction);

        isFlying = true;
        hasDropped = false;

        Debug.Log($"Drone started flight to {targetPosition} at {flightSpeed:F1} m/s");
    }

    private void Update()
    {
        if (!isFlying) return;

        // Move towards target using MoveTowards (very smooth and simple)
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            flightSpeed * Time.deltaTime
        );

        // Releasing projectile at the right time
        if (releaseResult.success && !hasDropped && (Time.time - flightStartTime) >= releaseResult.timeToRelease)
        {
            ReleaseProjectile();
        }

        // Check if we reached the target horizontally
        float distanceToTarget = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(targetPosition.x, 0, targetPosition.z)
        );

        if (distanceToTarget <= arrivalThreshold)
        {
            StopFlight();
        }
    }

    private void ReleaseProjectile()
    {
        if (projectilePrefab == null) 
        {
            Debug.LogWarning("Projectile prefab is not assigned!");
            return;
        }

        hasDropped = true;

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        if (projectileRb != null)
        {
            // The payload receives the drone's current horizontal speed
            Vector3 dropVelocity = transform.forward * flightSpeed;
            projectileRb.linearVelocity = dropVelocity;
        }

        // Pass the resistance settings (if the load has a Projectile component)
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.airResistanceSettings = airResistanceSettings;
            proj.SetupRigidbody();
        }

        Debug.Log($"Projectile dropped at {transform.position:F2} after {Time.time - flightStartTime:F2} seconds");
    }

    private void StopFlight()
    {
        isFlying = false;
        transform.position = targetPosition; // snap exactly to target

        Debug.Log("Drone reached the target and stopped.");
    }
}
