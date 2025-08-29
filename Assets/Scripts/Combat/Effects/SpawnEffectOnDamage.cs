using System.Collections;
using UnityEngine;

namespace bkTools.Combat
{
	[CreateAssetMenu(menuName = "bkTools/Effects/Spawn Effect On Damage", fileName = "SpawnEffectOnDamage")]
	public class SpawnEffectOnDamage : DamageEffect
	{
		[Header("Effect Prefab")]
		[SerializeField] private GameObject effectPrefab;

		[Header("Trigger Options")]
		[SerializeField] private bool onlyWhenHealthDecreases = true;

		[Header("Spawn Options")]
		[SerializeField] private Transform spawnPointOverride;
		[SerializeField] private Vector3 positionOffset = Vector3.zero;
		[SerializeField] private bool attachToTarget = false;
		[SerializeField] private bool useRendererBoundsCenter = false;

		[Header("Lifetime Options")]
		[SerializeField] private bool autoDestroyByParticleSystems = true;
		[SerializeField] private float additionalLifetimePadding = 0.25f;
		[SerializeField] private float destroyAfterSeconds = 2f;

		public override void Apply(EffectManager manager, float currentHealth, float maxHealth, Damageable damageable)
		{
			if (onlyWhenHealthDecreases && currentHealth >= maxHealth) return;
			if (manager == null) return;
			if (effectPrefab == null) return;
			manager.StartCoroutine(SpawnRoutine(manager, damageable));
		}

		private IEnumerator SpawnRoutine(EffectManager manager, Damageable damageable)
		{
			Transform parent = attachToTarget ? manager.transform : null;
			Vector3 pos = GetSpawnPosition(manager);
			Quaternion rot = (parent != null ? parent.rotation : manager.transform.rotation);

			GameObject instance = Object.Instantiate(effectPrefab, pos, rot, parent);

			if (instance == null) yield break;

			if (autoDestroyByParticleSystems)
			{
				float lifetime = CalculateMaxParticleSystemDuration(instance) + Mathf.Max(0f, additionalLifetimePadding);
				if (lifetime > 0f)
				{
					yield return new WaitForSeconds(lifetime);
					Object.Destroy(instance);
					yield break;
				}
			}

			yield return new WaitForSeconds(Mathf.Max(0f, destroyAfterSeconds));
			Object.Destroy(instance);
		}

		private Vector3 GetSpawnPosition(EffectManager manager)
		{
			if (spawnPointOverride != null)
			{
				return spawnPointOverride.position + positionOffset;
			}

			if (useRendererBoundsCenter)
			{
				var renderers = manager.TargetRenderers;
				if (renderers != null && renderers.Length > 0)
				{
					bool initialized = false;
					Bounds b = new Bounds();
					for (int i = 0; i < renderers.Length; i++)
					{
						var r = renderers[i];
						if (r == null) continue;
						if (!initialized)
						{
							b = r.bounds;
							initialized = true;
						}
						else
						{
							b.Encapsulate(r.bounds);
						}
					}
					if (initialized)
					{
						return b.center + positionOffset;
					}
				}
			}

			return manager.transform.position + positionOffset;
		}

		private float CalculateMaxParticleSystemDuration(GameObject instance)
		{
			float maxDuration = 0f;
			var particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
			for (int i = 0; i < particleSystems.Length; i++)
			{
				var ps = particleSystems[i];
				var main = ps.main;
				float duration = main.duration;
				// 최대 예상 수명: 시스템 duration + startLifetime 최대치
				float startLifetimeMax = 0f;
				var startLifetime = main.startLifetime;
				if (startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
					startLifetimeMax = Mathf.Max(startLifetime.constantMax, startLifetime.constantMin);
				else if (startLifetime.mode == ParticleSystemCurveMode.Constant)
					startLifetimeMax = startLifetime.constant;
				else
					startLifetimeMax = main.startLifetimeMultiplier; // 근사치

				float estimated = duration + startLifetimeMax;
				if (main.loop)
				{
					// 루프면 자동 계산 불가 → 0 유지하여 fallback 사용
					return 0f;
				}
				if (estimated > maxDuration) maxDuration = estimated;
			}
			return maxDuration;
		}
	}
}



