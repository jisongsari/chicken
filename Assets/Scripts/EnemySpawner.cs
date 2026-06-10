using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Camera targetCamera;
    public SpriteRenderer mapRenderer;
    public GameObject yudam;
    public float spawnInterval = 1f;

    private const float SpawnBandMultiplier = 0.5f;
    private const float CameraEdgePadding = 0.05f;
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
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        Enemy enemy = spawnedEnemy.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.yudam = yudam;
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 cameraPosition = targetCamera.transform.position;
        float cameraHalfHeight = targetCamera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * targetCamera.aspect;
        float cameraHeight = cameraHalfHeight * 2f;
        float cameraWidth = cameraHalfWidth * 2f;
        float minCameraX = cameraPosition.x - cameraHalfWidth;
        float maxCameraX = cameraPosition.x + cameraHalfWidth;
        float minCameraY = cameraPosition.y - cameraHalfHeight;
        float maxCameraY = cameraPosition.y + cameraHalfHeight;

        for (int i = 0; i < 8; i++)
        {
            int side = Random.Range(0, 4);
            float minX = minCameraX;
            float maxX = maxCameraX;
            float minY = minCameraY;
            float maxY = maxCameraY;

            if (side == 0)
            {
                minX = minCameraX - cameraWidth * SpawnBandMultiplier;
                maxX = minCameraX - CameraEdgePadding;
            }
            else if (side == 1)
            {
                minX = maxCameraX + CameraEdgePadding;
                maxX = maxCameraX + cameraWidth * SpawnBandMultiplier;
            }
            else if (side == 2)
            {
                minY = maxCameraY + CameraEdgePadding;
                maxY = maxCameraY + cameraHeight * SpawnBandMultiplier;
            }
            else
            {
                minY = minCameraY - cameraHeight * SpawnBandMultiplier;
                maxY = minCameraY - CameraEdgePadding;
            }

            if (mapRenderer != null)
            {
                Bounds mapBounds = mapRenderer.bounds;
                minX = Mathf.Max(minX, mapBounds.min.x);
                maxX = Mathf.Min(maxX, mapBounds.max.x);
                minY = Mathf.Max(minY, mapBounds.min.y);
                maxY = Mathf.Min(maxY, mapBounds.max.y);
            }

            if (minX <= maxX && minY <= maxY)
            {
                float x = Random.Range(minX, maxX);
                float y = Random.Range(minY, maxY);
                return new Vector3(x, y, enemyPrefab.transform.position.z);
            }
        }

        float fallbackX = Random.Range(minCameraX, maxCameraX);
        float fallbackY = maxCameraY + cameraHeight * SpawnBandMultiplier;
        return new Vector3(fallbackX, fallbackY, enemyPrefab.transform.position.z);
    }
}
