using System.Linq;
using bkTools.UI;
using UnityEngine;
using UnityEngine.Events;

namespace bkTools.Combat
{
	[CreateAssetMenu(menuName = "bkTools/Effects/Health Bar On Health Change", fileName = "HealthBarEffect")]
	public class HealthBarEffect : DamageEffect
	{
		
		public UnityAction<float, float> OnHealthChanged; 
		
		public override void Apply(EffectManager manager, float currentHealth, float maxHealth, Damageable damageable)
		{
			OnHealthChanged?.Invoke(currentHealth, maxHealth);
		}

		
	}
}


