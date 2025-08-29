using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bkTools.Combat
{
	[DisallowMultipleComponent]
	[AddComponentMenu("bkTools/Visual/Hit Flash On Health Change")] 
	public class HitFlashOnHealthChange : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private Damageable damageable;
		[SerializeField] private Renderer[] targetRenderers;
		[SerializeField] private bool includeInactiveChildren = true;

		[Header("Flash Settings")]
		[SerializeField] private Color flashColor = Color.red;
		[SerializeField] private float flashDuration = 1f;
		[SerializeField] private bool onlyWhenHealthDecreases = true;

		private readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
		private readonly int _colorId = Shader.PropertyToID("_Color");

		private readonly Dictionary<Renderer, Color[]> _originalColors = new();
		private readonly Dictionary<Renderer, int[]> _colorPropIds = new();
		private Coroutine _flashRoutine;
		private float _lastHealth = float.NaN;

		void Awake()
		{
			if (damageable == null) damageable = GetComponentInParent<Damageable>();
			if (targetRenderers == null || targetRenderers.Length == 0)
			{
				targetRenderers = GetComponentsInChildren<Renderer>(includeInactiveChildren);
			}
		}

		void OnEnable()
		{
			if (damageable != null)
			{
				_lastHealth = damageable.CurrentHealth;
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

		// UnityEvent<float>에 바인딩할 핸들러
		public void OnHealthChanged(float currentHealth)
		{
			bool shouldFlash = true;
			if (onlyWhenHealthDecreases)
			{
				if (float.IsNaN(_lastHealth))
				{
					// 초기화 단계에서는 플래시하지 않음
					shouldFlash = false;
				}
				else
				{
					shouldFlash = currentHealth < _lastHealth;
				}
			}
			_lastHealth = currentHealth;

			if (!shouldFlash) return;
			TriggerFlash();
		}

		private void TriggerFlash()
		{
			if (_flashRoutine != null)
			{
				StopCoroutine(_flashRoutine);
			}
			CacheOriginalsIfNeeded();
			_flashRoutine = StartCoroutine(FlashCoroutine());
		}

		private IEnumerator FlashCoroutine()
		{
			SetRenderersColor(flashColor);
			yield return new WaitForSeconds(flashDuration);
			RestoreOriginalColors();
			_flashRoutine = null;
		}

		private void CacheOriginalsIfNeeded()
		{
			_originalColors.Clear();
			_colorPropIds.Clear();

			foreach (var r in targetRenderers)
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

					int propId = mat.HasProperty(_baseColorId) ? _baseColorId : (mat.HasProperty(_colorId) ? _colorId : -1);
					propIds[i] = propId;
					if (propId == -1)
					{
						colors[i] = Color.white;
						continue;
					}
					colors[i] = mat.GetColor(propId);
				}

				_originalColors[r] = colors;
				_colorPropIds[r] = propIds;
			}
		}

		private void SetRenderersColor(Color color)
		{
			var block = new MaterialPropertyBlock();
			foreach (var kv in _colorPropIds)
			{
				var r = kv.Key;
				if (r == null) continue;
				var propIds = kv.Value;
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

		private void RestoreOriginalColors()
		{
			var block = new MaterialPropertyBlock();
			foreach (var r in targetRenderers)
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
	}
}


