using UnityEngine;

namespace bkTools.Combat
{
	[AddComponentMenu("bkTools/Damage/Damager")] 
	public class Damager : MonoBehaviour
	{
		[Header("데미지 설정")]
		[SerializeField] private float damage = 10f;
		[SerializeField] private bool critical = false;

		[Header("히트 필터")]
		[SerializeField] private LayerMask hitMask = ~0;
		[SerializeField] private bool useTrigger = true;

		public void HitTarget(IDamageable target)
		{
			if (target == null) return;
			target.ReceiveDamage(new DamageInfo(
				amount: damage,
				direction: transform.forward,
				position: transform.position,
				source: gameObject,
				isCritical: critical));
		}

		void OnTriggerEnter(Collider other)
		{
			if (!useTrigger) return;
			ProcessHit(other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject, other.ClosestPoint(transform.position));
		}

		void OnCollisionEnter(Collision collision)
		{
			if (useTrigger) return;
			var contact = collision.GetContact(0);
			ProcessHit(collision.collider.attachedRigidbody ? collision.collider.attachedRigidbody.gameObject : collision.collider.gameObject, contact.point);
		}

		void ProcessHit(GameObject other, Vector3 hitPoint)
		{
			if (!IsInLayerMask(other.layer, hitMask)) return;
			var damageable = other.GetComponentInParent<Damageable>();
			if (damageable == null) return;

			Vector3 dir = (damageable.transform.position - transform.position);
			var info = new DamageInfo(damage, dir, hitPoint, gameObject, critical);
			damageable.ReceiveDamage(info);
		}

		static bool IsInLayerMask(int layer, LayerMask mask)
		{
			int layerMask = 1 << layer;
			return (mask.value & layerMask) != 0;
		}
	}
}


