using UnityEngine;
using UnityEngine.UI;

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

    void Awake()
    {
        CameraShakeOffset = Vector3.zero;
    }

    void Start()
    {
        fixedX = transform.position.x;
        currentHealth = maxHealth;
        bossRenderer = GetComponent<Renderer>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        GameObject spawnObj = new GameObject("BossBulletSpawn");
        spawnObj.transform.SetParent(transform);
        spawnObj.transform.localPosition = new Vector3(bulletSpawnOffsetX, 0f, 0f);
        bulletSpawn = spawnObj.transform;
    }

    void Update()
    {
        FollowPlayerY();

        if (!hasBeenSeen && IsPartiallyInCamera())
            hasBeenSeen = true;

        if (hasBeenSeen)
        {
            FireUpdate();
            UpdateCameraShake();
        }
    }

    void FollowPlayerY()
    {
        if (player == null) return;

        float targetY = player.position.y;

        if (mapRenderer != null)
        {
            Bounds mapBounds = mapRenderer.bounds;
            float halfH = bossRenderer != null ? bossRenderer.bounds.extents.y : 0f;
            targetY = Mathf.Clamp(targetY, mapBounds.min.y + halfH, mapBounds.max.y - halfH);
        }

        Vector3 pos = transform.position;
        pos.x = fixedX;
        pos.y = Mathf.MoveTowards(pos.y, targetY, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    void FireUpdate()
    {
        if (bossBulletPrefab == null) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            FireBullet();
        }
    }

    void FireBullet()
    {
        float angle = Random.Range(180f - spreadHalfAngle, 180f + spreadHalfAngle) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        Vector3 spawnPos = bulletSpawn != null ? bulletSpawn.position : transform.position;
        spawnPos.y += Random.Range(-bulletSpawnRangeY, bulletSpawnRangeY);
        GameObject bulletObj = Instantiate(bossBulletPrefab, spawnPos, Quaternion.identity);
        BossBullet bullet = bulletObj.GetComponent<BossBullet>();
        if (bullet != null) bullet.Initialize(dir);
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (currentHealth <= 0)
        {
            CameraShakeOffset = Vector3.zero;
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 접촉 → 즉시 게임오버
        if (other.GetComponent<Bullet>() != null) return;
        if (other.GetComponent<BossBullet>() != null) return;

        if (other.GetComponentInParent<Move>() != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    bool IsPartiallyInCamera()
    {
        if (targetCamera == null || bossRenderer == null) return false;
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
        return GeometryUtility.TestPlanesAABB(planes, bossRenderer.bounds);
    }

    void UpdateCameraShake()
    {
        // 한 번 시야에 들어온 뒤로는 보스가 죽을 때까지 계속 흔들림
        float x = Mathf.Sin(Time.time * shakeFrequency) * shakeAmount;
        float y = Mathf.Sin(Time.time * shakeFrequency * 1.3f + 1.2f) * shakeAmount;
        CameraShakeOffset = new Vector3(x, y, 0f);
    }

    void OnDestroy()
    {
        CameraShakeOffset = Vector3.zero;
    }
}
