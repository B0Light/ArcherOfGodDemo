using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayableDirector playableDirector;

    [HideInInspector] public CharacterAnimationManager characterAnimationManager;
    [HideInInspector] public CharacterLocomotionManager characterLocomotionManager;
    [HideInInspector] public CharacterCombatManager characterCombatManager;
    public Variable<bool> isDead = new Variable<bool>(false);
    public bool isPerformingAction = false;
    
    [SerializeField] private Transform target;
    protected CharacterManager targetCharacterManager;

    public Variable<int> actionPoint = new Variable<int>(0);
    private double _defaultSpeed = 1.0;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playableDirector = GetComponent<PlayableDirector>();

        characterAnimationManager = GetComponent<CharacterAnimationManager>();
        characterLocomotionManager = GetComponent<CharacterLocomotionManager>();
        characterCombatManager = GetComponent<CharacterCombatManager>();
    }

    private void Start()
    {
        PrepareAim();
        SetTargetCharacterManager();
        SubscribeEvent();
        ShootTrigger();
    }

    private void OnDisable()
    {
        actionPoint.ClearAllSubscribers();
    }

    private void SetTargetCharacterManager()
    {
        targetCharacterManager = target.GetComponent<CharacterManager>();
    }
    
    protected virtual void SubscribeEvent()
    {
        characterLocomotionManager.onStop.AddListener(ShootTrigger);
        characterLocomotionManager.onMove.AddListener(CloseTrigger);
        
        targetCharacterManager.isDead.OnValueChanged += b => CloseTrigger();

        isDead.OnValueChanged += DeadProcess;
        
    }

    private void ShootTrigger()
    {
        playableDirector.enabled = true;
        playableDirector.Play();
    }

    public void CloseTrigger()
    {
        StopAllCoroutines();
        playableDirector.Stop();
        playableDirector.enabled = false;
    }

    private void PrepareAim()
    {
        if (characterCombatManager != null)
        {
            characterCombatManager.UpdateBowAim();
        }
    }

    private void DeadProcess(bool value)
    {
        StopAllCoroutines();
        playableDirector.enabled = false;
        characterAnimationManager.PlayTargetActionAnimation("Dead", true);
    }

    public Transform GetTarget()
    {
        return target;
    }
    
    public void SetTimelineSpeed(double speed)
    {
        if (playableDirector != null)
        {
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(speed);
        }
    }
    
    public IEnumerator SpeedBoostCoroutine()
    {
        // 원래 속도 저장
        _defaultSpeed = playableDirector.playableGraph.GetRootPlayable(0).GetSpeed();

        float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 1배속 → 2배속 으로 점진적으로 증가
            double newSpeed = Mathf.Lerp((float)_defaultSpeed, (float)(_defaultSpeed * 5), elapsed / duration);
            SetTimelineSpeed(newSpeed);
            yield return null;
        }

        // 5초 뒤 원상 복구
        SetTimelineSpeed(_defaultSpeed);
    }
    
    
}
