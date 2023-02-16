using UnityEngine;

public abstract class NegativeEffect : MonoBehaviour {
  public void init(float? durationSec) {
    if(durationSec != null) {
      Destroy(this, durationSec.Value);
    }
  }
}
