using UnityEngine;
using UnityEngine.InputSystem;

/// 플레이어 이동 및 스킬 관리 클래스
public class Move : MonoBehaviour
{
    // 플레이어 이동 속도 및 이동 범위
    public float moveSpeed = 5f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    // 카메라 및 맵 정보
    public Camera targetCamera;
    public SpriteRenderer mapRenderer;

    // 총알 관련
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;

    // 닭다리 스킬 관련
    public Transform chickenLeg1;
    public Transform chickenLeg2;
    public float chickenLegOrbitRadius = 1.2f;
    public float chickenLegOrbitSpeed = 180f;
    public float chickenLegExpandTime = 0.25f;
    public float chickenLegFadeTime = 0.18f;
    public float chickenLegScaleFactor = 1.2f;

    // 목적지
    public GameObject destination;

    [Header("Umbrella")]
    public float umbrellaRadiusMultiplier = 2f;
    public Color umbrellaColor = Color.cyan;
    public float umbrellaLineWidth = 0.05f;

    private Renderer playerRenderer;
    private Umbrella umbrella;
    private Boss cachedBoss;

    // 닭다리 스킬 관리용 변수
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
        // 필요한 컴포넌트 및 오브젝트 초기화
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
        // 키보드 입력 처리
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 총알 발사
        if (keyboard.spaceKey.wasPressedThisFrame)
            ShootBullet();

        // 닭다리 필살기 사용
        if (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame)
            StartChickenLegSkill();

        // 양심우산 열기 / 닫기
        if (keyboard.commaKey.wasPressedThisFrame)
            umbrella?.Open();
        if (keyboard.periodKey.wasPressedThisFrame)
            umbrella?.Close();

