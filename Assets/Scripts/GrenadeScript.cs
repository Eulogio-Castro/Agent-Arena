using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GrenadeScript : NetworkBehaviour
{
    [SerializeField] private float maxDamage;
    [SerializeField] private float minDamage;
    [SerializeField] private float blastRadius;
    [SerializeField] private float maxRadius;
    [SerializeField] private float cookTime;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject mesh;
    public KDManager thismanager;
    public int ownerID;
    public KDManager[] KDManagers;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Detonate());
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

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Detonate()
    {
        yield return new WaitForSeconds(cookTime);
        GameObject exposionInstance = Instantiate(explosionPrefab, gameObject.transform.position, gameObject.transform.rotation);
        mesh.SetActive(false);
        handleExplosionEffect(exposionInstance);

        RaycastHit[] hitInfo;
        List<NetworkIdentity> netIDs = new List<NetworkIdentity>();
        hitInfo = Physics.SphereCastAll(transform.position, blastRadius, Vector3.down, .1f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        for(int i = 0; i < hitInfo.Length; i++)
        {
            GameObject hitObject = hitInfo[i].collider.gameObject;
            Health healthComp = hitObject.GetComponentInParent<Health>();
            if(healthComp!=null)
            {
                if (healthComp.health <= 0) { continue; }
                if (netIDs.Contains(hitObject.GetComponentInParent<NetworkIdentity>()))
                {
                    continue;
                }



                netIDs.Add(hitObject.GetComponentInParent<NetworkIdentity>());

                float distance = Vector3.Distance(hitObject.transform.position, gameObject.transform.position);
                Debug.Log(distance);
                if (distance < maxRadius)
                {
                    if (maxDamage > healthComp.health)
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
                    healthComp.RemoveHealth(maxDamage);
                    Debug.Log(maxDamage);
                }

                else
                {
                    float damageDone = (distance / maxRadius) + minDamage;
                    if (maxDamage > healthComp.health)
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
                    healthComp.RemoveHealth(damageDone);
                    Debug.Log((distance / maxRadius) + minDamage);
                }
            }


        }

        NetworkServer.Destroy(gameObject);

    }


    [ClientRpc]
    void handleExplosionEffect(GameObject explosion)
    {
        NetworkServer.Spawn(explosion, gameObject);
    }




}
