using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu(menuName = "bkTools/Skill/AttackSpeedPermanent", fileName = "Skill_AttackSpeedPermanent")]
public class Skill_AttackSpeedPermanent : SkillSO
{
	[Header("Attack Speed Settings")]
	[SerializeField] private float multiplier = 1.1f;

	public override int GetValue()
	{
		return 1; // 단순 우선순위
	}

	public override void UseSkill(CharacterManager characterManager)
	{
		if (characterManager == null) return;
		if (characterManager.isDead.Value) return;
		if (characterManager.actionPoint < cost) return;

		if (characterManager.playableDirector == null) return;

		var root = characterManager.playableDirector.playableGraph.GetRootPlayable(0);
		double current = root.GetSpeed();
		double newSpeed = current * Mathf.Max(0.01f, multiplier);
		characterManager.SetTimelineSpeed(newSpeed);

		characterManager.actionPoint -= cost;
	}
}