        // WASD와 방향키 입력으로 이동 방향 계산
        Vector2 input = Vector2.zero;

        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) input.x = -1f;
        else if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) input.x = 1f;

        if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) input.y = -1f;
        else if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) input.y = 1f;

        Vector3 direction = new Vector3(input.x, input.y, 0f).normalized;
        Vector3 nextPosition = transform.position + direction * moveSpeed * Time.deltaTime;

        // 이동 가능한 범위로 위치 제한
        ClampPlayerPosition(ref nextPosition);

        transform.position = nextPosition;

        // 닭다리 위치 갱신
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

    // 목적지에 도착했는지 확인
    void TryReachGoal(GameObject other)
    {
        if (other == destination || other.transform.IsChildOf(destination.transform))
            GameManager.Instance.OnGoalReached();
    }

    // 플레이어가 받은 데미지를 GameManager에 전달
    public void TakeDamage(int damage)
    {
        GameManager.Instance.TakeDamage(damage);
    }

    // 총알 생성
    void ShootBullet()
    {
        if (!GameManager.Instance.TryShoot()) return;

        Vector3 spawnPosition = bulletSpawnPoint != null ? bulletSpawnPoint.position : transform.position;
        Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
    }

    // 양심우산 생성 및 초기화
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

    // 닭다리 스킬 오브젝트 초기화
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

    // 닭다리가 플레이어 주위를 공전하도록 갱신
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

            Vector3 offset =
                new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0f)
                * chickenLegOrbitRadius;

            chickenLegs[i].position = transform.position + offset;
            chickenLegs[i].localScale = chickenLegOriginalScales[i];

            SetChickenLegAlpha(i, 1f);
        }
    }

    // 닭다리 필살기 시작
    void StartChickenLegSkill()
    {
        if (isChickenLegSkillActive || targetCamera == null || !targetCamera.orthographic)
            return;

        int skillIndex = GameManager.Instance.TryUseChickenLegSkill();
        if (skillIndex < 0) return;

        activeChickenLegIndex = skillIndex;
        isChickenLegSkillActive = true;
        chickenLegSkillTimer = 0f;

        chickenLegTargetScale = GetChickenLegScreenFillScale();

        chickenLegSkills[activeChickenLegIndex].SetCanDestroyEnemies(true);

        SetChickenLegAlpha(activeChickenLegIndex, 1f);

        // 화면 안의 적 즉시 제거
        DestroyEnemiesInCamera();
    }

    // 필살기 진행(확대 및 사라짐)
    void UpdateChickenLegSkill()
    {
        chickenLegSkillTimer += Time.deltaTime;

        Vector3 cameraPosition = targetCamera.transform.position;
        Transform activeChickenLeg = chickenLegs[activeChickenLegIndex];

        activeChickenLeg.position =
            new Vector3(cameraPosition.x, cameraPosition.y, activeChickenLeg.position.z);

        float expandTime = Mathf.Max(chickenLegExpandTime, 0.01f);
        float fadeTime = Mathf.Max(chickenLegFadeTime, 0.01f);

        if (chickenLegSkillTimer <= expandTime)
        {
            float progress = chickenLegSkillTimer / expandTime;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

            activeChickenLeg.localScale =
                chickenLegOriginalScales[activeChickenLegIndex]
                * Mathf.Lerp(1f, chickenLegTargetScale, easedProgress);

            DestroyEnemiesInCamera();
            return;
        }

        float fadeProgress = (chickenLegSkillTimer - expandTime) / fadeTime;

        SetChickenLegAlpha(activeChickenLegIndex, 1f - fadeProgress);

        if (fadeProgress >= 1f)
            EndChickenLegSkill();
    }

    // 필살기 종료 및 상태 초기화
    void EndChickenLegSkill()
    {
        isChickenLegSkillActive = false;

        chickenLegUsed[activeChickenLegIndex] = true;

        chickenLegs[activeChickenLegIndex].localScale =
            chickenLegOriginalScales[activeChickenLegIndex];

        chickenLegSkills[activeChickenLegIndex].SetCanDestroyEnemies(false);

        SetChickenLegAlpha(activeChickenLegIndex, 0f);

        UpdateChickenLeg();
    }

    // 화면을 덮기 위한 확대 배율 계산
    float GetChickenLegScreenFillScale()
    {
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        Bounds bounds = chickenLegRenderers[activeChickenLegIndex].bounds;

        return Mathf.Max(
            cameraWidth / bounds.size.x,
            cameraHeight / bounds.size.y)
            * chickenLegScaleFactor;
    }

    // 현재 화면 안의 적 제거
    void DestroyEnemiesInCamera()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            if (IsInCamera(enemy.transform.position))
                Destroy(enemy.gameObject);
        }
    }

    // 오브젝트가 카메라 안에 있는지 확인
    bool IsInCamera(Vector3 worldPosition)
    {
        Vector3 viewportPosition = targetCamera.WorldToViewportPoint(worldPosition);

        return viewportPosition.z > 0f
            && viewportPosition.x >= 0f && viewportPosition.x <= 1f
            && viewportPosition.y >= 0f && viewportPosition.y <= 1f;
    }

    // 닭다리의 투명도 설정
    void SetChickenLegAlpha(int index, float alpha)
    {
        Color color = chickenLegRenderers[index].color;
        color.a = Mathf.Clamp01(alpha);
        chickenLegRenderers[index].color = color;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        // 플레이어를 따라 카메라 이동
        Vector3 cameraPosition = targetCamera.transform.position;

        cameraPosition.x = transform.position.x;
        cameraPosition.y = transform.position.y;

        ClampCameraPosition(ref cameraPosition);

        cameraPosition += Boss.CameraShakeOffset;

        targetCamera.transform.position = cameraPosition;
    }

    // 맵 높이에 맞게 카메라 크기 설정
    void FitCameraHeightToMap()
    {
        if (targetCamera == null || mapRenderer == null || !targetCamera.orthographic)
            return;

        targetCamera.orthographicSize = mapRenderer.bounds.size.y * 0.5f;
    }

    // 플레이어 이동 범위를 맵과 보스 위치에 맞게 제한
    void ClampPlayerPosition(ref Vector3 position)
    {
        position.y = Mathf.Clamp(position.y, minY, maxY);

        if (mapRenderer == null) return;

        Bounds mapBounds = mapRenderer.bounds;

        float playerHalfWidth =
            playerRenderer != null ? playerRenderer.bounds.extents.x : 0f;

        float maxX = mapBounds.max.x - playerHalfWidth;

        if (cachedBoss != null)
        {
            Renderer bossRenderer = cachedBoss.GetComponent<Renderer>();

            float bossHalfWidth =
                bossRenderer != null ? bossRenderer.bounds.extents.x : 0.5f;

            maxX = Mathf.Min(
                maxX,
                cachedBoss.transform.position.x - bossHalfWidth - playerHalfWidth);
        }

        position.x = Mathf.Clamp(
            position.x,
            mapBounds.min.x + playerHalfWidth,
            maxX);
    }

    // 카메라가 맵 밖으로 나가지 않도록 제한
    void ClampCameraPosition(ref Vector3 position)
    {
        if (mapRenderer == null || targetCamera == null || !targetCamera.orthographic)
            return;

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