using UnityEngine;

public class PersistentDamage : NegativeEffect {
  public AttackProp prop; // baseDamage is per sec

  public void init(AttackProp prop, float? durationSec) {
    base.init(durationSec);
    this.prop = prop;
  }

  void FixedUpdate() {
    AttackProp thisAtt = prop.Clone();
    thisAtt.damage.InitVal = prop.damage.InitVal * Time.deltaTime;
    this.gameObject.GetComponent<CombatCharacter>().takeDamage(thisAtt);
  }
}
