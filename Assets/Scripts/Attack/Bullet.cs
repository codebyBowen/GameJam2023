using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : Attack {
  public void Init(Vector2 initPos, Vector2 velocity) {
    transform.position = initPos;
    gameObject.GetComponent<Rigidbody2D>().velocity = velocity;
  }

  protected new void OnCollisionEnter2D(Collision2D collision) {
    base.OnCollisionEnter2D(collision);
    Destroy(this);
  }

}
