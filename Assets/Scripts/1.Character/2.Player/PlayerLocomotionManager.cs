using UnityEngine;

public class PlayerLocomotionManager : CharacterLocomotionManager
{
    [Header("Platform Boundaries")]
    [SerializeField] private float platformSize = 7f;
    [SerializeField] private float boundaryBuffer = 0.5f;
    [SerializeField] private Transform platformCenterTransform;
    [SerializeField] private float platformY = 0f;
    
    [Header("Player Specific Settings")]
    [SerializeField] private float playerRadius = 0.3f;
    
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
        
        CheckPlatformBoundaries();
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
    
    private Vector3 ClampDirectionToPlatform(Vector3 direction)
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
    
    private void CheckPlatformBoundaries()
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
    
    public bool IsPositionSafe(Vector3 position)
    {
        Vector3 center = platformCenterTransform ? platformCenterTransform.position : Vector3.zero;
        Vector3 local = position - center;
        float halfSize = platformSize * 0.5f;
        float buffer = boundaryBuffer + playerRadius;
        
        return Mathf.Abs(local.x) <= halfSize - buffer && 
               Mathf.Abs(local.z) <= halfSize - buffer;
    }
    
    public Vector3 GetSafePosition(Vector3 targetPosition)
    {
        Vector3 center = platformCenterTransform ? platformCenterTransform.position : Vector3.zero;
        Vector3 local = targetPosition - center;
        float halfSize = platformSize * 0.5f;
        float buffer = boundaryBuffer + playerRadius;
        
        local.x = Mathf.Clamp(local.x, -halfSize + buffer, halfSize - buffer);
        local.z = Mathf.Clamp(local.z, -halfSize + buffer, halfSize - buffer);
        
        Vector3 safeWorld = center + local;
        safeWorld.y = platformY;
        return safeWorld;
    }
    
    public (float size, float boundary) GetPlatformInfo()
    {
        return (platformSize, boundaryBuffer);
    }
    
    #region Gizmos
    
    private void OnDrawGizmosSelected()
    {
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
    
    #endregion
}
