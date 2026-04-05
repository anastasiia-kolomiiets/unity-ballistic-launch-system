using UnityEngine;

public class Drone : MonoBehaviour
{
    [Header("Drone Settings")]
    [Tooltip("Speed of the drone in m/s")]
    public float flightSpeed = 15f;

    [Tooltip("How close to the target (XZ plane) to consider it reached")]
    public float arrivalThreshold = 0.3f;

    private Vector3 targetPosition;
    private bool isFlying = false;

    /// <summary>
    /// Starts flight from startPos towards targetPos at given speed.
    /// Drone maintains constant height (Y from startPos).
    /// </summary>
    public void StartFlight(Vector3 startPos, Vector3 targetPos, float speed)
    {
        transform.position = startPos;
        flightSpeed = Mathf.Max(speed, 0.1f); // prevent zero/negative speed

        // Target position at the same height as start
        targetPosition = new Vector3(targetPos.x, startPos.y, targetPos.z);

        // Rotate drone to face target
        Vector3 direction = targetPosition - startPos;
        direction.y = 0f;   // only horizontal rotation

        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction);

        isFlying = true;

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

    private void StopFlight()
    {
        isFlying = false;
        transform.position = targetPosition; // snap exactly to target

        Debug.Log("Drone reached the target and stopped.");
    }
}
