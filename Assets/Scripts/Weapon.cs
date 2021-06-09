using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float magazineSize;
    [SerializeField] private float maxAmmo;
    [SerializeField] private float reloadTime;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private float lowDamage;
    [SerializeField] private float highDamage;
    [SerializeField] private bool isBurst;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetBulletPrefab()
    {
        return bulletPrefab;
    }

    public float GetBulletSpeed()
    {
        return bulletSpeed;
    }

    public float GetFireRate()
    {
        return fireRate;
    }

    public ParticleSystem GetParticleSystem()
    {
        return muzzleFlash;
    }

    public int GetDamage()
    {
        int damage = Mathf.FloorToInt(Random.Range(lowDamage, highDamage));
        return damage;
    }
    
    public bool GetIsBurst()
    {
        return isBurst;
    }
}
