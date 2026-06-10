using UnityEngine;

public class ChickenLegSkill : MonoBehaviour
{
    private bool canDestroyEnemies;

    public void SetCanDestroyEnemies(bool value)
    {
        canDestroyEnemies = value;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        DestroyEnemy(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        DestroyEnemy(collision.gameObject);
    }

    void DestroyEnemy(GameObject target)
    {
        if (!canDestroyEnemies)
        {
            return;
        }

        Enemy enemy = target.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            Destroy(enemy.gameObject);
        }
    }
}
