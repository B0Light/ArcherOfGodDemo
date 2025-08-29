using UnityEngine;

[CreateAssetMenu(menuName = "bkTools/Skill/ArtilleryRequest", fileName = "Skill_ArtilleryRequest")]
public class Skill_ArtilleryRequest : SkillSO
{
    public override void UseSkill(CharacterManager characterManager)
    {
        if (characterManager.actionPoint.Value >= cost)
        {
            characterManager.actionPoint.Value -= cost;
            
            
            
        }
        
    }
}
