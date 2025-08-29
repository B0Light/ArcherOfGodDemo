using System;
using System.Collections;
using UnityEngine;

public class CharacterCombatManager : MonoBehaviour
{
    private CharacterManager _characterManager;

    [SerializeField] private BowShooter bowShooter;
    [SerializeField] private Transform firePoint;

    private void Awake()
    {
        _characterManager = GetComponent<CharacterManager>();
    }

    private void OnEnable()
    {
        bowShooter.shootArrow.AddListener(AddActionPoint);
    }

    public void UpdateBowAim()
    {
        if (_characterManager == null || bowShooter == null) return;
        if (_characterManager.characterLocomotionManager == null) return;

        Transform target = _characterManager.GetTarget();
        if (target == null) return;

        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetPos = target.position;

        float desiredAngle = CalculateLaunchAngleForTarget(startPos, targetPos);

        bowShooter.SetLaunchAngle(desiredAngle);
        bowShooter.SetBow(target, firePoint);
    }

    private float CalculateLaunchAngleForTarget(Vector3 startPos, Vector3 targetPos)
    {
        // 플랫폼 기준 정보 활용
        CharacterLocomotionManager locomotion = _characterManager.characterLocomotionManager;
        float distanceFromCenter = locomotion.GetDistanceFromPlatformCenter();
        Vector3 offsetFromCenter = locomotion.GetOffsetFromPlatformCenter();

        // 수평 거리/고도차
        Vector3 planarStart = new Vector3(startPos.x, 0f, startPos.z);
        Vector3 planarTarget = new Vector3(targetPos.x, 0f, targetPos.z);
        float distance = Vector3.Distance(planarStart, planarTarget);
        float yOffset = targetPos.y - startPos.y;

        // 기본 각도 범위
        float minAngle = 15f;
        float maxAngle = 65f;

        // 플랫폼 중심에서 멀수록 더 낮은 각도를 선호(직선 사격 비중↑)
        // 오프셋 방향이 타겟과 같은 쪽이면 각도 소폭 낮춤
        float centerBias = Mathf.Clamp01(distanceFromCenter / 3f);
        float directionBias = 0f;
        Vector3 toTargetPlanar = (planarTarget - planarStart).normalized;
        Vector3 offsetPlanar = new Vector3(offsetFromCenter.x, 0f, offsetFromCenter.z).normalized;
        if (offsetPlanar.sqrMagnitude > 0.0001f)
        {
            directionBias = Mathf.Clamp01(Vector3.Dot(offsetPlanar, toTargetPlanar)) * 0.25f; // 최대 25% 축소
        }

        float baseAngle = Mathf.Lerp(maxAngle, minAngle, Mathf.Clamp01(centerBias + directionBias));

        // 고도차 보정: 타겟이 높으면 각도↑, 낮으면 각도↓
        float heightAdjust = Mathf.Clamp(yOffset * 2f, -10f, 10f);
        float candidateAngle = Mathf.Clamp(baseAngle + heightAdjust, minAngle, maxAngle);

        // 물리적으로 도달 가능한 각도 보정: 사거리 공식에서 유효한 각도 찾기
        float solvedAngle = SolveBallisticAngle(distance, yOffset, candidateAngle);
        return Mathf.Clamp(solvedAngle, minAngle, maxAngle);
    }

    private float SolveBallisticAngle(float distance, float yOffset, float initialGuess)
    {
        // BowShooter의 CalculateParabolaVelocity는 angle과 중력으로 속도를 산출
        float gravity = Mathf.Abs(Physics.gravity.y);
        float bestAngle = initialGuess;
        float bestScore = float.MaxValue;

        // 소범위 스윕으로 가장 낮은 초기속도(=스무스한 궤적)를 주는 각도를 선택
        for (int i = -10; i <= 10; i++)
        {
            float angle = Mathf.Clamp(initialGuess + i, 10f, 80f);
            float rad = angle * Mathf.Deg2Rad;
            float denom = distance * Mathf.Tan(rad) - yOffset;
            if (denom <= 0.001f) continue;
            float v = (1f / Mathf.Cos(rad)) * Mathf.Sqrt((0.5f * gravity * distance * distance) / denom);
            if (float.IsNaN(v) || float.IsInfinity(v)) continue;
            float score = v; // 낮을수록 좋다
            if (score < bestScore)
            {
                bestScore = score;
                bestAngle = angle;
            }
        }
        return bestAngle;
    }
    
    /* Animation Event */
    public void ShootArrow()
    {
        bowShooter.Shoot();
    }

    private void AddActionPoint()
    {
        _characterManager.actionPoint += 1;
    }
}