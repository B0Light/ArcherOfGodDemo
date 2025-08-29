using UnityEngine;

[CreateAssetMenu(menuName = "bkTools/Skill/ExplosiveArrow", fileName = "Skill_ExplosiveArrow")]
public class Skill_ExplosiveArrow : SkillSO
{
    [Header("Explosive Settings")]
    [SerializeField] private float extraSpeed = 0f;
    [SerializeField] private GameObject vfx;
    
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask hitMask;
    public override int GetValue()
    {
        return 3; // 간단한 우선순위 값
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

        // 단발 발사 후 지면 충돌 시 폭발
        var velocity = bow.GetCalculatedVelocity();
        if (velocity == Vector3.zero) return;
        var arrow = bow.CreateArrowAtMuzzle();
        if (arrow == null) return;

        // 폭발 트리거를 붙인 임시 오브젝트 생성
        var go = arrow.gameObject;
        var explosive = go.GetComponent<ExplosionOnGround>();
        if (explosive == null)
        {
            explosive = go.AddComponent<ExplosionOnGround>();
        }
        if(vfx != null)
            explosive.Init(vfx, groundMask, hitMask);
        arrow.IsExplosiveShot = true;

        // 약간의 속도 보정 가능
        if (extraSpeed != 0f)
        {
            velocity += velocity.normalized * extraSpeed;
        }

        bow.LaunchArrow(arrow, velocity);
    }
}


