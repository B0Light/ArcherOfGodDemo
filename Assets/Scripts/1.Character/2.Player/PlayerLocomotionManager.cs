using UnityEngine;

public class PlayerLocomotionManager : CharacterLocomotionManager
{ 
    private InputManager _inputManager;
    
    protected override void Awake()
    {
        base.Awake();
        _inputManager = InputManager.Instance;
    }
    
    protected override void Update()
    {
        HandlePlayerInput();
        
        base.Update();
    }
    
    private void HandlePlayerInput()
    {
        if (_inputManager == null) return;
        
        Vector2 moveInput = _inputManager.GetMoveDir();
        
        if (_inputManager.movementInputDetected)
        {
            Vector3 moveDirection = CalculateMoveDirection(moveInput);
            Vector3 clampedDirection = ClampDirectionToPlatform(moveDirection);
            SetMoveDirection(clampedDirection);
        }
        else
        {
            SetMoveDirection(Vector3.zero);
        }
    }
    
    private Vector3 CalculateMoveDirection(Vector2 input)
    {
        Vector3 moveDirection = Vector3.forward * input.y + Vector3.right * input.x;
        moveDirection.y = 0f;
        
        return moveDirection.normalized;
    }
}
