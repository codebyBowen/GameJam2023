using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{

	public Slider slider;
	// public Gradient gradient;
	public Image fill;

	public void SetMaxHealth(int health)
	{
		slider.maxValue = health;
		slider.value = health;
	}

    public void SetHealth(int health)
	{
		slider.value = health;
	}

}
