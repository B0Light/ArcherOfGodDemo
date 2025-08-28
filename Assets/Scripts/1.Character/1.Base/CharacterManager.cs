using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayableDirector playableDirector;
    
    [HideInInspector] public CharacterLocomotionManager characterLocomotionManager;
    
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
        characterLocomotionManager = GetComponent<CharacterLocomotionManager>();
    }

    private void Start()
    {
        SubscribeEvent();
        ShootTrigger();
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
        
    }

    private void CloseTrigger()
    {
        playableDirector.Stop();
        playableDirector.time = 0.1f;
    }
}
