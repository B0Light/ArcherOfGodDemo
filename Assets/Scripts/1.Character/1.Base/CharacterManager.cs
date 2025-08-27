using System;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    
    [HideInInspector] public CharacterVariableManager characterVariableManager;

    public Variable<bool> isDead = new Variable<bool>(false);
    public bool isPerformingAction = false;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
}
