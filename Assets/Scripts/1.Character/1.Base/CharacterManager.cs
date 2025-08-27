using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayableDirector playableDirector;
    
    [HideInInspector] public CharacterAnimationManager characterAnimationManager;
    [HideInInspector] public CharacterVariableManager characterVariableManager;
    [HideInInspector] public CharacterLocomotionManager characterLocomotionManager;
    [HideInInspector] public CharacterCombatManager characterCombatManager;
    
    public Variable<bool> isDead = new Variable<bool>(false);
    public bool isPerformingAction = false;
    
    private readonly int _shootHash = Animator.StringToHash("Shoot");
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playableDirector = GetComponent<PlayableDirector>();
        Init();
    }

    private void Init()
    {
        characterAnimationManager = GetComponent<CharacterAnimationManager>();
        characterVariableManager = GetComponent<CharacterVariableManager>();
        characterLocomotionManager = GetComponent<CharacterLocomotionManager>();
        characterCombatManager = GetComponent<CharacterCombatManager>();
    }

    private void Start()
    {
        SubscribeEvent();
    }

    private void SubscribeEvent()
    {
        characterLocomotionManager.onStop.AddListener(ShootTrigger);
        characterLocomotionManager.onMove.AddListener(CloseTrigger);
    }
    
    public bool CanAttack()
    {
        if (characterLocomotionManager.IsMoving()) 
            return false;
        return true;
    }

    private void ShootTrigger()
    {
        playableDirector.Play();
        characterCombatManager.ShootArrow();
    }

    private void CloseTrigger()
    {
        playableDirector.Stop();
        playableDirector.time = 0.1f;
    }
}
