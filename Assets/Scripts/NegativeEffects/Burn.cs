using UnityEngine;

public class Burn : PersistentDamage {
  public void init(float damagePerSec, float? durationSec) {
    base.init(new AttackProp(Phase.Fire, damagePerSec, DamageType.Internal), durationSec);
  }

  public static Burn attach(GameObject obj, float damagePerSec, float? durationSec) {
    Burn burn = obj.AddComponent<Burn>();
    burn.init(damagePerSec, durationSec);
    return burn;
  }
}
