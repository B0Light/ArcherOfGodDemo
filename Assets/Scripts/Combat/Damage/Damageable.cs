using UnityEngine;
using UnityEngine.Events;

namespace bkTools.Combat
{
	[DisallowMultipleComponent]
	[AddComponentMenu("bkTools/Damage/Damageable")] 
	public class Damageable : MonoBehaviour, IDamageable
	{
		[Header("스탯 연결")]
		[SerializeField] private Stats stats;
		[SerializeField] private string healthStatId = "Health";
		[SerializeField] private bool createIfMissing = true;
		[SerializeField] private float defaultMax = 100f;
		[SerializeField] private float defaultStart = 100f;

		[Header("넉백 설정(선택)")]
		[SerializeField] private bool applyKnockback = false;
		[SerializeField] private float knockbackForce = 5f;
		[SerializeField] private ForceMode knockbackForceMode = ForceMode.Impulse;

		[Header("이벤트")]
		public UnityEvent<float> OnHealthChanged = new();
		public UnityEvent<float> OnDamaged = new();
		public UnityEvent OnDeath = new();

		private Stat cachedHealth;

		public float MaxHealth => cachedHealth != null ? cachedHealth.Max : 0f;
		public float CurrentHealth => cachedHealth != null ? cachedHealth.Current : 0f;
		public bool IsDead => cachedHealth != null && cachedHealth.IsEmpty;

		void Awake()
		{
			if (stats == null) TryGetComponent(out stats);
			SetupHealthRef();
			if (cachedHealth != null)
			{
				OnHealthChanged.Invoke(cachedHealth.Current);
			}
		}

		void OnDestroy()
		{
			if (cachedHealth != null)
			{
				cachedHealth.OnValueChanged.RemoveListener(HandleStatValueChanged);
			}
		}

		public void ReceiveDamage(DamageInfo info)
		{
			if (cachedHealth == null)
			{
				SetupHealthRef();
				if (cachedHealth == null) return;
			}
			if (IsDead) return;

			float damage = Mathf.Max(0f, info.amount);
			if (info.isCritical) damage *= 1.5f;

			cachedHealth.Add(-damage);
			OnDamaged.Invoke(damage);

			if (applyKnockback && damage > 0f)
			{
				var rb = GetComponent<Rigidbody>();
				if (rb != null)
				{
					Vector3 dir = info.direction.sqrMagnitude > 0.0001f ? info.direction.normalized : -transform.forward;
					rb.AddForce(dir * knockbackForce, knockbackForceMode);
				}
			}

			if (IsDead)
			{
				OnDeath.Invoke();
			}
		}

		public void AddHealth(float amount)
		{
			if (cachedHealth == null)
			{
				SetupHealthRef();
				if (cachedHealth == null) return;
			}
			cachedHealth.Add(amount);
		}

		void SetupHealthRef()
		{
			if (stats == null) return;
			if (!stats.TryGet(healthStatId, out var s))
			{
				if (createIfMissing)
				{
					s = stats.GetOrCreate(healthStatId, 0f, defaultMax, defaultStart);
				}
				else
				{
					return;
				}
			}
			if (cachedHealth == s) return;
			if (cachedHealth != null)
			{
				cachedHealth.OnValueChanged.RemoveListener(HandleStatValueChanged);
			}
			cachedHealth = s;
			cachedHealth.OnValueChanged.AddListener(HandleStatValueChanged);
		}

		void HandleStatValueChanged(float value)
		{
			OnHealthChanged.Invoke(value);
			if (IsDead) OnDeath.Invoke();
		}
	}
}


