using UnityEngine;
using UnityEngine.InputSystem;

public class Move : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float minY = -4.5f;
    public float maxY = 4.5f;
    public Camera targetCamera;
    public SpriteRenderer mapRenderer;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;

    private Renderer playerRenderer;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        playerRenderer = GetComponent<Renderer>();
        FitCameraHeightToMap();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            ShootBullet();
        }

        Vector2 input = Vector2.zero;
        // 안!지!호! 입니다!
        if (keyboard.leftArrowKey.isPressed||keyboard.aKey.isPressed)
        {
            input.x = -1f;
        }
        else if (keyboard.rightArrowKey.isPressed||keyboard.dKey.isPressed)
        {
            input.x = 1f;
        }

        if (keyboard.downArrowKey.isPressed|| keyboard.sKey.isPressed)
        {
            input.y = -1f;
        }
        else if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed)
        {
            input.y = 1f;
        }

        Vector3 direction = new Vector3(input.x, input.y, 0f).normalized;
        Vector3 nextPosition = transform.position + direction * moveSpeed * Time.deltaTime;
        ClampPlayerPosition(ref nextPosition);
        transform.position = nextPosition;
    }

    void ShootBullet()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = transform.position;

        if (bulletSpawnPoint != null)
        {
            spawnPosition = bulletSpawnPoint.position;
        }

        Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 cameraPosition = targetCamera.transform.position;
        cameraPosition.x = transform.position.x;
        cameraPosition.y = transform.position.y;
        ClampCameraPosition(ref cameraPosition);
        targetCamera.transform.position = cameraPosition;
    }

    void FitCameraHeightToMap()
    {
        if (targetCamera == null || mapRenderer == null || !targetCamera.orthographic)
        {
            return;
        }

        targetCamera.orthographicSize = mapRenderer.bounds.size.y * 0.5f;
    }

    void ClampPlayerPosition(ref Vector3 position)
    {
        position.y = Mathf.Clamp(position.y, minY, maxY);

        if (mapRenderer == null)
        {
            return;
        }

        Bounds mapBounds = mapRenderer.bounds;
        float playerHalfWidth = 0f;

        if (playerRenderer != null)
        {
            playerHalfWidth = playerRenderer.bounds.extents.x;
        }

        position.x = Mathf.Clamp(
            position.x,
            mapBounds.min.x + playerHalfWidth,
            mapBounds.max.x - playerHalfWidth
        );
    }

    void ClampCameraPosition(ref Vector3 position)
    {
        if (mapRenderer == null || targetCamera == null || !targetCamera.orthographic)
        {
            return;
        }

        Bounds mapBounds = mapRenderer.bounds;
        float cameraHalfHeight = targetCamera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * targetCamera.aspect;

        position.x = Mathf.Clamp(
            position.x,
            mapBounds.min.x + cameraHalfWidth,
            mapBounds.max.x - cameraHalfWidth
        );

        position.y = mapBounds.center.y;
    }
}
