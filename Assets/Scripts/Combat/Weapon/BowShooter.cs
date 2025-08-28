using UnityEngine;

public class BowShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;   // 화살이 나가는 지점
    [SerializeField] private Transform target;      // 맞출 대상 (예: 캐릭터)

    [Header("Shooting")]
    [SerializeField] private float launchAngle = 45f; // 발사 각도 (도 단위)

    /* Animation Event */
    public void Shoot()
    {
        if (ArrowPool.Instance == null || target == null) return;

        var arrow = ArrowPool.Instance.Get();
        if (arrow == null) return;

        // 화살 위치 초기화
        arrow.transform.SetParent(null);
        arrow.transform.position = firePoint.position;
        arrow.transform.rotation = firePoint.rotation;

        Vector3 velocity = CalculateParabolaVelocity(target.position, firePoint.position, launchAngle);

        arrow.gameObject.SetActive(true);
        arrow.Launch(velocity.normalized, velocity.magnitude - arrow.BaseSpeed);
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

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
