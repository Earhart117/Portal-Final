using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public Slider healthSlider;
    public Slider staminaSlider;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void UpdateHealthSlider()
    {
        //set slider value = to health
        healthSlider.value = PlayerHealth.health;
    }

    public void UpdateStaminaSlider()
    {
        //set stamina slider
        staminaSlider.value = FPSController.stamina;
    }
}
