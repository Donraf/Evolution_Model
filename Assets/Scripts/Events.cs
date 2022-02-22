using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Events : MonoBehaviour
{
    public GameObject creature;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 20; i++)
        {
            Instantiate(creature, new Vector3(1, 2.5f, 1), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
