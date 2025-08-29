using UnityEngine;

[CreateAssetMenu(menuName = "bkTools/Skill/RapidFire", fileName = "Skill_RapidFire")]
public class Skill_RapidFire : SkillSO
{
    public override void UseSkill(CharacterManager characterManager)
    {
        if (characterManager.actionPoint >= cost)
        {
            characterManager.actionPoint -= cost;
            characterManager.StartCoroutine(characterManager.SpeedBoostCoroutine());
        }
        
    }
}
