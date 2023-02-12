using UnityEngine;
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
}
