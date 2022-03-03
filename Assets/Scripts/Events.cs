using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Events : MonoBehaviour
{
    public GameObject creature;
    public GameObject food;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 50; i++)
        {
            Instantiate(creature, new Vector3(1, 2.5f, 1), Quaternion.identity);
        }
        Instantiate(food, new Vector3(1, 5, 1), Quaternion.identity);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
