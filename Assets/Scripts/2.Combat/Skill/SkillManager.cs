using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    protected CharacterManager _characterManager;
    
    [SerializeField] protected List<SkillSO> skillList = new List<SkillSO>();
    
    private readonly Dictionary<SkillSO, float> _cooldownUntil = new();
    private List<SkillSO> _activeSkillList = new List<SkillSO>();

    private void Awake()
    {
        _characterManager = GetComponent<CharacterManager>();
    }
    
    public void UseSkill_Auto()
    {
        SkillSO curSkill = SelectSkill();
        if(curSkill == null) return;
        
        curSkill.UseSkill(_characterManager);
    }

    private SkillSO SelectSkill()
    {
        _activeSkillList.Clear();
        foreach (var skill in skillList)
        {
            if (skill.cost <= _characterManager.actionPoint)
            {
                _activeSkillList.Add(skill);
            }
        }

        if (_activeSkillList.Count == 0) return null;
        
        SkillSO bestSkill = null;
        int maxValue = int.MinValue;

        foreach (var skill in _activeSkillList)
        {
            int value = skill.GetValue();
            if (value > maxValue)
            {
                maxValue = value;
                bestSkill = skill;
            }
        }

        return bestSkill;
    }
}
