using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Animator animator;

    [HideInInspector] public CharacterAnimationManager characterAnimationManager;
    [HideInInspector] public CharacterVariableManager characterVariableManager;
    [HideInInspector] public CharacterLocomotionManager characterLocomotionManager;
    [HideInInspector] public CharacterCombatManager characterCombatManager;
    
    public Variable<bool> isDead = new Variable<bool>(false);
    public bool isPerformingAction = false;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        Init();
    }

    private void Init()
    {
        characterAnimationManager = GetComponent<CharacterAnimationManager>();
        characterVariableManager = GetComponent<CharacterVariableManager>();
        characterLocomotionManager = GetComponent<CharacterLocomotionManager>();
        characterCombatManager = GetComponent<CharacterCombatManager>();
    }
    
    public bool CanAttack()
    {
        if (characterLocomotionManager.IsMoving()) 
            return false;
        return true;
    }
}
