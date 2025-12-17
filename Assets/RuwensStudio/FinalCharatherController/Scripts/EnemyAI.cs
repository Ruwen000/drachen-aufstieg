using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float speed = 5f;
    public float changeTargetDistance = 1f;
    public float flyRange = 20f;
    public float heightMin = 5f;
    public float heightMax = 20f;

    private Vector3 targetPosition;

    void Start()
    {
        PickNewTarget();
    }

    void Update()
    {
        FlyTowardsTarget();

        if (Vector3.Distance(transform.position, targetPosition) < changeTargetDistance)
        {
            PickNewTarget();
        }
    }

    void PickNewTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * flyRange;
        randomDirection.y = 0; 

        Vector3 basePosition = transform.position;
        basePosition.y = Random.Range(heightMin, heightMax); 

        targetPosition = basePosition + randomDirection;
    }

    void FlyTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        Quaternion toRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 2);
    }
}
