using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAnts : MonoBehaviour
{

    [SerializeField] GameObject ant;
    [SerializeField] int noOfAnts;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < noOfAnts; i++) {
            Instantiate(ant);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
