using UnityEngine;

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

    void Awake()
    {
        // Rigidbody2D 자동 설정 (프리팹에 없어도 동작)
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // 트리거 콜라이더 자동 설정 (프리팹에 없어도 동작)
        if (GetComponent<Collider2D>() == null)
        {
            var circle = gameObject.AddComponent<CircleCollider2D>();
            circle.radius = 0.2f;
            circle.isTrigger = true;
        }
    }

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (deflectionTimer > 0f) deflectionTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }

    public void ReflectOff(Vector2 surfaceNormal)
    {
        if (deflectionTimer > 0f) return;
        moveDirection = Vector2.Reflect(moveDirection, surfaceNormal).normalized;
        isDeflected = true;
        deflectionTimer = deflectionCooldown;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Bullet>() != null) return;
        if (other.GetComponent<ChickenLegSkill>() != null) return;
        if (other.GetComponent<Umbrella>() != null) return;

        Boss boss = other.GetComponentInParent<Boss>();
        if (boss != null)
        {
            if (isDeflected)
            {
                boss.TakeDamage(1);
                Destroy(gameObject);
            }
            return;
        }

        if (other.GetComponentInParent<Move>() != null)
        {
            GameManager.Instance.GameOver();
            Destroy(gameObject);
        }
    }
}
