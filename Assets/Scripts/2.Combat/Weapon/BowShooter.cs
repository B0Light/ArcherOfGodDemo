using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class BowShooter : MonoBehaviour
{
    [Header("Refs")]
    private Transform _firePoint;   // 화살이 나가는 지점
    private Transform _target;      
    private float _launchAngle = 45f; // 발사 각도 (도 단위)

    public UnityEvent shootArrow;

    public void SetBow(Transform newTarget, Transform muzzle)
    {
        _target = newTarget;
        _firePoint = muzzle;
    }
    
    public void Shoot()
    {
        if (ArrowPool.Instance == null || _target == null || _firePoint == null) return;

        var arrow = ArrowPool.Instance.Get();
        if (arrow == null) return;
        
        // 발사 직전 현재 위치/타겟 기준으로 최적 각도 재계산 (가장자리 보정)
        _launchAngle = SolveBestAngle(_firePoint.position, _target.position, _launchAngle);

        // 화살 위치 초기화
        arrow.transform.SetParent(_firePoint);
        arrow.transform.localPosition = Vector3.zero;
        arrow.transform.localRotation = Quaternion.identity;
        arrow.transform.SetParent(null);

        Vector3 velocity = CalculateParabolaVelocity(_target.position, _firePoint.position, _launchAngle);

        arrow.gameObject.SetActive(true);
        arrow.Launch(
            velocity.normalized,
            velocity.magnitude - arrow.BaseSpeed,
            _target,
            _target.position,
            _firePoint.position
        );
        shootArrow?.Invoke();
    }

    private Vector3 CalculateParabolaVelocity(Vector3 targetPos, Vector3 startPos, float angle)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        float radianAngle = angle * Mathf.Deg2Rad;

        Vector3 planarTarget = new Vector3(targetPos.x, 0, targetPos.z);
        Vector3 planarStart = new Vector3(startPos.x, 0, startPos.z);

        float distance = Vector3.Distance(planarTarget, planarStart);
        float yOffset = startPos.y - targetPos.y;

        float initialVelocity =
            (1 / Mathf.Cos(radianAngle)) *
            Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) /
            (distance * Mathf.Tan(radianAngle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(radianAngle), initialVelocity * Mathf.Cos(radianAngle));

        float angleBetweenObjects = Vector3.SignedAngle(Vector3.forward, (planarTarget - planarStart).normalized, Vector3.up);
        return Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;
    }

    public void SetLaunchAngle(float newAngle)
    {
        _launchAngle = newAngle;
    }

    private float SolveBestAngle(Vector3 startPos, Vector3 targetPos, float initialGuess)
    {
        // BowShooter.CalculateParabolaVelocity는 yOffset = start - target을 사용
        float gravity = Mathf.Abs(Physics.gravity.y);
        Vector3 planarStart = new Vector3(startPos.x, 0f, startPos.z);
        Vector3 planarTarget = new Vector3(targetPos.x, 0f, targetPos.z);
        float distance = Vector3.Distance(planarStart, planarTarget);
        float yOffset = startPos.y - targetPos.y;

        float bestAngle = Mathf.Clamp(initialGuess, 10f, 80f);
        float bestScore = float.MaxValue;

        for (int i = -10; i <= 10; i++)
        {
            float angle = Mathf.Clamp(bestAngle + i, 10f, 80f);
            float rad = angle * Mathf.Deg2Rad;
            float denom = (distance * Mathf.Tan(rad)) + yOffset;
            if (denom <= 0.001f) continue;
            float v = (1f / Mathf.Cos(rad)) * Mathf.Sqrt((0.5f * gravity * distance * distance) / denom);
            if (float.IsNaN(v) || float.IsInfinity(v)) continue;
            if (v < bestScore)
            {
                bestScore = v;
                bestAngle = angle;
            }
        }

        return bestAngle;
    }
}
