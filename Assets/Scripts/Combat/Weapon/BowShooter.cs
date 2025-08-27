using UnityEngine;

public class BowShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;   // 화살이 나가는 지점
    [SerializeField] private Transform target;      // 맞출 대상 (예: 캐릭터)

    [Header("Shooting")]
    [SerializeField] private float launchAngle = 45f; // 발사 각도 (도 단위)
    [SerializeField] private float fireRate = 1f;

    private float _nextFireTime;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        }
    }

    private void Shoot()
    {
        // 🔹 arrowPool 필드를 통해 Get() 함수를 호출
        if (ArrowPool.Instance == null || target == null) return;

        var arrow = ArrowPool.Instance.Get();
        if (arrow == null) return;

        // 🔹 반드시 발사 위치 / 회전 초기화
        arrow.transform.SetParent(null);
        arrow.transform.position = firePoint.position;
        arrow.transform.rotation = firePoint.rotation;

        // 포물선 속도 계산
        Vector3 velocity = CalculateParabolaVelocity(target.position, firePoint.position, launchAngle);

        // 풀에서 나온 화살은 비활성 상태이므로, 세팅 후 활성화하고 Launch
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

        // 방향 회전 적용
        float angleBetweenObjects = Vector3.SignedAngle(Vector3.forward, (planarTarget - planarStart).normalized, Vector3.up);
        return Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;
    }
}
