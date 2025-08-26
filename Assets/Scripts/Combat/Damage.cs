using UnityEngine;

namespace bkTools.Combat
{
	public struct DamageInfo
	{
		public float amount;
		public Vector3 direction;
		public Vector3 position;
		public GameObject source;
		public bool isCritical;

		public DamageInfo(float amount, Vector3 direction, Vector3 position, GameObject source, bool isCritical)
		{
			this.amount = amount;
			this.direction = direction;
			this.position = position;
			this.source = source;
			this.isCritical = isCritical;
		}
	}

	public interface IDamageable
	{
		void ReceiveDamage(DamageInfo info);
	}
}


