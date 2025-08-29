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
    private GameObject _trailVfxInstance; 
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
        if (fireTrailVfxPrefab && _trailVfxInstance == null)
        {
            _trailVfxInstance = Instantiate(fireTrailVfxPrefab, this.transform);
        }
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

        // 화살 제거 전에 트레일도 함께 제거
        if (_trailVfxInstance != null)
        {
            Destroy(_trailVfxInstance);
            _trailVfxInstance = null;
        }

        var arrow = GetComponent<Arrow>();
        if (arrow != null)
        {
            arrow.ForceDespawn();
        }

        if (vfxInstance != null)
        {
            var area = vfxInstance.GetComponent<FirePillarAreaDamage>();
            if (area == null) area = vfxInstance.AddComponent<FirePillarAreaDamage>();
            area.Init(radius, dps, duration, tickInterval, _hitMask);
            area.StartDamage(position);
        }
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        int layerMask = 1 << layer;
        return (mask.value & layerMask) != 0;
    }
}



