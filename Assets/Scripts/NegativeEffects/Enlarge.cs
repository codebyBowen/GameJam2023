using UnityEngine;

public class Enlarge : NegativeEffect {

  public Vector3 originalScale;
  public float factor;
  public ReduceAttackDamage reduceAttackDamage;

  public void init(float factor, float? durationSec) {
    base.init(durationSec);
    this.factor = factor;
    this.originalScale = gameObject.transform.localScale;
    gameObject.transform.localScale = new Vector3(factor, factor, factor);
    gameObject.GetComponent<Rigidbody2D>().mass *= factor;
    this.reduceAttackDamage = ReduceAttackDamage.attach(gameObject, factor, durationSec);
  }

  public static Enlarge attach(GameObject obj, float factor, float? durationSec) {
    // For now, at most only one at the same time on any gameobject
    foreach(Enlarge enObj in obj.GetComponents<Enlarge>()) {
      Destroy(enObj);
    }
    Enlarge enlarge = obj.AddComponent<Enlarge>();
    enlarge.init(factor, durationSec);
    return enlarge;
  }

  void OnDestroy() {
    Destroy(reduceAttackDamage);
    gameObject.transform.localScale = originalScale;
    // TODO: handle properly
    gameObject.GetComponent<Rigidbody2D>().mass /= factor;
  }
}
