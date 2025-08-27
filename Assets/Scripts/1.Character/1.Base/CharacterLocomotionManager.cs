using UnityEngine;
using UnityEngine.Events;

public enum AnimationState
{
    Base,
    Locomotion,
    Jump,
    Fall,
    Dead,
}

public class CharacterLocomotionManager : MonoBehaviour
{
    [SerializeField] private AnimationState currentState = AnimationState.Base;
    
    [Header("Movement Settings")]
    //[SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float speedChangeDamping = 10f;
    [SerializeField] private float rotationSmoothing = 10f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Ground Check")]
    [SerializeField] protected float groundedOffset = 0f;
    [SerializeField] private LayerMask groundLayerMask = 1;
    
    [Header("Control")]
    public bool canMove = true;
    public bool canRotate = true;
    
    [Header("Idle Facing")] 
    [SerializeField] private bool faceWhenIdle = true;
    [SerializeField] private Transform idleLookTarget; // 설정 시 이 타겟을 바라봄
    [SerializeField] private Vector3 idleFacingDirection = Vector3.forward; // 타겟이 없으면 이 월드 방향
    [SerializeField] private float idleRotationSmoothing = 10f;
    
    [Header("Platform Boundaries")]
    [SerializeField] private float platformSize = 7f;
    [SerializeField] private float boundaryBuffer = 0.5f;
    [SerializeField] private Transform platformCenterTransform;
    [SerializeField] private float platformY = 0f;
    
    [Header("Player Specific Settings")]
    [SerializeField] private float playerRadius = 0.3f;
    
    // Private variables
    private CharacterManager _characterManager;
    private Vector3 _moveDirection;
    private Vector3 _velocity;
    private float _currentMaxSpeed;
    private float _speed2D;
    private bool _isGrounded = true;
    private bool _isJumping = false;
    private bool _canJump = false;
    
    // Animation hashes
    private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int _isJumpingHash = Animator.StringToHash("IsJumping");
    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");

    public UnityEvent onMove;
    public UnityEvent onStop;
    
    protected virtual void Awake()
    {
        _characterManager = GetComponent<CharacterManager>();
    }
    
    private void Start()
    {
        _currentMaxSpeed = runSpeed;
        canMove = true;
        canRotate = true;

        InputManager.Instance.jumpAction.AddListener(HandleJump);
        SwitchState(AnimationState.Locomotion);
    }
    
    protected virtual void Update()
    {
        switch (currentState)
        {
            case AnimationState.Locomotion:
                UpdateLocomotionState();
                break;
            case AnimationState.Jump:
                UpdateJumpState();
                break;
            case AnimationState.Fall:
                UpdateFallState();
                break;
        }
        
        CheckPlatformBoundaries();
    }
    
    #region ControlState
    private void SwitchState(AnimationState newState)
    {
        ExitCurrentState();
        currentState = newState;
        EnterState(newState);
    }
    
    private void EnterState(AnimationState stateToEnter)
    {
        if(_characterManager.isDead.Value)
        {
            SwitchState(AnimationState.Dead);
            return;
        }

        switch (currentState)
        {
            case AnimationState.Base:
                break;
            case AnimationState.Locomotion:
                EnterLocomotionState();
                break;
            case AnimationState.Jump:
                EnterJumpState();
                break;
            case AnimationState.Fall:
                EnterFallState();
                break;
        }
    }
    #endregion
    
    private void EnterLocomotionState()
    {
        _canJump = true;
    }
    
    private void UpdateLocomotionState()
    {
        GroundedCheck();

        if (!_isGrounded)
        {
            SwitchState(AnimationState.Fall);
        }

        HandleMovement();
        FaceMoveDirection();
        FaceIdleDirection();
        Move();
        UpdateAnimator();
    }
    
    private void ExitLocomotionState()
    {
        _canJump = false;
    }
    
