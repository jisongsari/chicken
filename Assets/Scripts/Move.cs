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
    public Transform chickenLeg1;
    public Transform chickenLeg2;
    public float chickenLegOrbitRadius = 1.2f;
    public float chickenLegOrbitSpeed = 180f;
    public float chickenLegExpandTime = 0.25f;
    public float chickenLegFadeTime = 0.18f;
    public float chickenLegScaleFactor = 1.2f;
    public GameObject destination;

    [Header("Umbrella")]
    public float umbrellaRadiusMultiplier = 2f;
    public Color umbrellaColor = Color.cyan;
    public float umbrellaLineWidth = 0.05f;

    private Renderer playerRenderer;
    private Umbrella umbrella;
    private Boss cachedBoss;
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

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        playerRenderer = GetComponent<Renderer>();
        SetupChickenLegs();
        SetupUmbrella();
        FitCameraHeightToMap();
        cachedBoss = FindAnyObjectByType<Boss>();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.wasPressedThisFrame)
            ShootBullet();

        if (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame)
            StartChickenLegSkill();

        if (keyboard.commaKey.wasPressedThisFrame)
            umbrella?.Open();
        if (keyboard.periodKey.wasPressedThisFrame)
            umbrella?.Close();

        Vector2 input = Vector2.zero;
        // 안!지!호! 입니다!
        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) input.x = -1f;
        else if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) input.x = 1f;

        if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) input.y = -1f;
        else if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) input.y = 1f;

        Vector3 direction = new Vector3(input.x, input.y, 0f).normalized;
        Vector3 nextPosition = transform.position + direction * moveSpeed * Time.deltaTime;
        ClampPlayerPosition(ref nextPosition);
        transform.position = nextPosition;

        UpdateChickenLeg();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryReachGoal(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryReachGoal(collision.gameObject);
    }

    void TryReachGoal(GameObject other)
    {
        if (other == destination || other.transform.IsChildOf(destination.transform))
            GameManager.Instance.OnGoalReached();
    }

    public void TakeDamage(int damage)
    {
        GameManager.Instance.TakeDamage(damage);
    }

    void ShootBullet()
    {
        if (!GameManager.Instance.TryShoot()) return;

        Vector3 spawnPosition = bulletSpawnPoint != null ? bulletSpawnPoint.position : transform.position;
        Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
    }

    void SetupUmbrella()
    {
        GameObject umbrellaObj = new GameObject("Umbrella");
        umbrellaObj.transform.SetParent(transform);
        umbrellaObj.transform.localPosition = Vector3.zero;
        umbrella = umbrellaObj.AddComponent<Umbrella>();
        umbrella.color = umbrellaColor;
        umbrella.lineWidth = umbrellaLineWidth;

        float headRadius = playerRenderer != null ? playerRenderer.bounds.extents.x : 0.5f;
        umbrella.Initialize(headRadius * umbrellaRadiusMultiplier);
    }

    void SetupChickenLegs()
    {
        chickenLegs = new Transform[] { chickenLeg1, chickenLeg2 };
        int count = chickenLegs.Length;
        chickenLegRenderers = new SpriteRenderer[count];
        chickenLegSkills = new ChickenLegSkill[count];
        chickenLegOriginalScales = new Vector3[count];
        chickenLegUsed = new bool[count];

        for (int i = 0; i < count; i++)
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
            if (chickenLegUsed[i]) continue;

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
        if (isChickenLegSkillActive || targetCamera == null || !targetCamera.orthographic) return;

        int skillIndex = GameManager.Instance.TryUseChickenLegSkill();
        if (skillIndex < 0) return;

        activeChickenLegIndex = skillIndex;
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
            EndChickenLegSkill();
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
        return Mathf.Max(cameraWidth / bounds.size.x, cameraHeight / bounds.size.y) * chickenLegScaleFactor;
    }

    void DestroyEnemiesInCamera()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            if (IsInCamera(enemy.transform.position))
                Destroy(enemy.gameObject);
        }
    }

    bool IsInCamera(Vector3 worldPosition)
    {
        Vector3 viewportPosition = targetCamera.WorldToViewportPoint(worldPosition);
        return viewportPosition.z > 0f
            && viewportPosition.x >= 0f && viewportPosition.x <= 1f
            && viewportPosition.y >= 0f && viewportPosition.y <= 1f;
    }

    void SetChickenLegAlpha(int index, float alpha)
    {
        Color color = chickenLegRenderers[index].color;
        color.a = Mathf.Clamp01(alpha);
        chickenLegRenderers[index].color = color;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 cameraPosition = targetCamera.transform.position;
        cameraPosition.x = transform.position.x;
        cameraPosition.y = transform.position.y;
        ClampCameraPosition(ref cameraPosition);
        cameraPosition += Boss.CameraShakeOffset;
        targetCamera.transform.position = cameraPosition;
    }

    void FitCameraHeightToMap()
    {
        if (targetCamera == null || mapRenderer == null || !targetCamera.orthographic) return;
        targetCamera.orthographicSize = mapRenderer.bounds.size.y * 0.5f;
    }

    void ClampPlayerPosition(ref Vector3 position)
    {
        position.y = Mathf.Clamp(position.y, minY, maxY);

        if (mapRenderer == null) return;

        Bounds mapBounds = mapRenderer.bounds;
        float playerHalfWidth = playerRenderer != null ? playerRenderer.bounds.extents.x : 0f;

        float maxX = mapBounds.max.x - playerHalfWidth;

        // 보스가 있으면 보스 왼쪽 경계로 X 제한
        if (cachedBoss != null)
        {
            Renderer bossRenderer = cachedBoss.GetComponent<Renderer>();
            float bossHalfWidth = bossRenderer != null ? bossRenderer.bounds.extents.x : 0.5f;
            maxX = Mathf.Min(maxX, cachedBoss.transform.position.x - bossHalfWidth - playerHalfWidth);
        }

        position.x = Mathf.Clamp(position.x, mapBounds.min.x + playerHalfWidth, maxX);
    }

    void ClampCameraPosition(ref Vector3 position)
    {
        if (mapRenderer == null || targetCamera == null || !targetCamera.orthographic) return;

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
