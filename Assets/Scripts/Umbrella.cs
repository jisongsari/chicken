using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class Umbrella : MonoBehaviour
{
    [Header("Visual")]
    public Color color = Color.cyan;
    public float lineWidth = 0.15f;
    public int arcSegments = 20;

    public bool IsOpen { get; private set; }
    public float Radius { get; private set; }
    public const float HalfAngleDeg = 60f;

    private PolygonCollider2D polygonCollider;
    private LineRenderer lineRenderer;

    void Awake()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();
        polygonCollider.isTrigger = true;
        polygonCollider.enabled = false;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.loop = false;
        lineRenderer.sortingOrder = 10;
        lineRenderer.enabled = false;
    }

    public void Initialize(float radius)
    {
        Radius = radius;
        BuildCollider(radius);
        BuildArc(radius);
    }

    public void Open()
    {
        IsOpen = true;
        polygonCollider.enabled = true;
        lineRenderer.enabled = true;
    }

    public void Close()
    {
        IsOpen = false;
        polygonCollider.enabled = false;
        lineRenderer.enabled = false;
    }

    void BuildCollider(float radius)
    {
        var points = new List<Vector2> { Vector2.zero };
        for (int i = 0; i <= arcSegments; i++)
        {
            float angle = Mathf.Lerp(-HalfAngleDeg, HalfAngleDeg, i / (float)arcSegments) * Mathf.Deg2Rad;
            points.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
        polygonCollider.SetPath(0, points.ToArray());
    }

    void BuildArc(float radius)
    {
        lineRenderer.positionCount = arcSegments + 1;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        for (int i = 0; i <= arcSegments; i++)
        {
            float angle = Mathf.Lerp(-HalfAngleDeg, HalfAngleDeg, i / (float)arcSegments) * Mathf.Deg2Rad;
            lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        BossBullet bullet = other.GetComponent<BossBullet>();
        if (bullet == null) return;

        Vector2 normal = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
        bullet.ReflectOff(normal);
    }
}
