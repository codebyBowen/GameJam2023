using UnityEngine;
using System;
using System.Collections.Generic;

public static class Utils {
  public static int[] layersFromLayerMask(LayerMask lm) {
    List<int> layers = new List<int>();
    uint bitstring = (uint)lm.value;
    for(int i = 31; bitstring > 0; i--) {
      if((bitstring >> i) > 0) {
        bitstring = ((bitstring << 32 - i) >> 32 -i);
        layers.Add(i);
      }
    }
    return layers.ToArray();
  }

  public static Vector2 ClampForce(Vector2 force, Vector2 currVelocity, Vector2 maxVelocityMagnitude, Rigidbody2D body) {
    Vector2 rst = new Vector2();
    for(int i = 0; i < 2; i++) {
      if(force[i] * currVelocity[i] >= 0) {
        // Force same direction as currVelocity or 0 force
        rst[i] = Math.Min(force[i], body.mass * (maxVelocityMagnitude[i] - currVelocity[i]));
      } else {
        rst[i] = Math.Max(force[i], body.mass * (maxVelocityMagnitude[i] + currVelocity[i]));
      }
    }

    return rst;
  }

}

