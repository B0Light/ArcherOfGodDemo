using System.Collections;
using bkTools.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace bkTools.UI
{
	[DisallowMultipleComponent]
	[AddComponentMenu("bkTools/UI/Health Bar UI")] 
	public class HealthBarUI : MonoBehaviour
	{
		[SerializeField] private HealthBarEffect healthBarEffect;
		[SerializeField] private Image image;
		
		private void OnEnable()
		{
			if (healthBarEffect != null)
			{
				healthBarEffect.OnHealthChanged += UpdateUI;
				Debug.Log("Health Bar Bind Complete");
			}
		}

		private void OnDisable()
		{
			if (healthBarEffect != null)
				healthBarEffect.OnHealthChanged -= UpdateUI;
		}

		private void UpdateUI(float current, float max)
		{
			//Debug.Log("Current : " + current +"\n max :" + max);
			image.fillAmount = current / max;
		}

	}
}


