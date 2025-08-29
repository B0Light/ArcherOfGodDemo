using System.Collections.Generic;
using UnityEngine;
using bkTools.Combat;

public class ExplosionOnGround : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float damage = 20f;
    private LayerMask _groundMask;
    private LayerMask _hitMask;
    [SerializeField] private GameObject explosionVfxPrefab;

    private bool _exploded;

    // OverlapSphereNonAlloc 결과 저장용 캐시 배열
    private readonly Collider[] _hits = new Collider[32]; // 필요에 따라 크기 조정 가능

    public void Init(GameObject vfx, LayerMask groundMask, LayerMask hitMask)
    {
        explosionVfxPrefab = vfx;
        _groundMask = groundMask;
        _hitMask = hitMask;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (_exploded) return;
        if (!IsInLayerMask(other.gameObject.layer, _groundMask)) return;
        ExplodeAt(transform.position);
    }

    private void ExplodeAt(Vector3 position)
    {
        if (_exploded) return;
        _exploded = true;

        if (explosionVfxPrefab != null)
        {
            Instantiate(explosionVfxPrefab, position, Quaternion.identity);
        }

        // OverlapSphereNonAlloc 사용
        int hitCount = Physics.OverlapSphereNonAlloc(position, radius, _hits, _hitMask, QueryTriggerInteraction.Ignore);

        var processed = new HashSet<Damageable>();
        for (int i = 0; i < hitCount; i++)
        {
            var col = _hits[i];
            var dmg = col.GetComponentInParent<Damageable>();
            if (dmg == null) continue;
            if (!processed.Add(dmg)) continue;

            Vector3 dir = (dmg.transform.position - position).normalized;
            dmg.ReceiveDamage(new DamageInfo(damage, dir, position, gameObject, false));
        }
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        int layerMask = 1 << layer;
        return (mask.value & layerMask) != 0;
    }
}
