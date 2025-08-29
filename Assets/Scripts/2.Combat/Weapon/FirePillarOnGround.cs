using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bkTools.Combat;

public class FirePillarOnGround : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float dps = 5f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float tickInterval = 1f;
    private LayerMask _groundMask;
    private LayerMask _hitMask;
    [SerializeField] private GameObject firePillarVfxPrefab;
    [SerializeField] private GameObject fireTrailVfxPrefab;
    private bool _spawned;
    private readonly Collider[] _hits = new Collider[32];

    public void Init(GameObject vfx,GameObject trailVfx, LayerMask groundMask, LayerMask hitMask)
    {
        firePillarVfxPrefab = vfx;
        fireTrailVfxPrefab = trailVfx;
        _groundMask = groundMask;
        _hitMask = hitMask;
    }

    public void InstantFireTrail()
    {
        if(fireTrailVfxPrefab)
            Instantiate(fireTrailVfxPrefab, this.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_spawned) return;
        if (!IsInLayerMask(other.gameObject.layer, _groundMask)) return;
        StartCoroutine(SpawnAndTickDamage(transform.position));
    }

    private IEnumerator SpawnAndTickDamage(Vector3 position)
    {
        if (_spawned) yield break;
        _spawned = true;

        GameObject vfxInstance = null;
        if (firePillarVfxPrefab != null)
        {
            vfxInstance = Instantiate(firePillarVfxPrefab, position, Quaternion.identity);
        }

        // 불기둥 소환 직후 화살은 풀로 반환하여 중복 충돌/소멸을 방지
        var arrow = GetComponent<Arrow>();
        if (arrow != null)
        {
            arrow.ForceDespawn();
        }

        // 틱 데미지는 VFX 오브젝트에 부착된 별도 컴포넌트가 담당
        if (vfxInstance != null)
        {
            var area = vfxInstance.GetComponent<FirePillarAreaDamage>();
            if (area == null) area = vfxInstance.AddComponent<FirePillarAreaDamage>();
            area.Init(radius, dps, duration, tickInterval, _hitMask);
            area.StartDamage(position);
        }

        // VFX 수명 관리는 FirePillarAreaDamage에서 수행
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        int layerMask = 1 << layer;
        return (mask.value & layerMask) != 0;
    }
}



