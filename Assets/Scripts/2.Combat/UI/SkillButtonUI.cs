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
    private PlayerSkillManager _playerSkillManager;
    private int _skillCost;
    public void Init(SkillSO skill, CharacterManager caster)
    {
        _characterManager = caster;
        _playerSkillManager = caster.GetComponent<PlayerSkillManager>();
        _skillCost = skill.cost;
        
        icon.sprite = skill.skillIcon;
        costText.text = skill.cost.ToString();
        skillName.text = skill.skillName;
        skillButton.interactable = false;
        if (_playerSkillManager != null)
            skillButton.onClick.AddListener(()=>_playerSkillManager.UseSkill_Locked(skill));
        else
            skillButton.onClick.AddListener(()=>skill.UseSkill(caster));
    }

    private void Update()
    {
        bool canAfford = _characterManager.actionPoint >= _skillCost;
        bool isAlive = _characterManager != null && !_characterManager.isDead.Value;
        bool shouldEnable = canAfford && isAlive;

        if (shouldEnable && skillButton.interactable == false)
            skillButton.interactable = true;
        if (!shouldEnable && skillButton.interactable == true)
            skillButton.interactable = false;
    }
}
