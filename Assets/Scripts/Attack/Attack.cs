using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Attack : MonoBehaviour {
  public AttackProp attProp;

  // Make readonly for implementation simplicity
  public readonly LayerMask targetLayers;

  protected void OnCollisionEnter2D(Collision2D collision) {
    if((targetLayers & (1 << collision.gameObject.layer)) != 0) {
      collision.gameObject.GetComponentInParent<CombatCharacter>().takeDamage(attProp);
    }
  }
}

