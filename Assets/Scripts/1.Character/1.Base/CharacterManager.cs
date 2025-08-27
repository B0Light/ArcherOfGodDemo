using System;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    
    [HideInInspector] public CharacterVariableManager characterVariableManager;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
}
