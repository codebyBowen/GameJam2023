using System;
using UnityEngine;
using System.Collections.Generic;

public class AttackProp : MonoBehaviour {
  public Phase phase;
  public ModableValue<float> damage;
  public DamageType damageType;
  public Action<Phase,Phase>? onPhaseChange;

  public AttackProp(Phase phase, float baseDamage, DamageType damageType = DamageType.Absolute, Action<Phase,Phase>? onPhaseChange = null) {
    this.phase = phase;
    this.damage = new ModableValue<float>(baseDamage);
    this.damageType = damageType;
    this.onPhaseChange = onPhaseChange;
  }

  public AttackProp(float baseDamage) : this(Phase.none, baseDamage) {
  }

  public void setPhase(Phase newP) {
    Phase oldP = phase;
    phase = newP;
    if(oldP != newP && onPhaseChange != null) {
      onPhaseChange(oldP, newP);
    }
  }

  public AttackProp Clone() {
    AttackProp obj = (AttackProp)this.MemberwiseClone();
    obj.damage = this.damage.Clone();
    return obj;
  }

}
