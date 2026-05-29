using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 5f;

    private Vector3 moveDirection = Vector3.right;

    void Start()
    {
        Enemy nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            moveDirection = (nearestEnemy.transform.position - transform.position).normalized;
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
        
    }

    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.SqrMagnitude(enemy.transform.position - transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }
}
