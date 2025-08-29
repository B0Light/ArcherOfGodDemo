using System.Collections;
using TMPro;
using UnityEngine;

public class ActionPointUI : MonoBehaviour
{
	[Header("Refs")]
	[SerializeField] private CharacterManager characterManager;
	[SerializeField] private TextMeshProUGUI totalText;

	private void Awake()
	{
		if (characterManager == null)
		{
			characterManager = GetComponentInParent<CharacterManager>();
		}
	}

	private void OnEnable()
	{
		Subscribe();
		RefreshTotal();
	}

	private void Subscribe()
	{
		if (characterManager != null)
		{
			characterManager.actionPoint.OnValueChanged += i => HandleShootArrow();
		}
	}

	private void HandleShootArrow()
	{
		StartCoroutine(RefreshNextFrame());
	}

	private IEnumerator RefreshNextFrame()
	{
		yield return null;
		RefreshTotal();
	}

	private void RefreshTotal()
	{
		if (totalText == null || characterManager == null) return;
		totalText.text = characterManager.actionPoint.Value.ToString();
	}
	
}


