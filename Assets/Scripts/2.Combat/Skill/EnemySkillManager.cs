using UnityEngine;

public class EnemySkillManager : SkillManager
{
	[Header("AI Casting Settings")]
	[SerializeField] private float thinkInterval = 0.25f;
	[SerializeField] private float castCooldown = 1.5f;

	private float _nextThinkTime;
	private float _nextCastTime;

	private CharacterLocomotionManager _locomotion;
	private CharacterCombatManager _combat;

	private void Start()
	{
		_locomotion = GetComponent<CharacterLocomotionManager>();
		_combat = GetComponent<CharacterCombatManager>();
	}

	private void Update()
	{
		if (Time.time < _nextThinkTime) return;
		_nextThinkTime = Time.time + Mathf.Max(0.05f, thinkInterval);

		if (!CanAttemptCast()) return;
		if (Time.time < _nextCastTime) return;

		// 목표를 조준 업데이트 (원거리 무기 기준)
		if (_combat != null)
		{
			_combat.UpdateBowAim();
		}

		UseSkill_Auto();
		_nextCastTime = Time.time + Mathf.Max(0.1f, castCooldown);
	}

	private bool CanAttemptCast()
	{
		if (_characterManager == null) return false;
		if (_characterManager.isDead.Value) return false;
		if (_characterManager.isPerformingAction) return false;

		Transform target = _characterManager.GetTarget();
		if (target == null) return false;
		var targetCm = target.GetComponent<CharacterManager>();
		if (targetCm != null && targetCm.isDead.Value) return false;

		if (_locomotion != null && _locomotion.IsMoving()) return false;

		// 액션 포인트로 선택 가능한 스킬이 있어야 함
		return true;
	}
}


