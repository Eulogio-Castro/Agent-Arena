using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTargets : MonoBehaviour
{
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform leftRifle;
    [SerializeField] private Transform rightRifle;
    [SerializeField] private Transform leftPistol;
    [SerializeField] private Transform rightPistol;
    [SerializeField] private int currentWeapon;

    // Start is called before the first frame update
    void Start()
    {
        rightHand.localPosition = rightRifle.localPosition;
        leftHand.localPosition = leftRifle.localPosition;
    }

    // Update is called once per frame
    public void SetHands(int weaponType)
    {
        if (weaponType == 0)
        {
            rightHand.localPosition = rightRifle.localPosition;
            leftHand.localPosition = leftRifle.localPosition;
        }

        else 
        {
            rightHand.localPosition = rightPistol.localPosition;
            leftHand.localPosition = leftPistol.localPosition;
        }
    }
}
