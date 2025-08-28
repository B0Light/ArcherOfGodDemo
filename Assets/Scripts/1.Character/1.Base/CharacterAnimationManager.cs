using UnityEngine;

public class CharacterAnimationManager : MonoBehaviour
{
    protected CharacterManager characterManager;
    
    [Header("Flags")]
    public bool applyRootMotion = false;
    
    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }
    public void PlayTargetActionAnimation(
        string targetAnimation,
        bool isPerformingAction, 
        bool rootMotion = true,
        bool canRotate = false,
        bool canMove = false)
    {
        if (targetAnimation != "Dead" && characterManager.isDead.Value) return;
        
        applyRootMotion = rootMotion;
        characterManager.animator.CrossFade(targetAnimation, 0.2f);
        characterManager.isPerformingAction = isPerformingAction;
        characterManager.characterLocomotionManager.canRotate = canRotate;
        characterManager.characterLocomotionManager.canMove = canMove;
    }
}
