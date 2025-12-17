using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int enemyCount = 5;
    public Vector3 spawnAreaSize = new Vector3(20, 5, 20);

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float y = Random.Range(5, 10); 
        float z = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);

   
        return transform.position + new Vector3(x, y, z);
    }
}
