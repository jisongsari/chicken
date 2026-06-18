using UnityEngine;

// 보스 총알 클래스: 플레이어의 양심우산에 반사되어 보스에 대미지를 줄 수 있다
public class BossBullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 8f;
    public float lifeTime = 10f;
    public float deflectionCooldown = 0.15f;

    private Vector2 moveDirection;
    private bool isDeflected;
    private float deflectionTimer;
    private Rigidbody2D rb;

    // 초기 설정: Rigidbody2D와 Collider2D 자동 설정
    void Awake()
    {
        // Rigidbody2D 자동 설정 (프리팹에 없어도 동작)
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // 트리거 collider 자동 설정 (프리팹에 없어도 동작)
        if (GetComponent<Collider2D>() == null)
        {
            var circle = gameObject.AddComponent<CircleCollider2D>();
            circle.radius = 0.2f;
            circle.isTrigger = true;
        }
    }

    // 초기화: 총알의 발사 방향 설정 및 제거 시간 설정
    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;
        Destroy(gameObject, lifeTime); // lifeTime 초 후 화면 밖으로 나간 총알은 제거
    }

    // 디플렉션 타이머 감소 - 반사 중일때 타이머가 0이 될 때까지 다시 반사되지 않도록 처리
    void Update()
    {
        if (deflectionTimer > 0f) deflectionTimer -= Time.deltaTime;
    }

    // 총알 이동 처리 - 발사 방향으로 speed 속도로 이동
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }

    // 총알 반사 처리 - 표면 법선 기준으로 반사 방향 업데이트
    public void ReflectOff(Vector2 surfaceNormal)
    {
        if (deflectionTimer > 0f) return; // 쿨다운 중이면 다시 반사되지 않음
        moveDirection = Vector2.Reflect(moveDirection, surfaceNormal).normalized;
        isDeflected = true; // 반사된 총알 표시
        deflectionTimer = deflectionCooldown; // 반사 쿨다운 시작
    }

    // 충돌 감지 - 총알 충돌 처리 (보스에 대미지는 반사된 총알만) 및 플레이어 접촉 처리
    void OnTriggerEnter2D(Collider2D other)
    {
        // 총알, 닭다리, 양심우산 스킬은 무시
        if (other.GetComponent<Bullet>() != null) return;
        if (other.GetComponent<ChickenLegSkill>() != null) return;
        if (other.GetComponent<Umbrella>() != null) return;

        // 보스에 접촉한 경우, 반사된 총알만 데미지 주기
        Boss boss = other.GetComponentInParent<Boss>();
        if (boss != null)
        {
            if (isDeflected)
            {
                boss.TakeDamage(1); // 반사된 총알만 보스에 데미지
                Destroy(gameObject);
            }
            return;
        }

        // 플레이어에 접촉한 경우 게임오버
        if (other.GetComponentInParent<Move>() != null)
        {
            GameManager.Instance.GameOver();
            Destroy(gameObject);
        }
    }
}
