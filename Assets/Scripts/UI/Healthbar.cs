using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void SetHealth(int health)
    {
        if (slider == null) return;

        if (health > slider.maxValue)
        {
            slider.maxValue = health;
            slider.value = health;
        }
        else if (health < 0)
        {
            slider.value = 0;
        }
        else
        {
            slider.value = health;
        }

    }
}
