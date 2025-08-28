using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayableDirector playableDirector;
    
    [HideInInspector] public CharacterLocomotionManager characterLocomotionManager;
    [HideInInspector] public CharacterCombatManager characterCombatManager;
    public Variable<bool> isDead = new Variable<bool>(false);
    public bool isPerformingAction = false;
    
    [SerializeField] private Transform target; 
    private float _launchAngle = 45f;
    
    private readonly int _shootHash = Animator.StringToHash("Shoot");
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playableDirector = GetComponent<PlayableDirector>();
        characterLocomotionManager = GetComponent<CharacterLocomotionManager>();
        characterCombatManager = GetComponent<CharacterCombatManager>();
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
