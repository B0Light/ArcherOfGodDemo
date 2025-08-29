using UnityEngine;

public class DisableGravityDuringFlight : MonoBehaviour
{
	[SerializeField] private bool disableGravity = true;
	[SerializeField] private float maxLifetime = 6f;

	private Rigidbody _rb;
	private float _endTime;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
	}

	private void OnEnable()
	{
		_endTime = Time.time + maxLifetime;
		if (_rb)
		{
			_rb.useGravity = !disableGravity;
		}
	}

	private void OnDisable()
	{
		if (_rb)
		{
			_rb.useGravity = true;
		}
	}

	private void Update()
	{
		if (Time.time >= _endTime)
		{
			// 자동으로 제거되도록 함 (풀 복귀 시 OnDisable에서 중력 복구)
			disableGravity = false; // 안전 장치
			enabled = false;
		}
	}
}


