using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject yudam;
    public float moveSpeed = 5f;

    private bool hitYudam;

    void OnTriggerEnter2D(Collider2D other)
    {
        Bullet bullet = other.GetComponent<Bullet>();

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
            Destroy(gameObject);
            return;
        }

        HitYudam(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Bullet bullet = collision.gameObject.GetComponent<Bullet>();

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
            Destroy(gameObject);
            return;
        }

        HitYudam(collision.gameObject);
    }

    void follow()
    {
        if (yudam == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, yudam.transform.position, moveSpeed * Time.deltaTime);
    }
    void Update()
    {
        follow();
    }

    void HitYudam(GameObject target)
    {
        if (hitYudam) return;

        // 양심우산은 유담의 일부로 인식하지 않음
        if (target.GetComponent<Umbrella>() != null) return;

        bool isYudam = target == yudam;

        if (!isYudam && yudam != null)
        {
            isYudam = target.transform.IsChildOf(yudam.transform);
        }

        Move yudamMove = target.GetComponentInParent<Move>();

        if (!isYudam && yudamMove != null)
        {
            isYudam = yudam == null || yudamMove.gameObject == yudam;
        }

        if (!isYudam)
        {
            return;
        }

        if (yudamMove != null)
        {
            yudamMove.TakeDamage(1);
        }

        hitYudam = true;
        Destroy(gameObject);
    }
}
