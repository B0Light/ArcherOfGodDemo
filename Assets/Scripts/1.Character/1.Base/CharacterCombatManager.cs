using System.Collections;
using UnityEngine;

public class CharacterCombatManager : MonoBehaviour
{
    private CharacterManager _characterManager;

    [SerializeField] private BowShooter bowShooter;
    [SerializeField] private float fireRate = 3f;

    private void Awake()
    {
        _characterManager = GetComponent<CharacterManager>();
    }

    private void Start()
    {
        StartCoroutine(AutoShootCoroutine());
    }

    private IEnumerator AutoShootCoroutine()
    {
        while (true)
        {
            if (_characterManager.CanAttack())
            {
                ShootArrow();
                yield return new WaitForSeconds(1f / Mathf.Max(0.01f, fireRate));
            }
            else
            {
                yield return null;
            }
        }
    }

    public void ShootArrow()
    {
        bowShooter.Shoot();
    }
}