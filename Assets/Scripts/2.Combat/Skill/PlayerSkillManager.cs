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
        // 초기에는 버튼 생성 안 함. 드로우 시점에 생성
        OnSkillDrawn += HandleSkillDrawn;
        OnSkillUsed += HandleSkillUsed;
    }

    private void InitBtn() { }

    private void CreateSkillBtn(SkillSO selectedSkill)
    {
        GameObject skillBtn = Instantiate(skillBtnPrefab, skillBtnParent);
        SkillButtonUI skillButtonUI = skillBtn.GetComponent<SkillButtonUI>();
        if (skillButtonUI)
        {
            skillButtonUI.Init(selectedSkill, _characterManager);
        }
        skillBtn.name = $"SkillBtn_{selectedSkill.skillName}";
    }

    public override void UseSkill_Locked(SkillSO skill)
    {
        if (skill == null) return;
        if (_characterManager == null) return;
        if (_characterManager.isDead.Value) return;
        if (_characterManager.actionPoint.Value < skill.cost) return;
        if (!IsSkillActivated(skill)) return;

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
        MarkSkillUsed(skill);

        _characterManager.StartCoroutine(UnlockAfterDelay(moveLockDuration, locomotion, prevMove, prevRotate));
    }
    private void HandleSkillDrawn(SkillSO skill)
    {
        CreateSkillBtn(skill);
    }

    private void HandleSkillUsed(SkillSO skill)
    {
        // 해당 스킬 버튼 제거
        for (int i = 0; i < skillBtnParent.childCount; i++)
        {
            var child = skillBtnParent.GetChild(i);
            var ui = child.GetComponent<SkillButtonUI>();
            if (ui != null && ui.IsForSkill(skill))
            {
                Destroy(child.gameObject);
                break;
            }
        }
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
