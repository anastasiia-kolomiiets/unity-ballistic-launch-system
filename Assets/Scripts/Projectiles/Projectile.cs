using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject explosionPrefab;

    public bool useAirDrag = false;
    public float dragCoefficient = 0.47f;
    public float crossSectionArea = 0.012f;
    public float airDensity = 1.225f;
    public float mass = 3.5f;

    private Rigidbody rb;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        if (useAirDrag)
        {
            rb.useGravity = false;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.mass = mass;
        }
    }

    void Start()
    {
        Destroy(gameObject, 20f);
    }

    private void FixedUpdate()
    {
        if (!useAirDrag || hasHit || rb == null) return;

        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;

        if (speed > 0.05f)
        {
            float dragMagnitude = 0.5f * airDensity * dragCoefficient * crossSectionArea * speed * speed;
            Vector3 dragForce = -velocity.normalized * dragMagnitude;
            Vector3 gravityForce = Physics.gravity * rb.mass;
            Vector3 totalForce = dragForce + gravityForce;
            rb.AddForce(totalForce, ForceMode.Force);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        if (collision.gameObject.CompareTag("Target"))
        {
            Debug.Log("Projectile hit target");
        }

        Explode();
    }

    void Explode()
    {
        if (explosionPrefab != null)
        {
            Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}
