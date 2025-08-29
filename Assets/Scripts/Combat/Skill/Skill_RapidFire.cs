using UnityEngine;

[CreateAssetMenu(menuName = "bkTools/Skill/RapidFire", fileName = "Skill_RapidFire")]
public class Skill_RapidFire : SkillSO
{
    public override void UseSkill(CharacterManager characterManager)
    {
        characterManager.StartCoroutine(characterManager.SpeedBoostCoroutine());
    }
}
