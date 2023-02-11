using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : HealthBar
{

	public Gradient gradient;
	public Image fill;

	public void SetMaxHealth(float health)
	{
                base.SetMaxHealth(health);
		fill.color = gradient.Evaluate(1f);
	}

    public void SetHealth(float health)
	{
                base.SetHealth(health);
		fill.color = gradient.Evaluate(slider.normalizedValue);
	}

}
