using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{

    [SerializeField] private NetworkWeaponSystem weaponSystem;
    [SerializeField] private Image ability1Counter;
    [SerializeField] private Image ability2Counter;
    [SerializeField] private float ability1Fill;
    [SerializeField] private float ability2Fill;
    [SerializeField] private float ability1StartTime;
    [SerializeField] private float ability2StartTime;
    [SerializeField] private float ability1Cooldown;
    [SerializeField] private float ability2Cooldown;


    // Start is called before the first frame update
    void Start()
    {
        weaponSystem = GetComponentInParent<NetworkWeaponSystem>();
        ability1Cooldown = weaponSystem.GetAbility1Cooldown();
        ability2Cooldown = weaponSystem.GetAbility2Cooldown();
        ability1Fill = 0f;
        ability2Fill = 0f;

    }

    // Update is called once per frame
    void Update()
    {
        if (ability1Fill != 0f)
        {
            float timeElapsed = Time.time - ability1StartTime;

            if (timeElapsed < ability1Cooldown)
            {
                ability1Fill = (ability1Cooldown - timeElapsed) / ability1Cooldown;
            }

            else
            {
                ability1Fill = 0f;
            }
        }

        if (ability2Fill != 0f)
        {
            float timeElapsed = Time.time - ability2StartTime;

            if (timeElapsed < ability2Cooldown)
            {
                ability2Fill = (ability2Cooldown - timeElapsed) / ability2Cooldown;
            }

            else
            {
                ability2Fill = 0f;
            }
        }


        ability1Counter.fillAmount = ability1Fill;
        ability2Counter.fillAmount = ability2Fill;

    }


    public void Ability1Start()
    {
        ability1StartTime = Time.time;
        ability1Fill = 1f;
    }

    public void Ability2Start()
    {
        ability2StartTime = Time.time;
        ability2Fill = 1f;
    }
}
