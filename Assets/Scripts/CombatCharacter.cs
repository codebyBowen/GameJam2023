using UnityEngine;
using BarthaSzabolcs.Tutorial_SpriteFlash;

public class CombatCharacter : MonoBehaviour {
  [SerializeField] public Health health;
  public AttackProp attProp;

  protected SimpleFlash flashEffect;

  void Start() {
    flashEffect = GetComponent<SimpleFlash>();
  }

  public virtual void takeDamage(AttackProp ap) {
    if (flashEffect) flashEffect.Flash();

    health.changeHP(-Damage.CalculateDamage(ap, attProp));
  }
}
