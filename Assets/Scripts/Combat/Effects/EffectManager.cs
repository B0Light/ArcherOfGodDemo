using System.Collections.Generic;
using UnityEngine;

namespace bkTools.Combat
{
	[DisallowMultipleComponent]
	[AddComponentMenu("bkTools/Effects/Effect Manager")] 
	public class EffectManager : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private Damageable damageable;
		[SerializeField] private Renderer[] targetRenderers;
		[SerializeField] private bool includeInactiveChildren = true;

		[Header("Registered Effects")]
		[SerializeField] private List<DamageEffect> healthChangeEffects = new();

		// Renderer 색상/메테리얼 캐시
		private readonly Dictionary<Renderer, Color[]> _originalColors = new();
		private readonly Dictionary<Renderer, int[]> _colorPropIds = new();
		private readonly Dictionary<Renderer, Material[]> _originalSharedMaterials = new();

		public Renderer[] TargetRenderers => targetRenderers;
		private float _lastHealth = float.NaN;

		void Awake()
		{
			if (damageable == null) TryGetComponent(out damageable);
			if (targetRenderers == null || targetRenderers.Length == 0)
			{
				targetRenderers = GetComponentsInChildren<Renderer>(includeInactiveChildren);
			}
		}

		void OnEnable()
		{
			if (damageable != null)
			{
				damageable.OnHealthChanged.AddListener(OnHealthChanged);
			}
		}

		void OnDisable()
		{
			if (damageable != null)
			{
				damageable.OnHealthChanged.RemoveListener(OnHealthChanged);
			}
		}
		
		private void OnHealthChanged(float currentHealth)
		{
			float prev = _lastHealth;
			_lastHealth = currentHealth;
			foreach (var effect in healthChangeEffects)
			{
				if (effect == null) continue;
				effect.Apply(this, currentHealth, prev, damageable);
			}
		}


		#region color

		// 색상 캐시 보장
		public void EnsureColorCaches(Renderer[] renderers, int baseColorId, int colorId)
		{
			_originalColors.Clear();
			_colorPropIds.Clear();
			foreach (var r in renderers)
			{
				if (r == null) continue;
				var sharedMats = r.sharedMaterials;
				if (sharedMats == null || sharedMats.Length == 0) continue;

				var colors = new Color[sharedMats.Length];
				var propIds = new int[sharedMats.Length];
				for (int i = 0; i < sharedMats.Length; i++)
				{
					var mat = sharedMats[i];
					if (mat == null)
					{
						propIds[i] = -1;
						continue;
					}
					int propId = mat.HasProperty(baseColorId) ? baseColorId : (mat.HasProperty(colorId) ? colorId : -1);
					propIds[i] = propId;
					colors[i] = propId == -1 ? Color.white : mat.GetColor(propId);
				}
				_originalColors[r] = colors;
				_colorPropIds[r] = propIds;
			}
		}

		// 즉시 색상 설정 (MPB 사용)
		public void SetRenderersColor(Renderer[] renderers, Color color)
		{
			var block = new MaterialPropertyBlock();
			foreach (var r in renderers)
			{
				if (r == null) continue;
				if (!_colorPropIds.TryGetValue(r, out var propIds)) continue;
				for (int i = 0; i < propIds.Length; i++)
				{
					int propId = propIds[i];
					if (propId == -1) continue;
					r.GetPropertyBlock(block, i);
					block.SetColor(propId, color);
					r.SetPropertyBlock(block, i);
				}
			}
		}

		public void RestoreOriginalColors(Renderer[] renderers)
		{
			var block = new MaterialPropertyBlock();
			foreach (var r in renderers)
			{
				if (r == null) continue;
				if (!_originalColors.TryGetValue(r, out var colors)) continue;
				if (!_colorPropIds.TryGetValue(r, out var propIds)) continue;
				int len = Mathf.Min(colors.Length, propIds.Length);
				for (int i = 0; i < len; i++)
				{
					int propId = propIds[i];
					if (propId == -1) continue;
					r.GetPropertyBlock(block, i);
					block.SetColor(propId, colors[i]);
					r.SetPropertyBlock(block, i);
				}
			}
		}

		// 메테리얼 교체용 캐시 빌드
		public void EnsureMaterialCaches(Renderer[] renderers)
		{
			_originalSharedMaterials.Clear();
			foreach (var r in renderers)
			{
				if (r == null) continue;
				_originalSharedMaterials[r] = r.sharedMaterials;
			}
		}

		// 모든 서브메시에 동일한 플래시 메테리얼 적용
		public void SetSharedMaterials(Renderer[] renderers, Material replacement)
		{
			if (replacement == null) return;
			foreach (var r in renderers)
			{
				if (r == null) continue;
				var src = r.sharedMaterials;
				if (src == null || src.Length == 0)
				{
					r.sharedMaterial = replacement;
					continue;
				}
				var arr = new Material[src.Length];
				for (int i = 0; i < arr.Length; i++) arr[i] = replacement;
				r.sharedMaterials = arr;
			}
		}

		public void RestoreSharedMaterials(Renderer[] renderers)
		{
			foreach (var r in renderers)
			{
				if (r == null) continue;
				if (!_originalSharedMaterials.TryGetValue(r, out var mats)) continue;
				r.sharedMaterials = mats;
			}
		}

		#endregion

		
	}
}


