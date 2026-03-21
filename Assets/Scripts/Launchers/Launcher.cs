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

    public Projectile lastProjectile { get; private set; }

    public BallisticResult Fire()
    {
        Vector3 start = firePoint.position;
        Vector3 end = target.position;

        BallisticResult result = BallisticCalculator.SolveBallisticArc(start, end, launchSpeed, Physics.gravity.magnitude);

        if (!result.success)
        {
            Debug.Log("Target is out of reach");
            return result;
        }

        // Horizontal rotation of the launcher
        transform.rotation = Quaternion.Euler(0, result.yaw, 0); 

        // Vertical rotation of the barrel
        barrelPivot.localRotation = Quaternion.Euler(-result.pitch, 0, 0);

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        lastProjectile = projectile.GetComponent<Projectile>();
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        Vector3 velocity = firePoint.forward * launchSpeed;
        rb.linearVelocity = velocity;

        return result;
    }

    public BallisticResult FireFromUI(Vector3 launcherPos, Vector3 targetPos, float speed)
    {
        transform.position = launcherPos;
        target.position = targetPos;
        launchSpeed = speed;

        return Fire();
    }

    public void ApplySettingsToLastProjectile(bool useDrag, float cd, float massVal, float area)
    {
        if (lastProjectile == null) return;

        lastProjectile.useAirDrag = useDrag;
        lastProjectile.dragCoefficient = cd;
        lastProjectile.mass = massVal;
        lastProjectile.crossSectionArea = area;
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
