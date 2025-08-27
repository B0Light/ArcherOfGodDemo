using System;
using UnityEngine;

public class CharacterCombatManager : MonoBehaviour
{
    private CharacterManager _characterManager;
    
    [SerializeField] private BowShooter bowShooter;
    [SerializeField] private float fireRate = 1f;
    private float _nextFireTime;

    private void Awake()
    {
        _characterManager = GetComponent<CharacterManager>();
    }

    private void Update()
    {
        if (_characterManager.CanAttack() && Time.time >= _nextFireTime)
        {
            bowShooter.Shoot();
            _nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        }
    }
}
