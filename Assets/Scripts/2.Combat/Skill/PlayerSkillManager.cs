using System;
using UnityEngine;

public class PlayerSkillManager : SkillManager
{
    [SerializeField] private GameObject skillBtnPrefab;
    [SerializeField] private Transform skillBtnParent;
    [Header("Lock Settings")]
    [SerializeField] private float moveLockDuration = 0.35f;
    [SerializeField] private bool lockRotation = true;

    
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

    public override void UseSkill_Locked(SkillSO skill)
    {
        if (skill == null) return;
        if (_characterManager == null) return;
        if (_characterManager.isDead.Value) return;
        if (_characterManager.actionPoint < skill.cost) return;

        var locomotion = _characterManager.characterLocomotionManager;
        bool prevMove = locomotion ? locomotion.canMove : true;
        bool prevRotate = locomotion ? locomotion.canRotate : true;

        _characterManager.isPerformingAction = true;
        if (locomotion)
        {
            locomotion.canMove = false;
            if (lockRotation) locomotion.canRotate = false;
        }

        skill.UseSkill(_characterManager);

        _characterManager.StartCoroutine(UnlockAfterDelay(moveLockDuration, locomotion, prevMove, prevRotate));
    }

    private System.Collections.IEnumerator UnlockAfterDelay(float delay, CharacterLocomotionManager locomotion, bool prevMove, bool prevRotate)
    {
        yield return new UnityEngine.WaitForSeconds(delay);
        if (locomotion)
        {
            locomotion.canMove = prevMove;
            locomotion.canRotate = prevRotate;
        }
        _characterManager.isPerformingAction = false;
    }
}
