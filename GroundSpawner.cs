using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zemin parçalarını (tile) oyuncunun önünde sürekli üretir ve
/// arkada kalanları geri döngüye sokar (object pooling).
/// Her zemin tile'ı ayrıca ObstacleSpawner içerebilir.
/// </summary>
public class GroundSpawner : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform player;
    public GameObject[] groundTilePrefabs; // farklı varyasyonlarda zemin prefabları

    [Header("Ayarlar")]
    public float tileLength = 20f;
    public int tilesOnScreen = 5;          // aynı anda sahnede kaç tile olacak
    public float spawnZStart = 0f;

    private float nextSpawnZ;
    private Queue<GameObject> activeTiles = new Queue<GameObject>();

    private void Start()
    {
        nextSpawnZ = spawnZStart;

        // Başlangıçta ekranı dolduracak kadar tile spawn et
        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile();
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Oyuncu, en öndeki tile'a belirli bir mesafeden yaklaştığında yeni tile ekle
        if (player.position.z > nextSpawnZ - (tilesOnScreen * tileLength))
        {
            SpawnTile();
            DeleteOldestTile();
        }
    }

    private void SpawnTile()
    {
        int index = Random.Range(0, groundTilePrefabs.Length);
        GameObject tile = Instantiate(groundTilePrefabs[index], transform);
        tile.transform.position = new Vector3(0, 0, nextSpawnZ);

        activeTiles.Enqueue(tile);
        nextSpawnZ += tileLength;

        // Tile üzerindeki ObstacleSpawner varsa tetikle
        ObstacleSpawner spawner = tile.GetComponentInChildren<ObstacleSpawner>();
        if (spawner != null)
        {
            spawner.SpawnObstaclesAndCoins();
        }
    }

    private void DeleteOldestTile()
    {
        if (activeTiles.Count <= tilesOnScreen)
            return;

        GameObject oldTile = activeTiles.Dequeue();
        Destroy(oldTile);
    }
}
