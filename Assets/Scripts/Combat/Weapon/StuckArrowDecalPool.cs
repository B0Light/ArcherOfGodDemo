using System.Collections.Generic;
using UnityEngine;

public class StuckArrowDecalPool : Singleton<StuckArrowDecalPool>
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 10;
    
    private Queue<StuckArrowDecal> _pool = new Queue<StuckArrowDecal>();

    protected override void Awake()
    {
        base.Awake();
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            StuckArrowDecal decal = CreateNewDecal();
            _pool.Enqueue(decal);
        }
    }

    private StuckArrowDecal CreateNewDecal()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        StuckArrowDecal decal = obj.GetComponent<StuckArrowDecal>();
        return decal;
    }

    public StuckArrowDecal Get()
    {
        if (_pool.Count > 0)
        {
            StuckArrowDecal decal = _pool.Dequeue();
            decal.gameObject.SetActive(true);
            return decal;
        }
        else
        {
            // 풀이 부족하면 새로 생성
            StuckArrowDecal newDecal = CreateNewDecal();
            newDecal.gameObject.SetActive(true);
            return newDecal;
        }
    }

    public void Return(StuckArrowDecal decal)
    {
        decal.gameObject.SetActive(false);
        decal.transform.SetParent(transform);
        _pool.Enqueue(decal);
    }
}