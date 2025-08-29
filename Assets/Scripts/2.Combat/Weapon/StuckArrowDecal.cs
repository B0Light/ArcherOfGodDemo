using UnityEngine;

public class StuckArrowDecal : MonoBehaviour, IPoolGameObject
{
    [SerializeField] private float stickTime = 2f; // 잔상 효과 지속 시간

    public void SetTransform(Transform tr)
    {
        this.transform.position = tr.position;
        this.transform.rotation = tr.rotation;
    }

    public void SetFromHit(RaycastHit hit, Vector3 forward, float surfaceOffset = 0.02f)
    {
        // 표면에 약간 띄워 배치하여 z-fighting 방지
        this.transform.position = hit.point + hit.normal * Mathf.Max(0f, surfaceOffset);
        // 화살 진행방향은 forward, 업 벡터는 표면 노말을 사용
        this.transform.rotation = Quaternion.LookRotation(forward.normalized, hit.normal);
    }

    public void DespawnAfterTime()
    {
        Invoke(nameof(Despawn), stickTime);
    }

    private void Despawn()
    {
        CancelInvoke();
        if (StuckArrowDecalPool.Instance != null)
        {
            StuckArrowDecalPool.Instance.Return(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // IPoolGameObject 구현
    public UnityEngine.Pool.IObjectPool<GameObject> Pool { get; set; }
    public void Pool_Release(GameObject gameObject)
    {
        if (gameObject != null)
        {
            gameObject.SetActive(false);
        }
    }
    public GameObject Pool_Get()
    {
        return gameObject;
    }
}