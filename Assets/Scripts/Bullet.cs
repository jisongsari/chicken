using UnityEngine;

// 총알 클래스: 가장 가까운 적을 향해 발사되는 총알
public class Bullet : MonoBehaviour
{
    // 총알의 이동 속도
    public float speed = 10f;

    // 총알이 자동으로 제거되는 시간
    public float lifeTime = 5f;

    // 총알이 이동할 방향 (기본값은 오른쪽)
    private Vector3 moveDirection = Vector3.right;

    void Start()
    {
        // 생성될 때 가장 가까운 적을 탐색
        Enemy nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            // 가장 가까운 적 방향으로 이동 방향 설정
            moveDirection = (nearestEnemy.transform.position - transform.position).normalized;
        }

        // 일정 시간이 지나면 총알 자동 삭제
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 설정된 방향으로 계속 이동
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    // 현재 씬에서 가장 가까운 Enemy를 찾는 함수
    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Enemy enemy in enemies)
        {
            // 거리 비교를 위해 제곱거리 사용(성능 향상)
            float distance = Vector3.SqrMagnitude(enemy.transform.position - transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        // 가장 가까운 적 반환
        return nearestEnemy;
    }
}