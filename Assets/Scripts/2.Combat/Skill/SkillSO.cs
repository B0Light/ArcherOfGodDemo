using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "bkTools/Skill/Empty", fileName = "Skill")]
public class SkillSO : ScriptableObject
{
    public string skillName = "EMPTY";
    public Sprite skillIcon;
    public int cost = 3;

    public virtual int GetValue()
    {
        return 0;
    }
    
    public virtual void UseSkill(CharacterManager characterManager)
    {
        
    }
}
