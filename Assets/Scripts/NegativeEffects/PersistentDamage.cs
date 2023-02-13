using UnityEngine;

public class PersistentDamage : NegativeEffect {
  public AttackProp prop; // baseDamage is per sec

  public void init(AttackProp prop) {
    base.init();
    this.prop = prop;
  }

  void FixedUpdate() {
    AttackProp thisAtt = prop.Clone();
    thisAtt.baseDamage = prop.baseDamage * Time.deltaTime;
    this.gameObject.GetComponent<CombatCharacter>().takeDamage(thisAtt);
  }
}
