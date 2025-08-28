using System.Collections.Generic;
using UnityEngine;

public abstract class Pool<TItem, TSelf> : Singleton<TSelf>
    where TItem : Component, IPoolGameObject
    where TSelf : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] protected TItem prefab;
    [SerializeField] protected int initialSize = 10;
    [SerializeField] protected bool expandable = true;

    protected readonly Queue<TItem> pooledItems = new Queue<TItem>();

    protected override void Awake()
    {
        base.Awake();
        Prewarm();
    }

    void Prewarm()
    {
        if (prefab == null)
        {
            Debug.LogError($"[{GetType().Name}] Prefab이 설정되지 않았습니다.");
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            var item = CreateNew();
            pooledItems.Enqueue(item);
        }
    }

    protected virtual TItem CreateNew()
    {
        var instance = Instantiate(prefab, transform);
        instance.gameObject.SetActive(false);
        // 풀 참조가 필요한 경우를 위해 인터페이스 통해 설정
        instance.Pool = null; // 외부 풀 타입과 호환되지 않으므로 생략 가능. 필요 시 구체 풀에서 설정.
        return instance;
    }

    public virtual TItem Get()
    {
        TItem item = null;
        while (pooledItems.Count > 0)
        {
            item = pooledItems.Dequeue();
            if (item != null && !item.gameObject.activeSelf) break;
        }

        if (item == null || item.gameObject.activeSelf)
        {
            if (!expandable)
            {
                Debug.LogWarning($"[{GetType().Name}] 풀이 비어 있고 확장이 비활성화되어 있습니다.");
                return null;
            }
            item = CreateNew();
        }

        // 가져올 때 인터페이스 콜백 제공 가능 (필요 시 확장)
        return item;
    }

    public virtual void Return(TItem item)
    {
        if (item == null) return;
        // 인터페이스의 Release 훅이 있으면 호출하고 비활성화
        item.Pool_Release(item.gameObject);
        item.transform.SetParent(transform);
        pooledItems.Enqueue(item);
    }
}


