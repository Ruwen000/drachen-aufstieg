using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 6f;
    public float damage = 2f;
    public bool useLifetime = true;
    private Vector3 direction;

    public void Initialize(Vector3 dir, float spd, float life)
    {
        direction = dir.normalized;
        speed = spd;
        lifetime = life;
        if (useLifetime) Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (direction.sqrMagnitude == 0f)
            transform.position += transform.forward * speed * Time.deltaTime;
        else
            transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else
        {
            // Optional: zerstören bei Hindernissen
        }
    }
}
