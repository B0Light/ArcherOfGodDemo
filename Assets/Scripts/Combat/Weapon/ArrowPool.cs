using UnityEngine;

public class ArrowPool : Pool<Arrow, ArrowPool>
{
    protected override Arrow CreateNew()
    {
        var instance = base.CreateNew();
        return instance;
    }
}
