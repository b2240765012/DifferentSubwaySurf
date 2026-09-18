using UnityEngine;

/// <summary>
/// Bir zemin tile'ı üzerinde, 3 lane'e rastgele desenlerde
/// engel ve coin yerleştirir. Her tile prefabına eklenir.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefablar")]
    public GameObject[] obstaclePrefabs; // yerde engel, üstten atlanabilir
    public GameObject[] highObstaclePrefabs; // altından kayarak geçilebilir engeller
    public GameObject coinPrefab;

    [Header("Lane Ayarları")]
    public float laneDistance = 3f;
    public float localZOffset = 10f; // tile içinde nereye yerleşeceği

    [Header("Olasılıklar (0-1)")]
    [Range(0f, 1f)] public float obstacleSpawnChance = 0.6f;
    [Range(0f, 1f)] public float coinRowChance = 0.5f;

    public void SpawnObstaclesAndCoins()
    {
        // Rastgele bir engel deseni seç
        if (Random.value < obstacleSpawnChance)
        {
            SpawnRandomObstaclePattern();
        }

        if (Random.value < coinRowChance)
        {
            SpawnCoinRow();
        }
    }

    private void SpawnRandomObstaclePattern()
    {
        // 0, 1 veya 2 lane'e engel koy (üçünü birden asla kapatma!)
        int blockedLanes = Random.Range(1, 3); // 1 veya 2 lane kapatılır
        int[] lanes = { 0, 1, 2 };
        Shuffle(lanes);

        for (int i = 0; i < blockedLanes; i++)
        {
            int lane = lanes[i];
            bool isHighObstacle = Random.value > 0.5f && highObstaclePrefabs.Length > 0;

            GameObject prefab = isHighObstacle
                ? highObstaclePrefabs[Random.Range(0, highObstaclePrefabs.Length)]
                : obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

            Vector3 spawnPos = transform.position + new Vector3((lane - 1) * laneDistance, 0, localZOffset);
            Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        }
    }

    private void SpawnCoinRow()
    {
        int lane = Random.Range(0, 3);
        int coinCount = Random.Range(3, 6);
        float spacing = 1.5f;

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPos = transform.position +
                new Vector3((lane - 1) * laneDistance, 1f, localZOffset + i * spacing);
            Instantiate(coinPrefab, spawnPos, Quaternion.identity, transform);
        }
    }

    private void Shuffle(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
