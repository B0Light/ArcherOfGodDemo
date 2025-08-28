using System.Collections.Generic;
using UnityEngine;

public class AILocomotionManager : CharacterLocomotionManager
{
    [Header("Dodge Settings")]
    [SerializeField] private float detectionRadius = 12f;
    [SerializeField] private float predictionTime = 0.6f;
    [SerializeField] private float minSeparationDistance = 1.0f;
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float dodgeSpeed = 0.9f; // 0~1 비율
    [SerializeField] private float rapidAttackChance = 0.25f;
    [SerializeField] private float postDodgeCooldown = 2f;

    private bool _dodging;
    private float _dodgeEndTime;
    private float _cooldownEndTime;
    private Vector3 _currentDodgeDir;

    private int _extraAttack = 1;

    protected override void Update()
    {
        HandleAIMovement();
        base.Update();
    }

    private void HandleAIMovement()
    {
        // 진행 중인 회피 유지
        if (_dodging)
        {
            if (Time.time < _dodgeEndTime)
            {
                Vector3 clampedDodge = ClampDirectionToPlatform(_currentDodgeDir);
                SetMoveDirection(clampedDodge);
                SetMovementSpeed(dodgeSpeed);
                return;
            }
            _dodging = false;
            _extraAttack = 1;
            if (Random.value < rapidAttackChance) _extraAttack = 2;
            _cooldownEndTime = Time.time + postDodgeCooldown * _extraAttack;
        }

        // 쿨다운 동안 정지
        if (Time.time < _cooldownEndTime)
        {
            SetMoveDirection(Vector3.zero);
            SetMovementSpeed(0f);
            return;
        }

        // 위협 탐지 및 회피 방향 계산
        if (TryComputeDodge(out Vector3 dodgeDir))
        {
            _currentDodgeDir = dodgeDir;
            _dodging = true;
            _dodgeEndTime = Time.time + dodgeDuration;
            Vector3 clampedDodge = ClampDirectionToPlatform(_currentDodgeDir);
            SetMoveDirection(clampedDodge);
            SetMovementSpeed(dodgeSpeed);
            return;
        }

        // 기본: 대기
        SetMoveDirection(Vector3.zero);
        SetMovementSpeed(0f);
    }

    private bool TryComputeDodge(out Vector3 dodgeDir)
    {
        dodgeDir = Vector3.zero;

        List<Arrow> arrowList = ArrowPool.Instance.GetInstanceList();
        if (arrowList == null || arrowList.Count == 0) return false;

        float bestScore = float.NegativeInfinity;
        Vector3 bestDodge = Vector3.zero;

        Vector3 myPos = transform.position + Vector3.up;
        Vector3 myPos2D = new Vector3(myPos.x, 0f, myPos.z);

        foreach (Arrow arrow in arrowList)
        {
            if (arrow == null || !arrow.gameObject.activeInHierarchy || arrow.IntendedTarget.gameObject != this.gameObject) continue;

            // y축으로는 회피를 못하니 2d로 계산 
            Vector3 arrowPos2D = new Vector3(arrow.transform.position.x, 0f, arrow.transform.position.z);
            Vector3 arrowVel2D = new Vector3(arrow.transform.forward.x, 0f, arrow.transform.forward.z) * arrow.BaseSpeed;

            // 아직 먼 화살이거나 땅에 박힌 화살은 무시 
            if ((arrowPos2D - myPos2D).sqrMagnitude > detectionRadius * detectionRadius ||
                arrowVel2D.sqrMagnitude < 0.0001f) continue;
            
            Vector3 relativePos = arrowPos2D - myPos2D;
            float timeToClosest = -Vector3.Dot(relativePos, arrowVel2D) / arrowVel2D.sqrMagnitude;

            // 멀어지는 화살 무시
            if (timeToClosest < 0 || timeToClosest > predictionTime) continue;
            
            Vector3 closestPoint = relativePos + arrowVel2D * timeToClosest;
            float separation = closestPoint.magnitude;
            // 안맞을 것 같은 화살 무시
            if (separation > minSeparationDistance) continue;

            // 회피 방향 계산 : 화살 속도에 수직
            Vector3 arrowDir = arrowVel2D.normalized;
            Vector3 perp1 = new Vector3(-arrowDir.z, 0f, arrowDir.x);
            Vector3 perp2 = -perp1;

            // 플랫폼 내에서 최적의 방향 탐색 
            Vector3 clamped1 = ClampDirectionToPlatform(perp1);
            Vector3 clamped2 = ClampDirectionToPlatform(perp2);
            Vector3 chosenDodge = (clamped1.sqrMagnitude >= clamped2.sqrMagnitude) ? perp1 : perp2;

            // 수직 회피 불가 : 멀어지는 방향으로 
            if (clamped1.sqrMagnitude < 0.0001f && clamped2.sqrMagnitude < 0.0001f)
            {
                chosenDodge = (-relativePos).normalized;
            }

            if (chosenDodge.sqrMagnitude < 0.0001f) continue;

            // 시간과 거리에 따라 방향별 점수 부여
            float score = (predictionTime - timeToClosest) + (minSeparationDistance - separation);
            if (score > bestScore)
            {
                bestScore = score;
                bestDodge = chosenDodge.normalized;
            }
        }

        if (bestDodge.sqrMagnitude > 0.0001f)
        {
            dodgeDir = bestDodge;
            return true;
        }

        return false;
    }
}

