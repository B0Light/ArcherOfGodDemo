using UnityEngine;

namespace bkTools
{
	[CreateAssetMenu(menuName = "bkTools/Stats/StatSO", fileName = "StatSO")]
	public class StatSO : ScriptableObject
	{
		[SerializeField] private string id = "Health";
		[SerializeField] private float minValue = 0f;
		[SerializeField] private float maxValue = 100f;
		[SerializeField] private float startValue = 100f;
		[SerializeField] private float regenPerSecond = 0f;
		[SerializeField] private bool clampToBounds = true;
		[SerializeField] private bool enableRegeneration = true;

		public string Id => id;
		public float Min => minValue;
		public float Max => maxValue;
		public float StartValue => startValue;
		public float RegenPerSecond => regenPerSecond;
		public bool ClampToBounds => clampToBounds;
		public bool EnableRegeneration => enableRegeneration;

		public Stat CreateRuntimeInstance()
		{
			var s = new Stat();
			s.Initialize(id, minValue, maxValue, startValue, regenPerSecond, clampToBounds);
			s.EnableRegeneration = enableRegeneration;
			return s;
		}
	}
}
