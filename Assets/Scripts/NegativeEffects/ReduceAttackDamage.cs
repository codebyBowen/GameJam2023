using UnityEngine;
using System.Linq.Expressions;

public class ReduceAttackDamage : NegativeEffect {
  public float factor;
  private ValueModification<float> __mod;

  public void init(float factor, float? durationSec) {
    base.init(durationSec);
    this.factor = factor;

    __mod = new ValueModification<float>(
          Expression.MakeBinary(ExpressionType.MultiplyChecked, ValueModification<float>.PlaceHolder, Expression.Constant(factor)),
          1
        );
    this.gameObject.GetComponent<CombatCharacter>().attProp.damage.Modifications.Add(__mod);
  }

  public static ReduceAttackDamage attach(GameObject obj, float factor, float? durationSec) {
    ReduceAttackDamage reduceAttackDamage = obj.AddComponent<ReduceAttackDamage>();
    reduceAttackDamage.init(factor, durationSec);
    return reduceAttackDamage;
  }

  void OnDestroy() {
    Debug.Assert(this.gameObject.GetComponent<CombatCharacter>().attProp.damage.Modifications.Remove(__mod));
  }
}
