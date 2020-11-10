using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHealth : MonoBehaviour
{
    public static int health = 100;

    public AudioSource _damaged;
    public AudioSource _dead;
    public bool isActive;

    UIManager uiManager;
    public GameObject deathMenuUI;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            DamagePlayer(12);
        }
        if (health == 0)
        {

            Kill();

        }
    }
    public void DamagePlayer(int _damageAmount)
    {

        //subtract health6
        //_damaged.Play();
        health -= _damageAmount;
        UnityEngine.Debug.Log("Damaged player");
        if (health < 0)
        {
            health = 0;
            _dead.Play();
        }
        //update slider
        uiManager.UpdateHealthSlider();
    }
   
    public void Kill()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isActive = true;

        Pause();
        UnityEngine.Debug.Log("player set inactive, ur dead");

    }
    public void Pause()
    {
        if (isActive == true)
        {
            deathMenuUI.SetActive(true);
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
    }
}
