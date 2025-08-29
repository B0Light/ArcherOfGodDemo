using UnityEngine;

public class Skill_RapidFIre : SkillSO
{
    public override void UseSkill(CharacterManager characterManager)
    {
        characterManager.StartCoroutine(characterManager.SpeedBoostCoroutine());
    }
}
