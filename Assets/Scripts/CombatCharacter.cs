using UnityEngine;

public class CombatCharacter : MonoBehaviour {
  [SerializeField] public Health health;
  public AttackProp attProp;

  public void takeDamage(AttackProp ap) {
    health.changeHP(-Damage.CalculateDamage(ap, attProp));
  }
}
