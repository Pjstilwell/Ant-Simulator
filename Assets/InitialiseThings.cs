using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialiseThings : MonoBehaviour
{

    [SerializeField] GameObject food;

    // Start is called before the first frame update
    void Start()
    {
        GameObject newFood = Instantiate(food, new Vector3(-5, 0.5f, 0), Quaternion.identity);
        // newFood.tag = "food";

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
