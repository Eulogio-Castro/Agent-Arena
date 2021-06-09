using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;
using System;
using Steamworks;

public class PlayerNameInput : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private InputField nameInputField = null;




    [SerializeField] private Button continueButton = null;
    // Start is called before the first frame update

    public static string DisplayName { get; private set; }

    private const string PlayerPrefsNameKey = "PlayerName";

    private void Start() => SetUpInputField();

    private void SetUpInputField()
    {

        if(!PlayerPrefs.HasKey(PlayerPrefsNameKey)) { return; }
        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);

        nameInputField.text = defaultName;

        SetPlayerName(defaultName);
    }


    public void SetPlayerName(string name)
    {
        name = nameInputField.text;
        continueButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        DisplayName = nameInputField.text;

        PlayerPrefs.SetString(PlayerPrefsNameKey, DisplayName);
    }

}
