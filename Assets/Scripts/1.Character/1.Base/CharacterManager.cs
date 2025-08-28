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
        PrepareAim();
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
    
    public void SetTimelineSpeed(double speed)
    {
        if (playableDirector != null)
        {
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(speed);
        }
    }

    private void CloseTrigger()
    {
        playableDirector.Stop();
        playableDirector.time = 0.1f;
    }

    private void PrepareAim()
    {
        if (characterCombatManager != null)
        {
            characterCombatManager.UpdateBowAim();
        }
    }

    public Transform GetTarget()
    {
        return target;
    }
}
