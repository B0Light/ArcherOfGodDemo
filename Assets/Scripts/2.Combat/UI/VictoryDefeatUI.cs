using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class VictoryDefeatUI : MonoBehaviour
{
	[SerializeField] private CanvasGroup bannerGroup;
	[SerializeField] private TextMeshProUGUI bannerText;
	[SerializeField] private string victoryText = "VICTORY";
	[SerializeField] private string defeatText = "DEFEAT";
	[SerializeField] private float fadeDuration = 0.6f;

	private void Start()
	{
		bannerGroup.alpha = 0f;
		bannerGroup.interactable = false;
		bannerGroup.blocksRaycasts = false;
	}

	public void ShowVictory(float delaySeconds)
	{
		StartCoroutine(ShowAfterDelay(victoryText, delaySeconds));
	}

	public void ShowDefeat(float delaySeconds)
	{
		StartCoroutine(ShowAfterDelay(defeatText, delaySeconds));
	}

	private IEnumerator ShowAfterDelay(string text, float delay)
	{
		if (bannerGroup == null || bannerText == null) yield break;
		yield return new WaitForSeconds(Mathf.Max(0f, delay));
		bannerText.text = text;
		bannerGroup.gameObject.SetActive(true);
		bannerGroup.alpha = 0f;
		float t = 0f;
		while (t < fadeDuration)
		{
			t += Time.deltaTime;
			bannerGroup.alpha = Mathf.Clamp01(t / fadeDuration);
			yield return null;
		}
		bannerGroup.alpha = 1f;
		bannerGroup.interactable = true;
		bannerGroup.blocksRaycasts = true;
	}
}


