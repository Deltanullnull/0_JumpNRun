using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeScript : MonoBehaviour
{

    public static LifeScript Instance;
    
    Image background;

    private float currentHealth = 1f;
    private float lastHealth = 1f;

	// Use this for initialization
	void Awake() {
        if (Instance == null)
            Instance = this;
        background = GetComponent<Image>();	
	}

    public void ResetHealth()
    {
        currentHealth = lastHealth = 1f;

        background.fillAmount = currentHealth;
    }
	
    public void SetHealth(float healthPerc)
    {
        currentHealth = healthPerc;
        
        StartCoroutine("DropHealth");
    }

	IEnumerator DropHealth()
    {
        float health = lastHealth;

        while (Mathf.Abs(health - currentHealth) > 0.01f)
        {
            health = Mathf.Lerp(health, currentHealth, Time.deltaTime * 5);

            background.fillAmount = health;
            yield return null;
        }

        background.fillAmount = currentHealth;
        lastHealth = currentHealth;

        yield return null;
    }
}
