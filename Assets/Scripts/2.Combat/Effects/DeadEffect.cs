using System.Collections;
using bkTools.Combat;
using UnityEngine;

[CreateAssetMenu(menuName = "bkTools/Effects/Dead_Animation", fileName = "Dead_Animation")]
public class DeadEffect : DamageEffect
{
    [SerializeField] private string deadAnimation = "Dead";
    public override void Apply(EffectManager manager, float currentHealth, float maxHealth, Damageable damageable)
    {
        if (currentHealth > 0) return;
        if (manager == null) return;
        manager.StartCoroutine(Run(manager));
    }

    private IEnumerator Run(EffectManager manager)
    {
        yield return new WaitForFixedUpdate();
        
        manager.PlayAnimation(deadAnimation);
        manager.DeathProcess();

    }
}
