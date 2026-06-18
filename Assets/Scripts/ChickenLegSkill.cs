using UnityEngine;

// 닭다리 스킬 클래스: 닭다리 스킬 담당
public class ChickenLegSkill : MonoBehaviour
{
    // 현재 닭다리 스킬이 적을 파괴할 수 있는 상태인지 저장
    private bool canDestroyEnemies;

    // 다른 스크립트에서 스킬 활성화/비활성화를 설정
    public void SetCanDestroyEnemies(bool value)
    {
        canDestroyEnemies = value;
    }

    // Trigger 방식으로 충돌했을 때 호출
    void OnTriggerEnter2D(Collider2D other)
    {
        DestroyEnemy(other.gameObject);
    }

    // Collision 방식으로 충돌했을 때 호출
    void OnCollisionEnter2D(Collision2D collision)
    {
        DestroyEnemy(collision.gameObject);
    }

    // 충돌한 오브젝트가 적인지 확인하고 제거
    void DestroyEnemy(GameObject target)
    {
        // 스킬이 비활성화 상태라면 아무 작업도 하지 않음
        if (!canDestroyEnemies)
        {
            return;
        }

        // 충돌한 오브젝트 또는 부모 오브젝트에서 Enemy 컴포넌트를 찾음
        Enemy enemy = target.GetComponentInParent<Enemy>();

        // Enemy 컴포넌트가 존재하면 해당 적 오브젝트 제거
        if (enemy != null)
        {
            Destroy(enemy.gameObject);
        }
    }
}