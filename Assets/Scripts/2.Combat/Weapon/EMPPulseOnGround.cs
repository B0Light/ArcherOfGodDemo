using System.Collections.Generic;
using UnityEngine;

public class EMPPulseOnGround : MonoBehaviour
{
	[SerializeField] private float radius = 3f;
	[SerializeField] private float disableDuration = 3f;
	[SerializeField] private LayerMask groundMask = 1;
	[SerializeField] private LayerMask affectMask = ~0;
	[SerializeField] private GameObject vfxPrefab;

	private bool _triggered;
	private readonly Collider[] _hits = new Collider[32];

	public void Init(GameObject vfx, LayerMask groundMask, LayerMask affectMask, float radius = 3f, float duration = 3f)
	{
		this.vfxPrefab = vfx;
		this.groundMask = groundMask;
		this.affectMask = affectMask;
		this.radius = radius;
		this.disableDuration = duration;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_triggered) return;
		if (!IsInLayerMask(other.gameObject.layer, groundMask)) return;
		_triggered = true;
		TriggerAt(transform.position);
	}

	private void TriggerAt(Vector3 position)
	{
		if (vfxPrefab != null)
		{
			Instantiate(vfxPrefab, position, Quaternion.identity);
		}

		int hitCount = Physics.OverlapSphereNonAlloc(position, radius, _hits, affectMask, QueryTriggerInteraction.Ignore);
		var processed = new HashSet<CharacterLocomotionManager>();
		for (int i = 0; i < hitCount; i++)
		{
			var col = _hits[i];
			var locomotion = col.GetComponentInParent<CharacterLocomotionManager>();
			if (locomotion == null) continue;
			if (!processed.Add(locomotion)) continue;

			var lockComp = locomotion.GetComponent<MovementLock>();
			if (lockComp == null) lockComp = locomotion.gameObject.AddComponent<MovementLock>();
			lockComp.Apply(disableDuration);
		}
	}

	private static bool IsInLayerMask(int layer, LayerMask mask)
	{
		int layerMask = 1 << layer;
		return (mask.value & layerMask) != 0;
	}
}


