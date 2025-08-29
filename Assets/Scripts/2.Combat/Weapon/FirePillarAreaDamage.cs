using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bkTools.Combat;

public class FirePillarAreaDamage : MonoBehaviour
{
	[Header("Area Damage Settings")]
	[SerializeField] private float radius = 2.5f;
	[SerializeField] private float dps = 5f;
	[SerializeField] private float duration = 5f;
	[SerializeField] private float tickInterval = 1f;
	[SerializeField] private LayerMask hitMask = ~0;
	[SerializeField] private bool destroyGameObjectOnFinish = true;

	private Vector3 _centerPosition;
	private bool _running;
	private readonly Collider[] _hits = new Collider[32];

	public void Init(float radius, float dps, float duration, float tickInterval, LayerMask hitMask)
	{
		this.radius = radius;
		this.dps = dps;
		this.duration = duration;
		this.tickInterval = tickInterval;
		this.hitMask = hitMask;
	}

	public void StartDamage(Vector3 position)
	{
		_centerPosition = position;
		if (_running) return;
		_running = true;
		StartCoroutine(TickRoutine());
	}

	private IEnumerator TickRoutine()
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			int hitCount = Physics.OverlapSphereNonAlloc(_centerPosition, radius, _hits, hitMask, QueryTriggerInteraction.Ignore);
			var processed = new HashSet<Damageable>();
			for (int i = 0; i < hitCount; i++)
			{
				var col = _hits[i];
				var dmg = col.GetComponentInParent<Damageable>();
				if (dmg == null) continue;
				if (!processed.Add(dmg)) continue;

				float tickDamage = dps * tickInterval;
				Vector3 dir = (dmg.transform.position - _centerPosition).normalized;
				dmg.ReceiveDamage(new DamageInfo(tickDamage, dir, _centerPosition, gameObject, false));
			}

			yield return new WaitForSeconds(tickInterval);
			elapsed += tickInterval;
		}

		_running = false;
		if (destroyGameObjectOnFinish)
		{
			Destroy(gameObject);
		}
	}
}


