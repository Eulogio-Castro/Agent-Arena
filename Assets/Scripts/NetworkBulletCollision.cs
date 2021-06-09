using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkBulletCollision : NetworkBehaviour
{
    [SerializeField] public float bulletDamage;
    [SerializeField] private float castLength = 0.1f;
    [SerializeField] private GameObject hitPrefab;
    public KDManager thismanager;
    public int ownerID;
    private Vector3 hitPosition;
    private Vector3 hitNormal;
    public KDManager[] KDManagers;
    NetworkConnection bulletOwner;

    private void Start()
    {
        StartCoroutine(serverDelete(gameObject, 2.0f));

        if (ownerID != 0)
        {
            KDManagers = GameObject.FindObjectsOfType<KDManager>();

            foreach (KDManager manager in KDManagers)
            {
                if (manager.GetComponent<NetworkIdentity>().netId == ownerID)
                {
                    thismanager = manager;
                }
            }
        }
    }


    IEnumerator serverDelete(GameObject destroyThis, float delay)
    {
        yield return new WaitForSeconds(delay);
        NetworkServer.Destroy(destroyThis);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if( ownerID == 0) { return; }
        GameObject hitObj = collision.gameObject;

        Health healthComp = hitObj.GetComponentInParent<Health>();
        if (healthComp != null)
        {
            if(healthComp.health <= 0) { return;  }


            if (bulletDamage > healthComp.health)
            {
                if (healthComp.netId != ownerID)
                {
                    thismanager.AddKill();
                }

                if (healthComp.netId == ownerID)
                {
                    thismanager.SubtractKill();
                }
            }
            healthComp.RemoveHealth(bulletDamage);
        }


        hitPosition = collision.GetContact(0).point;
        hitNormal = collision.GetContact(0).normal;
        HandleHit();
        
    }

    [Command]
    private void HandleHit()
    {
        GameObject hitInstance = Instantiate(hitPrefab, hitPosition, Quaternion.FromToRotation(Vector3.forward, hitNormal));
        NetworkServer.Spawn(hitInstance);
        NetworkServer.Destroy(gameObject);
    }

}



