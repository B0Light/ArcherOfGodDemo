using System.Collections.Generic;
using UnityEngine;

public class ArrowPool : Singleton<ArrowPool>
{
    [Header("Pool Settings")]
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private int initialSize = 20;
    [SerializeField] private bool expandable = true;
    
    private readonly Queue<Arrow> _pool = new Queue<Arrow>();
    
    protected override void Awake()
    {
        base.Awake();
        Prewarm();
    }
    
    private void Prewarm()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("[ArrowPool] Arrow Prefab이 설정되지 않았습니다.");
            return;
        }
        
        for (int i = 0; i < initialSize; i++)
        {
            var arrow = CreateNew(false);
            _pool.Enqueue(arrow);
        }
    }
    
    private Arrow CreateNew(bool active)
    {
        // Instantiate를 통해 새로운 화살을 생성하고 이 Pool 객체의 자식으로 설정합니다.
        var obj = Instantiate(arrowPrefab, transform);
        obj.gameObject.SetActive(false);
        return obj;
    }
    
    public Arrow Get()
    {
        Arrow arrow = null;
        // 풀에서 사용 가능한 화살을 찾습니다.
        while (_pool.Count > 0)
        {
            arrow = _pool.Dequeue();
            // 비활성화된 오브젝트인지 확인
            if (arrow != null && !arrow.gameObject.activeSelf)
            {
                break;
            }
        }
        
        // 사용 가능한 화살이 없는 경우
        if (arrow == null || arrow.gameObject.activeSelf)
        {
            if (!expandable)
            {
                Debug.LogWarning("[ArrowPool] 풀이 비어 있고 확장이 비활성화되어 있습니다.");
                return null;
            }
            arrow = CreateNew(false);
        }

        // 화살 활성화는 호출자에서 수행합니다.
        return arrow;
    }


    public void Return(Arrow arrow)
    {
        if (arrow == null) return;
        arrow.gameObject.SetActive(false);
        // 부모를 다시 이 풀 객체로 설정하여 계층 구조를 정리합니다.
        arrow.transform.SetParent(transform);
        _pool.Enqueue(arrow);
    }
}
