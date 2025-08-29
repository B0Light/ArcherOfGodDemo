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

    private CharacterManager _characterManager;

    private void Awake()
    {
        _characterManager = GetComponentInParent<CharacterManager>();
    }

    public void SetBow(Transform newTarget, Transform muzzle)
    {
        _target = newTarget;
        _firePoint = muzzle;
    }
    
    public void Shoot()
    {
        if (_characterManager != null && _characterManager.isDead.Value) return;
        if (ArrowPool.Instance == null || _target == null || _firePoint == null) return;
        var targetCm = _target.GetComponent<CharacterManager>();
        if (targetCm != null && targetCm.isDead.Value) return;
        _launchAngle = SolveBestAngle(_firePoint.position, _target.position, _launchAngle);
        Vector3 velocity = CalculateParabolaVelocity(_target.position, _firePoint.position, _launchAngle);
        var arrow = CreateArrowAtMuzzle();
        if (arrow == null) return;
        LaunchArrow(arrow, velocity);
    }

    public Vector3 GetCalculatedVelocity()
    {
        if (_target == null || _firePoint == null) return Vector3.zero;
        _launchAngle = SolveBestAngle(_firePoint.position, _target.position, _launchAngle);
        return CalculateParabolaVelocity(_target.position, _firePoint.position, _launchAngle);
    }

    public Arrow CreateArrowAtMuzzle()
    {
        if (ArrowPool.Instance == null || _firePoint == null) return null;
        var arrow = ArrowPool.Instance.Get();
        if (arrow == null) return null;
        arrow.transform.SetParent(_firePoint);
        arrow.transform.localPosition = Vector3.zero;
        arrow.transform.localRotation = Quaternion.identity;
        arrow.transform.SetParent(null);
        return arrow;
    }

    public void LaunchArrow(Arrow arrow, Vector3 velocity)
    {
        if (arrow == null) return;
        if (_characterManager != null && _characterManager.isDead.Value) return;
        arrow.gameObject.SetActive(true);
        arrow.Launch(
            velocity.normalized,
            velocity.magnitude - arrow.BaseSpeed,
            _target,
            _target != null ? _target.position : arrow.transform.position + velocity * 0.1f,
            _firePoint != null ? _firePoint.position : arrow.transform.position
        );
        shootArrow?.Invoke();
    }

    public void ShootMulti(int count, float totalSpreadDegrees)
    {
        if (count <= 0) return;
        if (_characterManager != null && _characterManager.isDead.Value) return;
        if (ArrowPool.Instance == null || _target == null || _firePoint == null) return;
        Vector3 baseVelocity = GetCalculatedVelocity();
        if (baseVelocity == Vector3.zero) return;

        if (count == 1)
        {
            var single = CreateArrowAtMuzzle();
            LaunchArrow(single, baseVelocity);
            return;
        }

        float step = (count > 1) ? (totalSpreadDegrees / (count - 1)) : 0f;
        float start = -totalSpreadDegrees * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float yaw = start + step * i;
            Vector3 v = Quaternion.AngleAxis(yaw, Vector3.up) * baseVelocity;
            var arrow = CreateArrowAtMuzzle();
            LaunchArrow(arrow, v);
        }
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
