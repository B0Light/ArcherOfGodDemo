using UnityEngine;

[CreateAssetMenu(menuName = "bkTools/Skill/SplitArrow", fileName = "Skill_SplitArrow")]
public class Skill_SplitArrow : SkillSO
{
    [Header("Split Settings")]
    [SerializeField] private int arrowCount = 5;
    [SerializeField] private float spreadDegrees = 25f;

    public override int GetValue()
    {
        return 2; // 간단한 우선순위 값
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
        bow.ShootMulti(Mathf.Max(1, arrowCount), Mathf.Max(0f, spreadDegrees));
    }
}



