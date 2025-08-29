using UnityEngine;

[CreateAssetMenu(menuName = "bkTools/Skill/BombArrow", fileName = "Skill_BombArrow")]
public class Skill_BombArrow : SkillSO
{
    public override void UseSkill(CharacterManager characterManager)
    {
        if (characterManager.actionPoint >= cost)
        {
            characterManager.actionPoint -= cost;
        }
        
    }
}
