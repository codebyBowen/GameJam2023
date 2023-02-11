using UnityEngine;
using System.Collections;

public class Sensor_HeroKnight : MonoBehaviour {

    private int m_ColCount = 0;

    private float m_DisableTimer;

    private int layer_env;

    private void Start() {
      layer_env = LayerMask.NameToLayer("Environment");
      Debug.Assert(layer_env != -1);
    }

    private void OnEnable()
    {
        m_ColCount = 0;
    }

    public bool State()
    {
        if (m_DisableTimer > 0)
            return false;
        return m_ColCount > 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //TODO: handle this properly
        if(other.gameObject.layer == layer_env) {
          m_ColCount++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //TODO: handle this properly
        if(other.gameObject.layer == layer_env) {
          m_ColCount--;
        }
    }

    void FixedUpdate()
    {
        m_DisableTimer -= Time.deltaTime;
    }

    public void Disable(float duration)
    {
        m_DisableTimer = duration;
    }
}
