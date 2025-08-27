using System;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : Singleton<InputManager>
{
    [HideInInspector] public PlayerManager playerManager;

    private PlayerControls _playerControls;
    
    public Vector2 moveComposite;
    public bool movementInputDetected;

    private bool _jumpInput = false;
    public UnityEvent jumpAction;
    
    private void OnEnable()
    {
        if (_playerControls == null)
        {
            _playerControls = new PlayerControls();
            
            _playerControls.Locomotion.Move.performed += i => moveComposite = i.ReadValue<Vector2>();
            _playerControls.Locomotion.Jump.performed += i => _jumpInput = true;
        }
        
        _playerControls.Enable();
    }
    
    private void Update()
    {
        HandleAllInputs();
    }

    private void HandleAllInputs()
    {
        HandleMoveInput();
        HandleJumpInput();
    }
    
    private void HandleMoveInput() => movementInputDetected = moveComposite.magnitude > 0;

    private void HandleJumpInput()
    {
        if (_jumpInput)
        {
            _jumpInput = false;
            jumpAction?.Invoke();
            Debug.Log("JUMP");
        }
    }
    
    public Vector2 GetMoveDir() => movementInputDetected ? moveComposite : Vector2.zero;
}
