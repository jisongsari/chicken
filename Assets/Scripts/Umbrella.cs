using System.Collections.Generic;
using UnityEngine;

// PolygonCollider2D가 반드시 존재하도록 설정
[RequireComponent(typeof(PolygonCollider2D))]

// 우산 클래스: 플레이어가 펼치면 보스 총알을 반사할 수 있는 양심우산
public class Umbrella : MonoBehaviour
{
    [Header("Visual")]
    // 우산의 색상과 외형 설정
    public Color color = Color.cyan;
    public float lineWidth = 0.15f;
    public int arcSegments = 20;

    // 우산의 현재 상태와 크기
    public bool IsOpen { get; private set; }
    public float Radius { get; private set; }

    // 우산이 펼쳐지는 각도(좌우 60도)
    public const float HalfAngleDeg = 60f;

    private PolygonCollider2D polygonCollider;
    private LineRenderer lineRenderer;

    void Awake()
    {
        // 충돌 판정용 PolygonCollider 설정
        polygonCollider = GetComponent<PolygonCollider2D>();
        polygonCollider.isTrigger = true;
        polygonCollider.enabled = false;

        // 우산 모양을 그리기 위한 LineRenderer 생성
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.loop = false;
        lineRenderer.sortingOrder = 10;
        lineRenderer.enabled = false;
    }

    // 우산의 크기에 맞게 충돌 영역과 외형 생성
    public void Initialize(float radius)
    {
        Radius = radius;

        BuildCollider(radius);
        BuildArc(radius);
    }

    // 우산 펼치기
    public void Open()
    {
        IsOpen = true;

        polygonCollider.enabled = true;
        lineRenderer.enabled = true;
    }

    // 우산 접기
    public void Close()
    {
        IsOpen = false;

        polygonCollider.enabled = false;
        lineRenderer.enabled = false;
    }

    // 부채꼴 모양의 충돌 영역 생성
    void BuildCollider(float radius)
    {
        var points = new List<Vector2> { Vector2.zero };

        for (int i = 0; i <= arcSegments; i++)
        {
            float angle =
                Mathf.Lerp(-HalfAngleDeg, HalfAngleDeg, i / (float)arcSegments)
                * Mathf.Deg2Rad;

            points.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }

        polygonCollider.SetPath(0, points.ToArray());
    }

    // LineRenderer를 이용해 우산의 호를 그림
    void BuildArc(float radius)
    {
        lineRenderer.positionCount = arcSegments + 1;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        for (int i = 0; i <= arcSegments; i++)
        {
            float angle =
                Mathf.Lerp(-HalfAngleDeg, HalfAngleDeg, i / (float)arcSegments)
                * Mathf.Deg2Rad;

            lineRenderer.SetPosition(
                i,
                new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius
            );
        }
    }

    // 보스 탄환과 충돌하면 반사
    void OnTriggerEnter2D(Collider2D other)
    {
        BossBullet bullet = other.GetComponent<BossBullet>();

        if (bullet == null)
            return;

        // 충돌 지점의 법선 벡터를 계산하여 탄환 반사
        Vector2 normal =
            ((Vector2)other.transform.position - (Vector2)transform.position).normalized;

        bullet.ReflectOff(normal);
    }
}