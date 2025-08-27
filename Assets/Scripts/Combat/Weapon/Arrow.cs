using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private float baseSpeed = 40f;
    [SerializeField] private float lifeTime = 6f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody _rb;
    private Collider _collider;
    private ArrowPool _pool;
    private float _despawnTime;
    private bool _stuck = false;
    
    public float BaseSpeed => baseSpeed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        if (_rb) _rb.interpolation = RigidbodyInterpolation.Interpolate;

        // 화살이 트리거 충돌로만 판정되도록 설정
        if (_collider) _collider.isTrigger = true;
    }

    private void OnEnable()
    {
        _stuck = false;
        if (_rb)
        {
            _rb.isKinematic = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        if (_collider) _collider.enabled = true;

        _despawnTime = Time.time + lifeTime;
    }

    private void Update()
    {
        if (!_stuck && _rb != null && _rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            // 화살의 진행 방향을 속도 벡터에 맞춤
            transform.forward = _rb.linearVelocity.normalized;
        }

        if (!_stuck && Time.time >= _despawnTime)
        {
            Despawn();
        }
    }

    public void Launch(Vector3 direction, float extraSpeed)
    {
        _pool = ArrowPool.Instance;
        float speed = baseSpeed + extraSpeed;

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        if (_rb)
        {
            _rb.linearVelocity = direction.normalized * speed;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_stuck) return; 
        
        if ((targetLayer.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }
        
        Stick(other);
    }

    private void Stick(Collider other)
    {
        if (StuckArrowDecalPool.Instance != null)
        {
            RaycastHit hit;
            Vector3 moveDir = _rb != null && _rb.linearVelocity.sqrMagnitude > 0.0001f
                ? _rb.linearVelocity.normalized
                : transform.forward;

            float castDistance = Mathf.Max(0.1f, (_rb != null ? _rb.linearVelocity.magnitude : 0f) * Time.deltaTime * 2f);
            if (Physics.Raycast(transform.position - moveDir * 0.1f, moveDir, out hit, castDistance + 0.2f, ~0, QueryTriggerInteraction.Ignore))
            {
                StuckArrowDecal decal = StuckArrowDecalPool.Instance.Get();
                bool hitIsGround = (groundLayer.value & (1 << hit.collider.gameObject.layer)) != 0;
                Vector3 forwardForDecal = hitIsGround ? Vector3.down : moveDir;
                decal.SetFromHit(hit, forwardForDecal);
                decal.transform.SetParent(other.transform, true);
                decal.DespawnAfterTime();
            }
            else
            {
                StuckArrowDecal decal = StuckArrowDecalPool.Instance.Get();
                bool otherIsGround = (groundLayer.value & (1 << other.gameObject.layer)) != 0;
                if (otherIsGround)
                {
                    decal.transform.position = transform.position + Vector3.up * 0.02f;
                    decal.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.up);
                }
                else
                {
                    decal.SetTransform(transform);
                }
                decal.transform.SetParent(other.transform, true);
                decal.DespawnAfterTime();
            }
        }
        else
        {
            Debug.LogError("StuckArrowDecalPool is not assigned!");
        }
        Despawn();
    }

    private void Despawn()
    {
        CancelInvoke();
        transform.SetParent(null);

        if (_pool != null)
        {
            _pool.Return(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}