    private void EnterJumpState()
    {
        if (_characterManager != null && _characterManager.animator != null)
        {
            _characterManager.animator.SetBool(_isJumpingHash, true);
        }

        _velocity.y = jumpForce;
        _isJumping = true;
    }
    private void UpdateJumpState()
    {
        ApplyGravity();

        if (_velocity.y <= 0f)
        {
            _characterManager.animator.SetBool(_isJumpingHash, false);
            SwitchState(AnimationState.Fall);
        }

        GroundedCheck();
        HandleMovement();
        FaceMoveDirection();
        Move();
        UpdateAnimator();
    }
    
    private void ExitJumpState()
    {
        if (_characterManager != null && _characterManager.animator != null)
        {
            _characterManager.animator.SetBool(_isJumpingHash, false);
        }
    }
    
    private void EnterFallState()
    {
        _velocity.y = 0f;
    }
    
    private void UpdateFallState()
    {
        GroundedCheck();
        HandleMovement();
        FaceMoveDirection();
        ApplyGravity();
        Move();
        UpdateAnimator();

        if (_isGrounded)
        {
            SwitchState(AnimationState.Locomotion);
        }
    }

    private void ExitFallState() { }

    private void ExitCurrentState()
    {
        switch (currentState)
        {
            case AnimationState.Locomotion:
                ExitLocomotionState();
                break;
            case AnimationState.Jump:
                ExitJumpState();
                break;
            case AnimationState.Fall:
                ExitFallState();
                break;
        }
    }
    
    #region Movement
    
    private void HandleMovement()
    {
        if (!canMove) return;
        if (_moveDirection.magnitude > 0.1f)
        {
            Vector3 targetVelocity = _moveDirection * _currentMaxSpeed;
            
            _velocity.x = Mathf.Lerp(_velocity.x, targetVelocity.x, speedChangeDamping * Time.deltaTime);
            _velocity.z = Mathf.Lerp(_velocity.z, targetVelocity.z, speedChangeDamping * Time.deltaTime);
            
            _speed2D = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
            _speed2D = Mathf.Round(_speed2D * 1000f) / 1000f;
            
            if (canRotate)
            {
                FaceMoveDirection();
            }

            onMove?.Invoke();
        }
        else
        {
            // 정지 시 속도 감소
            _velocity.x = Mathf.Lerp(_velocity.x, 0f, speedChangeDamping * Time.deltaTime);
            _velocity.z = Mathf.Lerp(_velocity.z, 0f, speedChangeDamping * Time.deltaTime);
            _speed2D = 0f;
            onStop?.Invoke();
        }
        
        if (!_isGrounded)
        {
            ApplyGravity();
        }
    }
    
