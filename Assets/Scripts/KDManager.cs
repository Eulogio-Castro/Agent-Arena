using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using System;

public class KDManager : NetworkBehaviour
{

    [SyncVar(hook = nameof(HandleKillsChanged))]
    public float killCount = 0f;

    [SerializeField] private int deathCount = 0;
    [SerializeField] private TMP_Text killCountText;
    [SerializeField] private TMP_Text deathCountText;

    public static event Action OnKillsChanged;


    public void HandleKillsChanged(float oldValue, float newValue) => ChangeKills();


    // Start is called before the first frame update
    void Start()
    {
        killCount = 0;
        deathCount = 0;
    }

    private void ChangeKills()
    {
        Debug.Log("HandleKillsChanged");
        OnKillsChanged?.Invoke();
        killCountText.text = killCount.ToString();

    }

    public void AddKill()
    {
        killCount += 1;
        killCountText.text = killCount.ToString();
    }

    public void SubtractKill()
    {
        killCount -= 1;
        killCountText.text = killCount.ToString();
    }

    public void AddDeath()
    {
        deathCount += 1;
        deathCountText.text = deathCount.ToString();
    }

 
}
