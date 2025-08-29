using System;
using UnityEngine;

public class PlayerSkillManager : SkillManager
{
    [SerializeField] private GameObject skillBtnPrefab;
    [SerializeField] private Transform skillBtnParent;

    
    private void Start()
    {
        InitBtn();
    }

    private void InitBtn()
    {
        foreach (var skillSo in skillList)
        {
            CreateSkillBtn(skillSo);
        }
    }

    private void CreateSkillBtn(SkillSO selectedSkill)
    {
        GameObject skillBtn = Instantiate(skillBtnPrefab, skillBtnParent);
        SkillButtonUI skillButtonUI = skillBtn.GetComponent<SkillButtonUI>();
        if (skillButtonUI)
        {
            skillButtonUI.Init(selectedSkill, _characterManager);
        }
    }
}
