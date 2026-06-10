using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform yudam;
    public float moveSpeed = 5f;
    void OnTriggerEnter2D(Collider2D other)
    {
        Bullet bullet = other.GetComponent<Bullet>();

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Bullet bullet = collision.gameObject.GetComponent<Bullet>();

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
            Destroy(gameObject);
        }
    }

    void follow()
    {
        transform.position = Vector3.MoveTowards(transform.position, yudam.position, moveSpeed * Time.deltaTime);
    }
    void Update()
    {
        follow();
    }
}
