using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    protected CharacterManager _characterManager;
    
    [SerializeField] protected List<SkillSO> skillList = new List<SkillSO>();
    [Header("Card Draw Settings")]
    [SerializeField] protected float drawIntervalSeconds = 5f;
    
    private readonly Dictionary<SkillSO, float> _cooldownUntil = new();
    private List<SkillSO> _activeSkillList = new List<SkillSO>();
    protected readonly List<SkillSO> _activatedPool = new List<SkillSO>();

    public event Action<SkillSO> OnSkillDrawn;
    public event Action<SkillSO> OnSkillUsed;

    private void Awake()
    {
        _characterManager = GetComponent<CharacterManager>();
        StartCoroutine(DrawRoutine());
    }
    
    public void UseSkill_Auto()
    {
        if (_characterManager == null) return;
        if (_characterManager.isDead.Value) return;
        SkillSO curSkill = SelectSkill();
        if(curSkill == null) return;
        
        curSkill.UseSkill(_characterManager);
        MarkSkillUsed(curSkill);
    }

    public virtual void UseSkill_Locked(SkillSO skill)
    {
        if (skill == null) return;
        if (_characterManager == null) return;
        if (_characterManager.isDead.Value) return;
        skill.UseSkill(_characterManager);
        MarkSkillUsed(skill);
    }

    private SkillSO SelectSkill()
    {
        _activeSkillList.Clear();
        for (int i = 0; i < _activatedPool.Count; i++)
        {
            var skill = _activatedPool[i];
            if (skill == null) continue;
            if (skill.cost <= _characterManager.actionPoint.Value)
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

    protected void MarkSkillUsed(SkillSO skill)
    {
        if (skill == null) return;
        int idx = _activatedPool.IndexOf(skill);
        if (idx >= 0)
        {
            _activatedPool.RemoveAt(idx);
            OnSkillUsed?.Invoke(skill);
        }
    }

    public bool IsSkillActivated(SkillSO skill)
    {
        return skill != null && _activatedPool.Contains(skill);
    }

    private IEnumerator DrawRoutine()
    {
        var wait = new WaitForSeconds(Mathf.Max(0.1f, drawIntervalSeconds));
        while (true)
        {
            yield return wait;
            if (_characterManager == null) continue;
            if (_characterManager.isDead.Value) continue;
            if (skillList == null || skillList.Count == 0) continue;

            _activeSkillList.Clear();
            for (int i = 0; i < skillList.Count; i++)
            {
                var s = skillList[i];
                if (s == null) continue;
                if (!_activatedPool.Contains(s))
                {
                    _activeSkillList.Add(s);
                }
            }
            if (_activeSkillList.Count == 0) continue;
            int pick = UnityEngine.Random.Range(0, _activeSkillList.Count);
            var drawn = _activeSkillList[pick];
            if (drawn != null)
            {
                _activatedPool.Add(drawn);
                OnSkillDrawn?.Invoke(drawn);
            }
        }
    }
}
