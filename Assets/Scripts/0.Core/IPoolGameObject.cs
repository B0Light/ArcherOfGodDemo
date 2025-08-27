using UnityEngine;
using UnityEngine.Pool;

public interface IPoolGameObject
{
    IObjectPool<GameObject> Pool { get; set; }

    void Pool_Release(GameObject gameObject);
    GameObject Pool_Get();
}
