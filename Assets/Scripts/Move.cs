using UnityEngine;
using UnityEngine.InputSystem;

public class Move : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int maxHealth = 5;
    public float minY = -4.5f;
    public float maxY = 4.5f;
    public Camera targetCamera;
    public SpriteRenderer mapRenderer;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public Transform chickenLeg;
    public float chickenLegOrbitRadius = 1.2f;
    public float chickenLegOrbitSpeed = 180f;
    public float chickenLegExpandTime = 0.25f;
    public float chickenLegFadeTime = 0.18f;

    private Renderer playerRenderer;
    private int currentHealth;
    private GUIStyle heartStyle;
    private SpriteRenderer chickenLegRenderer;
    private ChickenLegSkill chickenLegSkill;
    private Vector3 chickenLegOriginalScale;
    private float chickenLegOrbitAngle;
    private bool isChickenLegSkillActive;
    private float chickenLegSkillTimer;
    private float chickenLegTargetScale;

    void Start()
    {
        currentHealth = maxHealth;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        playerRenderer = GetComponent<Renderer>();
        SetupChickenLeg();
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

        if ((keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame) && chickenLeg != null)
        {
            StartChickenLegSkill();
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

        UpdateChickenLeg();
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

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
    }

    void SetupChickenLeg()
    {
        if (chickenLeg == null)
        {
            return;
        }

        chickenLegOriginalScale = chickenLeg.localScale;
        chickenLegRenderer = chickenLeg.GetComponentInChildren<SpriteRenderer>();
        chickenLegSkill = chickenLeg.GetComponent<ChickenLegSkill>();
        chickenLegSkill.SetCanDestroyEnemies(false);
        SetChickenLegAlpha(1f);
    }

    void UpdateChickenLeg()
    {
        if (chickenLeg == null)
        {
            return;
        }

        if (isChickenLegSkillActive)
        {
            UpdateChickenLegSkill();
            return;
        }

        chickenLegOrbitAngle += chickenLegOrbitSpeed * Time.deltaTime;
        float angleInRadians = chickenLegOrbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0f) * chickenLegOrbitRadius;
        chickenLeg.position = transform.position + offset;
        chickenLeg.localScale = chickenLegOriginalScale;
        SetChickenLegAlpha(1f);
    }

    void StartChickenLegSkill()
    {
        if (isChickenLegSkillActive || targetCamera == null || !targetCamera.orthographic)
        {
            return;
        }

        isChickenLegSkillActive = true;
        chickenLegSkillTimer = 0f;
        chickenLegTargetScale = GetChickenLegScreenFillScale();

        if (chickenLegSkill != null)
        {
            chickenLegSkill.SetCanDestroyEnemies(true);
        }

        SetChickenLegAlpha(1f);
        DestroyEnemiesInCamera();
    }

    void UpdateChickenLegSkill()
    {
        chickenLegSkillTimer += Time.deltaTime;

        Vector3 cameraPosition = targetCamera.transform.position;
        chickenLeg.position = new Vector3(cameraPosition.x, cameraPosition.y, chickenLeg.position.z);

        float expandTime = Mathf.Max(chickenLegExpandTime, 0.01f);
        float fadeTime = Mathf.Max(chickenLegFadeTime, 0.01f);

        if (chickenLegSkillTimer <= expandTime)
        {
            float progress = chickenLegSkillTimer / expandTime;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            chickenLeg.localScale = chickenLegOriginalScale * Mathf.Lerp(1f, chickenLegTargetScale, easedProgress);
            DestroyEnemiesInCamera();
            return;
        }

        float fadeProgress = (chickenLegSkillTimer - expandTime) / fadeTime;
        SetChickenLegAlpha(1f - fadeProgress);

        if (fadeProgress >= 1f)
        {
            EndChickenLegSkill();
        }
    }

    void EndChickenLegSkill()
    {
        isChickenLegSkillActive = false;
        chickenLeg.localScale = chickenLegOriginalScale;

        if (chickenLegSkill != null)
        {
            chickenLegSkill.SetCanDestroyEnemies(false);
        }

        SetChickenLegAlpha(1f);
        UpdateChickenLeg();
    }

    float GetChickenLegScreenFillScale()
    {
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        if (chickenLegRenderer == null)
        {
            return Mathf.Max(cameraWidth, cameraHeight);
        }

        Bounds bounds = chickenLegRenderer.bounds;
        float currentWidth = bounds.size.x;
        float currentHeight = bounds.size.y;

        if (currentWidth <= 0f || currentHeight <= 0f)
        {
            return Mathf.Max(cameraWidth, cameraHeight);
        }

        return Mathf.Max(cameraWidth / currentWidth, cameraHeight / currentHeight) * 1.2f;
    }

    void DestroyEnemiesInCamera()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            if (IsInCamera(enemy.transform.position))
            {
                Destroy(enemy.gameObject);
            }
        }
    }

    bool IsInCamera(Vector3 worldPosition)
    {
        Vector3 viewportPosition = targetCamera.WorldToViewportPoint(worldPosition);
        return viewportPosition.z > 0f
            && viewportPosition.x >= 0f
            && viewportPosition.x <= 1f
            && viewportPosition.y >= 0f
            && viewportPosition.y <= 1f;
    }

    void SetChickenLegAlpha(float alpha)
    {
        if (chickenLegRenderer == null)
        {
            return;
        }

        Color color = chickenLegRenderer.color;
        color.a = Mathf.Clamp01(alpha);
        chickenLegRenderer.color = color;
    }

    void OnGUI()
    {
        if (heartStyle == null)
        {
            heartStyle = new GUIStyle(GUI.skin.label);
            heartStyle.fontSize = 56;
            heartStyle.normal.textColor = Color.red;
        }

        string hearts = "";

        for (int i = 0; i < currentHealth; i++)
        {
            hearts += "♥";
        }

        GUI.Label(new Rect(20f, 12f, 420f, 80f), hearts, heartStyle);
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
