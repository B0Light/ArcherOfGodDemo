using System.Collections;
using bkTools.Combat;
using UnityEngine;

[CreateAssetMenu(menuName = "bkTools/Effects/Dead_Animation", fileName = "Dead_Animation")]
public class DeadEffect : DamageEffect
{
    public override void Apply(EffectManager manager, float currentHealth, float maxHealth, Damageable damageable)
    {
        if (currentHealth > 0) return;
        if (manager == null) return;
        manager.DeathProcess();
    }
}
