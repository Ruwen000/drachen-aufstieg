using UnityEngine;

public class BeamDamageDetector : MonoBehaviour
{
    public float laserLength = 100f;
    public LayerMask hitLayers;
    public GameObject BirdMeet; 
    public float damagePerSecond = 1f; 

    void Update()
    {
        Vector3 start = transform.position;
        Vector3 end = start + transform.forward * laserLength;

        if (Physics.Linecast(start, end, out RaycastHit hit, hitLayers))
        {
            Collider col = hit.collider;
            if (col == null) return;

            var boss = col.GetComponent<BossAI>();
            if (boss != null)
            {
                boss.TakeDamage(damagePerSecond * Time.deltaTime);
            }
            else if (col.CompareTag("Enemy"))
            {
                Vector3 spawnPosition = col.transform.position;
                Quaternion spawnRotation = col.transform.rotation;

                if (BirdMeet != null)
                {
                    Instantiate(BirdMeet, spawnPosition, spawnRotation);
                }

                Destroy(col.gameObject);
                Debug.Log("Vogel getroffen!");
            }
        }

        Debug.DrawLine(start, end, Color.red);
    }
}
