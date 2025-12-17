using UnityEngine;

public class DetectFireBeamHit : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float laserLength = 20f;
    public LayerMask hitLayers;
    public GameObject BirdMeet;

    void Update()
    {
        Vector3 start = transform.position;
        Vector3 end = start + transform.forward * laserLength;

        if (Physics.Linecast(start, end, out RaycastHit hit, hitLayers))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                Vector3 spawnPosition = hit.collider.transform.position;
                Quaternion spawnRotation = hit.collider.transform.rotation;

                if (BirdMeet != null)
                {
                    Instantiate(BirdMeet, spawnPosition, spawnRotation);
                }

                Destroy(hit.collider.gameObject);
                Debug.Log("Vogel getroffen!");
            }
        }

        Debug.DrawLine(start, end, Color.red);
    }
}
