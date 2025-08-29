using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private Button skillButton;

    private CharacterManager _characterManager;
    private int _skillCost;
    public void Init(SkillSO skill, CharacterManager caster)
    {
        _characterManager = caster;
        _skillCost = skill.cost;
        
        icon.sprite = skill.skillIcon;
        costText.text = skill.cost.ToString();
        skillName.text = skill.skillName;
        skillButton.interactable = false;
        skillButton.onClick.AddListener(()=>skill.UseSkill(caster));
    }

    private void Update()
    {
        if (_characterManager.actionPoint >= _skillCost && skillButton.interactable == false)
            skillButton.interactable = true;
        if (_characterManager.actionPoint < _skillCost && skillButton.interactable == true)
            skillButton.interactable = false;
    }
}
