using UnityEngine;

public class CombatCharacter : MonoBehaviour {
  [SerializeField] public Health health;

  public void takeDamage(AttackProp ap) {
    health.changeHP(-ap.baseDamage);
  }
}
