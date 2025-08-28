using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bkTools.Combat
{
	[CreateAssetMenu(menuName = "bkTools/Effects/Flash Color On Health Change", fileName = "FlashColorOnHealthChange")]
	public class FlashColorOnDamage : DamageEffect
	{
		[SerializeField] private Color flashColor = Color.red;
		[SerializeField] private float flashDuration = 1f;
		[SerializeField] private bool onlyWhenHealthDecreases = true;

		public override void Apply(EffectManager manager, float currentHealth, float previousHealth, Damageable damageable)
		{
			if (onlyWhenHealthDecreases && currentHealth >= previousHealth) return;
			if (manager == null) return;
			manager.StartCoroutine(Run(manager));
		}

		private IEnumerator Run(EffectManager manager)
		{
			// 대상
			var renderers = manager.TargetRenderers;
			if (renderers == null || renderers.Length == 0) yield break;

			// 색상 캐시 및 적용
			int baseColorId = Shader.PropertyToID("_BaseColor");
			int colorId = Shader.PropertyToID("_Color");
			manager.EnsureColorCaches(renderers, baseColorId, colorId);
			manager.SetRenderersColor(renderers, flashColor);
			yield return new WaitForSeconds(flashDuration);
			manager.RestoreOriginalColors(renderers);
		}
	}
}


