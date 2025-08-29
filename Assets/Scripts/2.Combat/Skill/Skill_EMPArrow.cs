using UnityEngine;

[CreateAssetMenu(menuName = "bkTools/Skill/EMPArrow", fileName = "Skill_EMPArrow")]
public class Skill_EMPArrow : SkillSO
{
	[Header("EMP Settings")]
	[SerializeField] private float extraSpeed = 0f;
	[SerializeField] private GameObject vfx;
	[SerializeField] private LayerMask groundMask = 1;
	[SerializeField] private LayerMask affectMask = ~0;
	[SerializeField] private float radius = 3f;
	[SerializeField] private float disableDuration = 3f;

	public override int GetValue()
	{
		return 2;
	}

	public override void UseSkill(CharacterManager characterManager)
	{
		if (characterManager == null) return;
		if (characterManager.actionPoint < cost) return;

		var ccm = characterManager.characterCombatManager;
		if (ccm == null) return;
		var bow = ccm.GetBowShooter();
		if (bow == null) return;

		characterManager.actionPoint -= cost;

		var velocity = bow.GetCalculatedVelocity();
		if (velocity == Vector3.zero) return;
		var arrow = bow.CreateArrowAtMuzzle();
		if (arrow == null) return;

		var comp = arrow.gameObject.GetComponent<EMPPulseOnGround>();
		if (comp == null) comp = arrow.gameObject.AddComponent<EMPPulseOnGround>();
		comp.Init(vfx, groundMask, affectMask, radius, disableDuration);

		if (extraSpeed != 0f)
		{
			velocity += velocity.normalized * extraSpeed;
		}

		bow.LaunchArrow(arrow, velocity);
	}
}


