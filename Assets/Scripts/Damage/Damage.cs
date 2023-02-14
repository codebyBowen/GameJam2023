using System;
using System.Linq;
using UnityEngine;

public static class Damage {

  static readonly float[,] phaseMultiplyer = new float[Enum.GetNames(typeof(Phase)).Length,Enum.GetNames(typeof(Phase)).Length]; //1st dimension is the attacker's phase

  static Damage() {
    foreach(Phase p in Enum.GetValues(typeof(Phase))) {
      phaseMultiplyer[(int)Phase.none,(int)p] = phaseMultiplyer[(int)p,(int)p] = 1.0F;
      phaseMultiplyer[(int)p,(int)Phase.none] = 1.3F;
    }

    // Overcoming + Generating
    phaseMultiplyer[(int)Phase.Wood,(int)Phase.Water] = 1.5F;
    phaseMultiplyer[(int)Phase.Water,(int)Phase.Wood] = 0.5F;
    phaseMultiplyer[(int)Phase.Water,(int)Phase.Fire] = 1.5F;
    phaseMultiplyer[(int)Phase.Fire,(int)Phase.Water] = 0.5F;
    phaseMultiplyer[(int)Phase.Fire,(int)Phase.Wood] = 1.5F;
    phaseMultiplyer[(int)Phase.Wood,(int)Phase.Fire] = 0.5F;

    foreach(float f in phaseMultiplyer) {
      Debug.Assert(f != 0.0F);
    }
  }

  static public float CalculateDamage(AttackProp attacker, AttackProp victim) {
    return attacker.damage.Get() * phaseMultiplyer[(int)attacker.phase,(int)victim.phase];
  }
}

