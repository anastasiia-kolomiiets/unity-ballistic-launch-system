using UnityEngine;

public class Launcher : MonoBehaviour
{
    [Header("References")]
    public Transform barrelPivot;
    public Transform firePoint;
    public Transform target;
    public GameObject projectilePrefab;

    [Header("Settings")]
    public float launchSpeed = 25f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    public void Fire()
    {
        Vector3 start = firePoint.position;
        Vector3 end = target.position;

        if (!BallisticCalculator.SolveBallisticArc(
            start,
            end,
            launchSpeed,
            Physics.gravity.magnitude,
            out float yaw,
            out float pitch))
        {
            Debug.Log("Target is out of reach");
            return;
        }

        // Horizontal rotation of the launcher
        transform.rotation = Quaternion.Euler(0, yaw, 0); 

        // Vertical rotation of the barrel
        barrelPivot.localRotation = Quaternion.Euler(-pitch, 0, 0);

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        Vector3 velocity = firePoint.forward * launchSpeed;
        rb.linearVelocity = velocity;
    }

    public void FireFromUI(Vector3 launcherPos, Vector3 targetPos, float speed)
    {
        transform.position = launcherPos;
        target.position = targetPos;

        launchSpeed = speed;

        Fire();
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
