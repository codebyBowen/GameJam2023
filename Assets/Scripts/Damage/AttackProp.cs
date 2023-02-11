public class AttackProp {
  public Phase phase;
  public float baseDamage;

  public AttackProp(Phase phase, float baseDamage) : this(baseDamage) {
    this.phase = phase;
  }

  public AttackProp(float baseDamage) {
    this.baseDamage = baseDamage;
  }
}
