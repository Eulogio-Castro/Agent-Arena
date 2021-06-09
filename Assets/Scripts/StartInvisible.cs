using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartInvisible : MonoBehaviour
{
    private MeshRenderer mesh;
    public float delayTime;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        mesh.enabled = false;
        StartCoroutine("DelayVisibility");
    }


    IEnumerator DelayVisibility()
    {
        yield return new WaitForSeconds(delayTime);
        mesh.enabled = true;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
