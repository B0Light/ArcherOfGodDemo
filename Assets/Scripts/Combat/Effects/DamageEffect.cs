using UnityEngine;

namespace bkTools.Combat
{
	public abstract class DamageEffect : ScriptableObject
	{
		public abstract void Apply(EffectManager manager, float currentHealth, float previousHealth, Damageable damageable);
	}
}


