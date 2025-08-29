using UnityEngine;

[CreateAssetMenu(menuName = "bkTools/Skill/FirePillarArrow", fileName = "Skill_FirePillarArrow")]
public class Skill_FirePillarArrow : SkillSO
{
    [Header("Fire Pillar Settings")]
    [SerializeField] private GameObject fireTrailVfx;
    [SerializeField] private GameObject firePillarVfx;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float extraSpeed = 0f;

    public override int GetValue()
    {
        return 4;
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

        var comp = arrow.gameObject.GetComponent<FirePillarOnGround>();
        if (comp == null) comp = arrow.gameObject.AddComponent<FirePillarOnGround>();
        comp.Init(firePillarVfx,fireTrailVfx, groundMask, hitMask);
        comp.InstantFireTrail();
        if (extraSpeed != 0f)
        {
            velocity += velocity.normalized * extraSpeed;
        }

        bow.LaunchArrow(arrow, velocity);
    }
}