    private void Move()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.Move(_velocity * Time.deltaTime);
        }
    }
    
    private void FaceMoveDirection()
    {
        if (_moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothing * Time.deltaTime);
        }
    }
    
    private void FaceIdleDirection()
    {
        if (!faceWhenIdle) return;
        if (!canRotate) return;
        if (IsMoving()) return;

        Vector3 targetDirection;
        if (idleLookTarget != null)
        {
            targetDirection = idleLookTarget.position - transform.position;
            targetDirection.y = 0f;
        }
        else
        {
            targetDirection = new Vector3(idleFacingDirection.x, 0f, idleFacingDirection.z);
        }

        if (targetDirection.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, idleRotationSmoothing * Time.deltaTime);
    }
    
    #endregion
    
    #region Jump
    
    private void HandleJump()
    {
        if(!CanJump()) return;
        
        if (_canJump && !_isJumping)
        {
            Debug.Log("Switch State : jump");
            SwitchState(AnimationState.Jump);
        }
    }

    private bool CanJump()
    {
        if (!canMove || !_isGrounded) return false;
        if (_characterManager.isPerformingAction) return false;
        if (currentState == AnimationState.Jump) return false;
        return true;
    }
    
    private void ApplyGravity()
    {
        if (_velocity.y > gravity)
        {
            _velocity.y += gravity * Time.deltaTime;
        }
    }
    
    #endregion
    
    #region Ground Check
    
    private void GroundedCheck()
    {
        // 캐릭터의 중심점에서 아래로 레이캐스트
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        _isGrounded = Physics.CheckSphere(spherePosition, 0.2f, groundLayerMask);
        
        // 착지 시 점프 상태 해제
        if (_isGrounded && _isJumping)
        {
            _isJumping = false;
            if (_characterManager != null && _characterManager.animator != null)
            {
                _characterManager.animator.SetBool(_isJumpingHash, false);
            }
        }
    }
    
    #endregion
    
    #region Animation
    
    private void UpdateAnimator()
    {
        if (_characterManager == null || _characterManager.animator == null) return;
        
        _characterManager.animator.SetFloat(_moveSpeedHash, _speed2D);
        _characterManager.animator.SetBool(_isJumpingHash, _isJumping);
        _characterManager.animator.SetBool(_isGroundedHash, _isGrounded);
    }
    
    #endregion

    
    public void SetMoveDirection(Vector3 direction)
    {
        _moveDirection = direction.normalized;
    }
    
    public void SetMovementSpeed(float speed)
    {
        speed = Mathf.Clamp01(speed);
        _currentMaxSpeed = Mathf.Lerp(0f, runSpeed, speed);
    }

    public bool IsMoving()
    {
        return _speed2D > 0.1f;
    }
    
    public bool IsJumping()
    {
        return _isJumping;
    }
    
    public bool IsGrounded()
    {
        return _isGrounded;
    }

    #region Platform

    protected Vector3 ClampDirectionToPlatform(Vector3 direction)
    {
        Vector3 center = platformCenterTransform ? platformCenterTransform.position : Vector3.zero;
        Vector3 localPos = transform.position - center;
        float halfSize = platformSize * 0.5f;
        float buffer = boundaryBuffer + playerRadius;
        
        Vector3 clampedDirection = direction;
        
        if (Mathf.Abs(localPos.x) >= halfSize - buffer)
        {
            if (Mathf.Approximately(Mathf.Sign(localPos.x),Mathf.Sign(direction.x)))
            {
                clampedDirection.x = 0f;
            }
        }
        
        if (Mathf.Abs(localPos.z) >= halfSize - buffer)
        {
            if (Mathf.Approximately(Mathf.Sign(localPos.z), Mathf.Sign(direction.z)))
            {
                clampedDirection.z = 0f;
            }
        }
        
        return clampedDirection.normalized;
    }
    protected void CheckPlatformBoundaries()
    {
        Vector3 center = platformCenterTransform ? platformCenterTransform.position : Vector3.zero;
        Vector3 currentPos = transform.position;
        Vector3 localPos = currentPos - center;
        float halfSize = platformSize * 0.5f;
        float buffer = boundaryBuffer + playerRadius;
        
        bool positionChanged = false;
        Vector3 clampedLocal = localPos;
        
        if (Mathf.Abs(localPos.x) > halfSize - buffer)
        {
            clampedLocal.x = Mathf.Sign(localPos.x) * (halfSize - buffer);
            positionChanged = true;
        }
        
        if (Mathf.Abs(localPos.z) > halfSize - buffer)
        {
            clampedLocal.z = Mathf.Sign(localPos.z) * (halfSize - buffer);
            positionChanged = true;
        }
        
        if (!Mathf.Approximately(currentPos.y, platformY))
        {
            positionChanged = true;
        }
        
        if (positionChanged)
        {
            Vector3 newWorldPos = center + clampedLocal;
            newWorldPos.y = platformY;
            transform.position = newWorldPos;
        }
        
    }

    #endregion
    
    
    
    protected virtual void OnDrawGizmosSelected()
    {
        // 지면 체크 구체 표시
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        Gizmos.DrawWireSphere(spherePosition, 0.2f);
        
        Vector3 center = platformCenterTransform ? platformCenterTransform.position : Vector3.zero;
        Gizmos.color = Color.yellow;
        Vector3 size = new Vector3(platformSize, 0.1f, platformSize);
        Gizmos.DrawWireCube(center, size);
        
        Gizmos.color = Color.green;
        float safeSize = platformSize - (boundaryBuffer + playerRadius) * 2f;
        Vector3 safeSize3D = new Vector3(safeSize, 0.1f, safeSize);
        Gizmos.DrawWireCube(center, safeSize3D);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, playerRadius);
    }

}
