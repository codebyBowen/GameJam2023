using System;
using UnityEngine;

public class AttackProp : MonoBehaviour {
  public Phase phase;
  public float baseDamage;
  public Action<Phase,Phase>? onPhaseChange;

  public AttackProp(Phase phase, float baseDamage, Action<Phase,Phase>? onPhaseChange = null) {
    this.phase = phase;
    this.baseDamage = baseDamage;
    this.onPhaseChange = onPhaseChange;
  }

  public AttackProp(float baseDamage) : this(Phase.none, baseDamage, null) {
  }

  public void setPhase(Phase newP) {
    Phase oldP = phase;
    phase = newP;
    if(phase != newP && onPhaseChange != null) {
      onPhaseChange(oldP, newP);
    }
  }

}
