using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Move : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int maxHealth = 5;
    public int maxChickenCount = 20;
    public int maxChickenLegSkillCount = 2;
    public float minY = -4.5f;
    public float maxY = 4.5f;
    public Camera targetCamera;
    public SpriteRenderer mapRenderer;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public Transform chickenLeg1;
    public Transform chickenLeg2;
    public float chickenLegOrbitRadius = 1.2f;
    public float chickenLegOrbitSpeed = 180f;
    public float chickenLegExpandTime = 0.25f;
    public float chickenLegFadeTime = 0.18f;
    public Text healthText;
    public Text chickenCountText;

    private Renderer playerRenderer;
    private int currentHealth;
    private int currentChickenCount;
    private int currentChickenLegSkillCount;
    private Transform[] chickenLegs;
    private SpriteRenderer[] chickenLegRenderers;
    private ChickenLegSkill[] chickenLegSkills;
    private Vector3[] chickenLegOriginalScales;
    private bool[] chickenLegUsed;
    private float chickenLegOrbitAngle;
    private bool isChickenLegSkillActive;
    private int activeChickenLegIndex;
    private float chickenLegSkillTimer;
    private float chickenLegTargetScale;
    private bool gameActive;

    public void SetGameActive(bool active)
    {
        gameActive = active;
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentChickenCount = maxChickenCount;
        currentChickenLegSkillCount = maxChickenLegSkillCount;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        playerRenderer = GetComponent<Renderer>();
        SetupChickenLeg();
        UpdateHealthUI();
        UpdateChickenCountUI();
        FitCameraHeightToMap();
    }

    void Update()
    {
        if (!gameActive)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            ShootBullet();
        }

        if (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame)
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
        if (currentChickenCount <= 0)
        {
            return;
        }

        Vector3 spawnPosition = transform.position;

        if (bulletSpawnPoint != null)
        {
            spawnPosition = bulletSpawnPoint.position;
        }

        Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        currentChickenCount--;
        UpdateChickenCountUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        UpdateHealthUI();
    }

    void SetupChickenLeg()
    {
        chickenLegs = new Transform[] { chickenLeg1, chickenLeg2 };
        chickenLegRenderers = new SpriteRenderer[chickenLegs.Length];
        chickenLegSkills = new ChickenLegSkill[chickenLegs.Length];
        chickenLegOriginalScales = new Vector3[chickenLegs.Length];
        chickenLegUsed = new bool[chickenLegs.Length];

        for (int i = 0; i < chickenLegs.Length; i++)
        {
            chickenLegOriginalScales[i] = chickenLegs[i].localScale;
            chickenLegRenderers[i] = chickenLegs[i].GetComponentInChildren<SpriteRenderer>();
            chickenLegSkills[i] = chickenLegs[i].GetComponent<ChickenLegSkill>();
            chickenLegSkills[i].SetCanDestroyEnemies(false);
            SetChickenLegAlpha(i, 1f);
        }
    }

    void UpdateChickenLeg()
    {
        if (isChickenLegSkillActive)
        {
            UpdateChickenLegSkill();
            return;
        }

        chickenLegOrbitAngle += chickenLegOrbitSpeed * Time.deltaTime;

        for (int i = 0; i < chickenLegs.Length; i++)
        {
            if (chickenLegUsed[i])
            {
                continue;
            }

            float phase = i * 180f;
            float angleInRadians = (chickenLegOrbitAngle + phase) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0f) * chickenLegOrbitRadius;
            chickenLegs[i].position = transform.position + offset;
            chickenLegs[i].localScale = chickenLegOriginalScales[i];
            SetChickenLegAlpha(i, 1f);
        }
    }

    void StartChickenLegSkill()
    {
        if (isChickenLegSkillActive || currentChickenLegSkillCount <= 0 || targetCamera == null || !targetCamera.orthographic)
        {
            return;
        }

        activeChickenLegIndex = maxChickenLegSkillCount - currentChickenLegSkillCount;
        currentChickenLegSkillCount--;
        isChickenLegSkillActive = true;
        chickenLegSkillTimer = 0f;
        chickenLegTargetScale = GetChickenLegScreenFillScale();
        chickenLegSkills[activeChickenLegIndex].SetCanDestroyEnemies(true);
        SetChickenLegAlpha(activeChickenLegIndex, 1f);
        DestroyEnemiesInCamera();
    }

    void UpdateChickenLegSkill()
    {
        chickenLegSkillTimer += Time.deltaTime;

        Vector3 cameraPosition = targetCamera.transform.position;
        Transform activeChickenLeg = chickenLegs[activeChickenLegIndex];
        activeChickenLeg.position = new Vector3(cameraPosition.x, cameraPosition.y, activeChickenLeg.position.z);

        float expandTime = Mathf.Max(chickenLegExpandTime, 0.01f);
        float fadeTime = Mathf.Max(chickenLegFadeTime, 0.01f);

        if (chickenLegSkillTimer <= expandTime)
        {
            float progress = chickenLegSkillTimer / expandTime;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            activeChickenLeg.localScale = chickenLegOriginalScales[activeChickenLegIndex] * Mathf.Lerp(1f, chickenLegTargetScale, easedProgress);
            DestroyEnemiesInCamera();
            return;
        }

        float fadeProgress = (chickenLegSkillTimer - expandTime) / fadeTime;
        SetChickenLegAlpha(activeChickenLegIndex, 1f - fadeProgress);

        if (fadeProgress >= 1f)
        {
            EndChickenLegSkill();
        }
    }

    void EndChickenLegSkill()
    {
        isChickenLegSkillActive = false;
        chickenLegUsed[activeChickenLegIndex] = true;
        chickenLegs[activeChickenLegIndex].localScale = chickenLegOriginalScales[activeChickenLegIndex];
        chickenLegSkills[activeChickenLegIndex].SetCanDestroyEnemies(false);
        SetChickenLegAlpha(activeChickenLegIndex, 0f);
        UpdateChickenLeg();
    }

    float GetChickenLegScreenFillScale()
    {
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        Bounds bounds = chickenLegRenderers[activeChickenLegIndex].bounds;
        float currentWidth = bounds.size.x;
        float currentHeight = bounds.size.y;

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

    void SetChickenLegAlpha(int index, float alpha)
    {
        Color color = chickenLegRenderers[index].color;
        color.a = Mathf.Clamp01(alpha);
        chickenLegRenderers[index].color = color;
    }

    void UpdateHealthUI()
    {
        string hearts = "";

        for (int i = 0; i < currentHealth; i++)
        {
            hearts += "♥";
        }

        healthText.text = hearts;
    }

    void UpdateChickenCountUI()
    {
        chickenCountText.text = "남은 치킨 : " + currentChickenCount.ToString();
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
