using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarFill : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image overHealBar;
    [SerializeField] private Health agentHealthScript;
    [SerializeField] private float MaxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool overHeal;
    [SerializeField] private float overHealMax;
    [SerializeField] private float overHealAmount;
    [SerializeField] private float fillAmount = 1f;
    [SerializeField] private float overFillAmount = 0f;
    [SerializeField] private bool overHealLasting;
    [SerializeField] private float decreaseRate = 2f;
    [SerializeField] private bool decreasingHealth = false;
    // Start is called before the first frame update
    void Start()
    {
        MaxHealth = agentHealthScript.maxHealth;
        overHealAmount = agentHealthScript.overHealAmount;
        currentHealth = agentHealthScript.health;
        overHeal = agentHealthScript.overHeal;
        overHealBar.fillAmount = overFillAmount;
        overHealMax = agentHealthScript.overHealMax;
        overHealLasting = agentHealthScript.overHealLasting;
    }

    // Update is called once per frame
    void Update()
    {
        if (!overHealLasting && currentHealth > MaxHealth)
        {
            if (!decreasingHealth)
            {
                decreasingHealth = true;
                StartCoroutine(decreaseOverHeal());
            }
        }
    }

    public void updateHealthUI(float newHealth)
    {
        currentHealth = agentHealthScript.health;

        if (!overHeal || newHealth <= MaxHealth)
        {
            overHealBar.fillAmount = 0f;
            fillAmount = newHealth / MaxHealth;
            healthBar.fillAmount = fillAmount;

        }

        else
        {
            overHealAmount = newHealth - MaxHealth;
            agentHealthScript.overHealAmount = overHealAmount;
            overFillAmount = overHealAmount / overHealMax;
            overHealBar.fillAmount = overFillAmount;
            healthBar.fillAmount = 1f;
        }



    }

    IEnumerator decreaseOverHeal()
    {
        yield return new WaitForSeconds(1f);
        if(overHealAmount > decreaseRate)
        {
            agentHealthScript.RemoveHealth(decreaseRate);
        }

        else
        {
            overHealAmount = 0;
            agentHealthScript.health = MaxHealth;
            agentHealthScript.overHealAmount = 0f;
            updateHealthUI(agentHealthScript.health);
        }


        decreasingHealth = false;
    }

}
