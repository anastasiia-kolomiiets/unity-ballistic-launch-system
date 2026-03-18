using UnityEngine;

[System.Serializable]
public struct BallisticAngles
{
    public bool success;
    public float yaw;
    public float pitch;
}

public class Launcher : MonoBehaviour
{
    [Header("References")]
    public Transform barrelPivot;
    public Transform firePoint;
    public Transform target;
    public GameObject projectilePrefab;

    [Header("Settings")]
    public float launchSpeed = 25f;

    public BallisticAngles Fire()
    {
        Vector3 start = firePoint.position;
        Vector3 end = target.position;

        BallisticAngles result = new BallisticAngles { success = false, yaw = 0f, pitch = 0f };

        if (!BallisticCalculator.SolveBallisticArc(
            start,
            end,
            launchSpeed,
            Physics.gravity.magnitude,
            out float yaw,
            out float pitch))
        {
            Debug.Log("Target is out of reach");
            return result;
        }

        // Horizontal rotation of the launcher
        transform.rotation = Quaternion.Euler(0, yaw, 0); 

        // Vertical rotation of the barrel
        barrelPivot.localRotation = Quaternion.Euler(-pitch, 0, 0);

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        Vector3 velocity = firePoint.forward * launchSpeed;
        rb.linearVelocity = velocity;

        result.success = true;
        result.yaw = yaw;
        result.pitch = pitch;
        return result;
    }

    public BallisticAngles FireFromUI(Vector3 launcherPos, Vector3 targetPos, float speed)
    {
        transform.position = launcherPos;
        target.position = targetPos;
        launchSpeed = speed;

        return Fire();
    }

    // Shooting direction visualisation
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
