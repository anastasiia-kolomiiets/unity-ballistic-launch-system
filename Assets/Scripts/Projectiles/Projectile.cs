using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject explosionPrefab;

    void Start()
    {
        Destroy(gameObject, 10f);
    }

    void OnCollisionEnter(Collision collision)
    {
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
