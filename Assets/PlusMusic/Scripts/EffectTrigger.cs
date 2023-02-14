using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectTrigger : MonoBehaviour
{
    public string effect = "CutOff";
    public float value;
    public float duration;
    private void OnTriggerEnter(Collider other)
    {
        PlusMusic_DJ.Instance.SetMixerSetting(effect, value, duration);
    }
}
