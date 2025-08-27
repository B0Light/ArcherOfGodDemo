using UnityEngine;

public class CharacterLocomotionManager : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float speedChangeDamping = 10f;
    [SerializeField] private float rotationSmoothing = 10f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravityMultiplier = 2f;
    
    [Header("Ground Check")]
    [SerializeField] private float groundedOffset = -0.14f;
    [SerializeField] private LayerMask groundLayerMask = 1;
    
    [Header("Control")]
    public bool canMove = true;
    public bool canRotate = true;
    
    // Private variables
    private CharacterManager _characterManager;
    private Vector3 _moveDirection;
    private Vector3 _velocity;
    private float _currentMaxSpeed;
    private float _speed2D;
    private bool _isGrounded = true;
    private bool _isJumping = false;
    
    // Animation hashes
    private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int _isJumpingHash = Animator.StringToHash("IsJumping");
    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
    
    protected virtual void Awake()
    {
        _characterManager = GetComponent<CharacterManager>();
    }
    
    protected virtual void Start()
    {
        _currentMaxSpeed = runSpeed;
        canMove = true;
        canRotate = true;

        InputManager.Instance.jumpAction.AddListener(HandleJump);
    }
    
    protected virtual void Update()
    {
        GroundedCheck();
        HandleMovement();
        UpdateAnimator();
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
        }
        else
        {
            // 정지 시 속도 감소
            _velocity.x = Mathf.Lerp(_velocity.x, 0f, speedChangeDamping * Time.deltaTime);
            _velocity.z = Mathf.Lerp(_velocity.z, 0f, speedChangeDamping * Time.deltaTime);
            _speed2D = 0f;
        }
        
        if (!_isGrounded)
        {
            ApplyGravity();
        }
        
        Move();
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
    
    #endregion
    
    #region Jump
    
    private void HandleJump()
    {
        if (!canMove || !_isGrounded) return;
        
        if (!_isJumping)
        {
            Jump();
        }
    }
    
    private void Jump()
    {
        if (!_isGrounded) return;
        
        _isJumping = true;
        _velocity.y = jumpForce;
        
        // 애니메이터 업데이트
        if (_characterManager != null && _characterManager.animator != null)
        {
            _characterManager.animator.SetBool(_isJumpingHash, true);
        }
    }
    
    private void ApplyGravity()
    {
        if (_velocity.y > Physics.gravity.y)
        {
            _velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
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
    
    #region Public Methods
    
    /// <summary>
    /// 이동 방향을 설정합니다.
    /// </summary>
    /// <param name="direction">정규화된 이동 방향 벡터</param>
    public void SetMoveDirection(Vector3 direction)
    {
        _moveDirection = direction.normalized;
    }
    
    /// <summary>
    /// 현재 이동 속도를 설정합니다.
    /// </summary>
    /// <param name="speed">속도 (0 = 정지, 1 = 달리기)</param>
    public void SetMovementSpeed(float speed)
    {
        speed = Mathf.Clamp01(speed);
        _currentMaxSpeed = Mathf.Lerp(0f, runSpeed, speed);
    }
    
    /// <summary>
    /// 걷기/달리기 모드를 전환합니다.
    /// </summary>
    /// <param name="isWalking">true = 걷기, false = 달리기</param>
    public void SetWalkMode(bool isWalking)
    {
        _currentMaxSpeed = isWalking ? walkSpeed : runSpeed;
    }
    
    /// <summary>
    /// 점프를 수행합니다.
    /// </summary>
    public void PerformJump()
    {
        if (canMove && _isGrounded && !_isJumping)
        {
            Jump();
        }
    }
    
    /// <summary>
    /// 현재 이동 상태를 반환합니다.
    /// </summary>
    public bool IsMoving()
    {
        return _speed2D > 0.1f;
    }
    
    /// <summary>
    /// 현재 점프 상태를 반환합니다.
    /// </summary>
    public bool IsJumping()
    {
        return _isJumping;
    }
    
    /// <summary>
    /// 현재 지면 접촉 상태를 반환합니다.
    /// </summary>
    public bool IsGrounded()
    {
        return _isGrounded;
    }
    
    #endregion
    
    #region Gizmos
    
    private void OnDrawGizmosSelected()
    {
        // 지면 체크 구체 표시
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        Gizmos.DrawWireSphere(spherePosition, 0.2f);
    }
    
    #endregion
}
