using UnityEngine;

public class StuckArrowDecalPool : Pool<StuckArrowDecal, StuckArrowDecalPool>
{
    protected override StuckArrowDecal CreateNew()
    {
        var instance = base.CreateNew();
        return instance;
    }

    public override StuckArrowDecal Get()
    {
        if (prefab == null)
        {
            Debug.LogError("[StuckArrowDecalPool] Prefab이 설정되지 않았습니다.");
            return null;
        }
        var item = base.Get();
        if (item != null)
        {
            item.gameObject.SetActive(true);
        }
        return item;
    }
}