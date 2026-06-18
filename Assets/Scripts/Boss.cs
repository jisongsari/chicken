using UnityEngine;
using UnityEngine.UI;


/// 보스 클래스: 플레이어를 따라다니며 지속적으로 총알을 발사하는 보스
public class Boss : MonoBehaviour 
{
    [Header("References")]
    public Transform player;
    public SpriteRenderer mapRenderer;
    public Slider healthSlider;
    public GameObject bossBulletPrefab;
    public Camera targetCamera;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Combat")]
    public int maxHealth = 5;
    public float fireInterval = 1f;
    [Tooltip("총알이 퍼지는 반각 (°). 180° 기준 좌우로 이만큼 랜덤")]
    public float spreadHalfAngle = 80f;
    public float bulletSpawnOffsetX = -0.8f;
    [Tooltip("총알 발사 위치 상하 랜덤 범위 (±이 값 사이에서 랜덤)")]
    public float bulletSpawnRangeY = 1f;

    [Header("Camera Shake")]
    public float shakeAmount = 0.05f;
    public float shakeFrequency = 25f;

    public static Vector3 CameraShakeOffset { get; private set; }

    private int currentHealth;
    private float fireTimer;
    private float fixedX;
    private Transform bulletSpawn;
    private bool hasBeenSeen;
    private Renderer bossRenderer;

    /// 게임 시작 시 호출 - 정적 카메라 쉐이크 오프셋 초기화
    void Awake()
    {
        CameraShakeOffset = Vector3.zero;
    }

    // 초기 설정: 보스의 위치, 체력, UI, 총알 생성 포인트 설정
    void Start()
    {
        // 보스의 X 좌표는 고정 (Y축으로만 플레이어를 따라다님)
        fixedX = transform.position.x;
        currentHealth = maxHealth;
        bossRenderer = GetComponent<Renderer>();

        // UI 슬라이더 초기화
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        // 총알 생성 포인트 설정 (보스 왼쪽)
        GameObject spawnObj = new GameObject("BossBulletSpawn");
        spawnObj.transform.SetParent(transform);
        spawnObj.transform.localPosition = new Vector3(bulletSpawnOffsetX, 0f, 0f);
        bulletSpawn = spawnObj.transform;
    }

    // 매 프레임 호출 - 플레이어 추적, 공격, 카메라 흔들림 처리
    void Update()
    {
        // 플레이어의 Y 위치를 따라다님
        FollowPlayerY();

        // 보스가 카메라 시야에 들어왔는지 확인 (한 번만 true로 변함)
        if (!hasBeenSeen && IsPartiallyInCamera())
            hasBeenSeen = true;

        // 카메라에 보인 이후부터 공격과 카메라 흔들림 시작
        if (hasBeenSeen)
        {
            FireUpdate();
            UpdateCameraShake();
        }
    }

    // 플레이어의 Y 위치를 따라다님 (맵 경계 내에서만 이동)
    void FollowPlayerY()
    {
        if (player == null) return;

        float targetY = player.position.y;

        // 맵 경계 내로 이동 제한
        if (mapRenderer != null)
        {
            Bounds mapBounds = mapRenderer.bounds;
            float halfH = bossRenderer != null ? bossRenderer.bounds.extents.y : 0f;
            targetY = Mathf.Clamp(targetY, mapBounds.min.y + halfH, mapBounds.max.y - halfH);
        }

        // X 좌표는 고정, Y 좌표는 moveSpeed 속도로 부드럽게 이동
        Vector3 pos = transform.position;
        pos.x = fixedX;
        pos.y = Mathf.MoveTowards(pos.y, targetY, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    // 총알 발사 타이머 관리 - fireInterval 간격으로 FireBullet() 호출
    void FireUpdate()
    {
        if (bossBulletPrefab == null) return;

        // 타이머 증가 및 발사 간격 체크
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            FireBullet();
        }
    }

    // 보스 총알 생성 - 플레이어 반대 방향(180도)을 중심으로 퍼진 각도로 발사
    void FireBullet()
    {
        // 180도 ± spreadHalfAngle 범위의 랜덤 각도 계산
        float angle = Random.Range(180f - spreadHalfAngle, 180f + spreadHalfAngle) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        // 총알 생성 위치 설정 (Y축에 약간의 랜덤성 추가)
        Vector3 spawnPos = bulletSpawn != null ? bulletSpawn.position : transform.position;
        spawnPos.y += Random.Range(-bulletSpawnRangeY, bulletSpawnRangeY);
        GameObject bulletObj = Instantiate(bossBulletPrefab, spawnPos, Quaternion.identity);
        BossBullet bullet = bulletObj.GetComponent<BossBullet>();
        if (bullet != null) bullet.Initialize(dir);
    }

    // 데미지 처리 - 체력 감소 및 UI 업데이트, 체력이 0 이하면 보스 제거
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        
        // UI 슬라이더 업데이트
        if (healthSlider != null) healthSlider.value = currentHealth;
        
        // 체력이 0 이하면 보스 파괴 및 카메라 흔들림 중지
        if (currentHealth <= 0)
        {
            CameraShakeOffset = Vector3.zero;
            Destroy(gameObject);
        }
    }

    // 충돌 감지 - 보스와 플레이어가 접촉하면 게임오버 처리
    // 플레이어의 총알이나 보스의 총알은 무시
    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 총알과 보스 총알은 처리하지 않음
        if (other.GetComponent<Bullet>() != null) return;
        if (other.GetComponent<BossBullet>() != null) return;

        // 플레이어(Move 컴포넌트 소유)와 접촉 시 게임오버
        if (other.GetComponentInParent<Move>() != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    // 보스가 카메라 시야에 부분적으로라도 보이는지 확인
    // Frustum Culling을 이용한 카메라 시야 범위 체크
    bool IsPartiallyInCamera()
    {
        if (targetCamera == null || bossRenderer == null) return false;
        
        // 카메라의 시야 평면 계산
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
        // 보스의 바운드박스가 시야 평면과 교차하는지 확인
        return GeometryUtility.TestPlanesAABB(planes, bossRenderer.bounds);
    }

    // 카메라 흔들림 효과 계산 및 적용
    // 보스가 시야에 보인 이후부터 보스가 죽을 때까지 지속적으로 카메라를 흔듦
    void UpdateCameraShake()
    {
        // 사인 함수를 이용한 부드러운 흔들림 효과 생성
        // X와 Y축에 다른 주파수를 사용해 자연스러운 진동 표현
        float x = Mathf.Sin(Time.time * shakeFrequency) * shakeAmount;
        float y = Mathf.Sin(Time.time * shakeFrequency * 1.3f + 1.2f) * shakeAmount;
        CameraShakeOffset = new Vector3(x, y, 0f);
    }

    // 보스 파괴 시 호출 - 카메라 흔들림 효과 제거
    void OnDestroy()
    {
        CameraShakeOffset = Vector3.zero;
    }
}
