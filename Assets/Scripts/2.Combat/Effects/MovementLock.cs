using System.Collections;
using UnityEngine;

public class MovementLock : MonoBehaviour
{
	private CharacterLocomotionManager _locomotion;
	private int _lockCount = 0;

	private void Awake()
	{
		_locomotion = GetComponent<CharacterLocomotionManager>();
		if (_locomotion == null)
		{
			_locomotion = GetComponentInParent<CharacterLocomotionManager>();
		}
	}

	public void Apply(float duration)
	{
		if (_locomotion == null) return;
		_lockCount++;
		_locomotion.canMove = false;
		StartCoroutine(ReleaseAfter(duration));
	}

	private IEnumerator ReleaseAfter(float duration)
	{
		yield return new WaitForSeconds(duration);
		_lockCount = Mathf.Max(0, _lockCount - 1);
		if (_locomotion != null && _lockCount == 0)
		{
			_locomotion.canMove = true;
		}
	}
}


