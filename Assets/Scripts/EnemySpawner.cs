using UnityEngine;

// 적 스포너 클래스: 일정 시간마다 카메라 화면 밖에서 적을 생성
public class EnemySpawner : MonoBehaviour
{
    // 적 프리팹과 필요한 오브젝트들
    public GameObject enemyPrefab;
    public Camera targetCamera;
    public SpriteRenderer mapRenderer;
    public GameObject yudam;

    // 적 생성 간격
    public float spawnInterval = 1f;

    // 적을 카메라 바깥에 생성하기 위한 범위
    private const float SpawnBandMultiplier = 0.5f;
    private const float CameraEdgePadding = 0.05f;

    // 생성 시간 측정용 타이머
    private float spawnTimer;

    void Start()
    {
        // 카메라가 지정되지 않았다면 메인 카메라 사용
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        // 필요한 오브젝트가 없거나 카메라가 Orthographic이 아니면 실행하지 않음
        if (enemyPrefab == null || targetCamera == null || !targetCamera.orthographic)
        {
            return;
        }

        // 생성 타이머 증가
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval)
        {
            return;
        }

        // 일정 시간이 지나면 적 생성
        spawnTimer = 0f;
        SpawnEnemy();
    }

    // 적 생성
    void SpawnEnemy()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        GameObject spawnedEnemy =
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        Enemy enemy = spawnedEnemy.GetComponent<Enemy>();

        // 생성된 적에게 플레이어 정보 전달
        if (enemy != null)
        {
            enemy.yudam = yudam;
        }
    }

    // 카메라 화면 밖의 랜덤 위치 계산
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

        // 화면의 네 방향 중 하나를 선택하여 생성 위치 탐색
        for (int i = 0; i < 8; i++)
        {
            int side = Random.Range(0, 4);

            float minX = minCameraX;
            float maxX = maxCameraX;
            float minY = minCameraY;
            float maxY = maxCameraY;

            if (side == 0)
            {
                // 왼쪽
                minX = minCameraX - cameraWidth * SpawnBandMultiplier;
                maxX = minCameraX - CameraEdgePadding;
            }
            else if (side == 1)
            {
                // 오른쪽
                minX = maxCameraX + CameraEdgePadding;
                maxX = maxCameraX + cameraWidth * SpawnBandMultiplier;
            }
            else if (side == 2)
            {
                // 위쪽
                minY = maxCameraY + CameraEdgePadding;
                maxY = maxCameraY + cameraHeight * SpawnBandMultiplier;
            }
            else
            {
                // 아래쪽
                minY = minCameraY - cameraHeight * SpawnBandMultiplier;
                maxY = minCameraY - CameraEdgePadding;
            }

            // 맵 밖으로 생성되지 않도록 범위 제한
            if (mapRenderer != null)
            {
                Bounds mapBounds = mapRenderer.bounds;

                minX = Mathf.Max(minX, mapBounds.min.x);
                maxX = Mathf.Min(maxX, mapBounds.max.x);
                minY = Mathf.Max(minY, mapBounds.min.y);
                maxY = Mathf.Min(maxY, mapBounds.max.y);
            }

            // 유효한 위치라면 랜덤 생성
            if (minX <= maxX && minY <= maxY)
            {
                float x = Random.Range(minX, maxX);
                float y = Random.Range(minY, maxY);

                return new Vector3(x, y, enemyPrefab.transform.position.z);
            }
        }

        // 적절한 위치를 찾지 못하면 화면 위쪽에서 생성
        float fallbackX = Random.Range(minCameraX, maxCameraX);
        float fallbackY = maxCameraY + cameraHeight * SpawnBandMultiplier;

        return new Vector3(fallbackX, fallbackY, enemyPrefab.transform.position.z);
    }
}