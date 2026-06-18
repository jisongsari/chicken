using UnityEngine;


// 적 클래스: 플레이어를 따라다니며 충돌 시 데미지를 주는 적
public class Enemy : MonoBehaviour
{
    // 플레이어(유담) 오브젝트
    public GameObject yudam;

    // 적의 이동 속도
    public float moveSpeed = 5f;

    // 플레이어와 중복 충돌하는 것을 방지
    private bool hitYudam;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 총알과 충돌하면 총알과 적을 모두 제거
        Bullet bullet = other.GetComponent<Bullet>();

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
            Destroy(gameObject);
            return;
        }

        // 플레이어 충돌 여부 확인
        HitYudam(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Collision 방식에서도 동일하게 총알 충돌 처리
        Bullet bullet = collision.gameObject.GetComponent<Bullet>();

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
            Destroy(gameObject);
            return;
        }

        HitYudam(collision.gameObject);
    }

    // 플레이어를 향해 이동
    void follow()
    {
        if (yudam == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            yudam.transform.position,
            moveSpeed * Time.deltaTime);
    }

    void Update()
    {
        follow();
    }

    // 플레이어와 충돌했을 때 데미지 처리
    void HitYudam(GameObject target)
    {
        // 이미 충돌 처리한 경우 중복 실행 방지
        if (hitYudam) return;

        // 양심우산은 플레이어 충돌 판정에서 제외
        if (target.GetComponent<Umbrella>() != null) return;

        bool isYudam = target == yudam;

        // 플레이어의 자식 오브젝트와 충돌한 경우도 플레이어로 판정
        if (!isYudam && yudam != null)
        {
            isYudam = target.transform.IsChildOf(yudam.transform);
        }

        // 부모 오브젝트에서 Move 컴포넌트를 탐색
        Move yudamMove = target.GetComponentInParent<Move>();

        if (!isYudam && yudamMove != null)
        {
            isYudam = yudam == null || yudamMove.gameObject == yudam;
        }

        // 플레이어가 아니라면 처리하지 않음
        if (!isYudam)
        {
            return;
        }

        // 플레이어에게 데미지 적용
        if (yudamMove != null)
        {
            yudamMove.TakeDamage(1);
        }

        // 중복 충돌 방지 후 적 제거
        hitYudam = true;
        Destroy(gameObject);
    }
}