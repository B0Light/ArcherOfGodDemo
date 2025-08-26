using System;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    [HideInInspector] public PlayerManager playerManager;

    private PlayerControls _playerControls;
    
    public Vector2 moveComposite;
    public bool movementInputDetected;
    
    private void OnEnable()
    {
        if (_playerControls == null)
        {
            _playerControls = new PlayerControls();
            
            _playerControls.Locomotion.Move.performed += i => moveComposite = i.ReadValue<Vector2>();
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
    }
    
    private void HandleMoveInput()
    {
        movementInputDetected = moveComposite.magnitude > 0;
    }
    
    public Vector3 CalculateInput()
    {
        Vector3 moveDirection = movementInputDetected ? 
            (Vector3.forward * moveComposite.y) + (Vector3.right * moveComposite.x) :
            Vector3.zero;

        return moveDirection;
    }
}
