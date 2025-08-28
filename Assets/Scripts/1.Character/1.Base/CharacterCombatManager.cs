using System.Collections;
using UnityEngine;

public class CharacterCombatManager : MonoBehaviour
{
    private CharacterManager _characterManager;

    [SerializeField] private BowShooter bowShooter;

    private void Awake()
    {
        _characterManager = GetComponent<CharacterManager>();
    }
    
}