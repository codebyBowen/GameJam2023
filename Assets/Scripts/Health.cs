using UnityEngine;
using UnityEngine.UI;
using System;

public class Health : MonoBehaviour {
  public float maxHP;
  public float currentHP;
  public HealthBar? healthBar;
  public Action? dieCB;

  public void Start() {
    if(healthBar != null) {
      healthBar.SetMaxHealth(maxHP);
      healthBar.SetHealth(currentHP);
    }
  }

  public virtual void changeHP(float change) {
    currentHP = Math.Min(currentHP + change, maxHP);
    if(currentHP <= 0.0F) {
      currentHP = 0.0F;
      if(dieCB != null)
        dieCB();
    }
    if(healthBar != null) {
      healthBar.SetHealth(currentHP);
    }
  }
}
