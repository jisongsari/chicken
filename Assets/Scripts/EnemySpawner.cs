using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Camera targetCamera;
    public SpriteRenderer mapRenderer;
    public float spawnInterval = 1f;
    public float horizontalCameraMultiplier = 2f;

    private float spawnTimer;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        if (enemyPrefab == null || targetCamera == null || !targetCamera.orthographic)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval)
        {
            return;
        }

        spawnTimer = 0f;
        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 cameraPosition = targetCamera.transform.position;
        float cameraHalfHeight = targetCamera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * targetCamera.aspect;

        float halfSpawnWidth = cameraHalfWidth * horizontalCameraMultiplier;
        float minX = cameraPosition.x - halfSpawnWidth;
        float maxX = cameraPosition.x + halfSpawnWidth;
        float minY = cameraPosition.y - cameraHalfHeight;
        float maxY = cameraPosition.y + cameraHalfHeight;

        if (mapRenderer != null)
        {
            Bounds mapBounds = mapRenderer.bounds;
            minX = Mathf.Max(minX, mapBounds.min.x);
            maxX = Mathf.Min(maxX, mapBounds.max.x);
            minY = Mathf.Max(minY, mapBounds.min.y);
            maxY = Mathf.Min(maxY, mapBounds.max.y);
        }

        if (minX > maxX)
        {
            minX = maxX = cameraPosition.x;
        }

        if (minY > maxY)
        {
            minY = maxY = cameraPosition.y;
        }

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        return new Vector3(x, y, enemyPrefab.transform.position.z);
    }
}
